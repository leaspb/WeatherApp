using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using WeatherApp.Api.Contracts;

namespace WeatherApp.Api.Services.Weather;

public sealed class WeatherService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    WeatherApiCircuitBreaker circuitBreaker,
    SemaphoreSlim refreshLock,
    WeatherApiOptions options) : IWeatherService
{
    private const string WeatherCacheKey = "weather:moscow";
    private static readonly TimeSpan MoscowUtcOffset = TimeSpan.FromHours(3);

    public async Task<WeatherResponse> GetWeatherAsync(CancellationToken cancellationToken)
    {
        if (memoryCache.TryGetValue(WeatherCacheKey, out WeatherResponse? cachedWeather) && cachedWeather is not null)
        {
            return cachedWeather;
        }

        await refreshLock.WaitAsync(cancellationToken);

        try
        {
            if (memoryCache.TryGetValue(WeatherCacheKey, out cachedWeather) && cachedWeather is not null)
            {
                return cachedWeather;
            }

            circuitBreaker.EnsureRequestAllowed(DateTimeOffset.UtcNow);

            try
            {
                var currentResponseTask = GetJsonWithRetryAsync(BuildCurrentRequestUri(options), cancellationToken);
                var forecastResponseTask = GetJsonWithRetryAsync(BuildForecastRequestUri(options), cancellationToken);

                await Task.WhenAll(currentResponseTask, forecastResponseTask);

                var response = MapWeatherResponse(
                    await currentResponseTask,
                    await forecastResponseTask);

                memoryCache.Set(
                    WeatherCacheKey,
                    response,
                    TimeSpan.FromMinutes(options.CacheDurationMinutes));

                circuitBreaker.RecordSuccess();

                return response;
            }
            catch (Exception exception) when (IsTransient(exception))
            {
                circuitBreaker.RecordFailure(
                    DateTimeOffset.UtcNow,
                    options.CircuitBreakerFailureThreshold,
                    TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds));

                throw;
            }
        }
        finally
        {
            refreshLock.Release();
        }
    }

    private async Task<JsonDocument> GetJsonWithRetryAsync(
        string requestUri,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await GetJsonAsync(requestUri, cancellationToken);
            }
            catch (Exception exception) when (IsTransient(exception) && attempt < options.RetryCount)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(150 * (attempt + 1)), cancellationToken);
            }
        }
    }

    private async Task<JsonDocument> GetJsonAsync(
        string requestUri,
        CancellationToken cancellationToken)
    {
        using var timeoutCancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(options.RequestTimeoutSeconds));

        HttpResponseMessage response;

        try
        {
            response = await httpClient.GetAsync(requestUri, timeoutCancellationTokenSource.Token);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Weather API request timed out.", exception);
        }

        using (response)
        {
            if (IsTransientStatusCode(response.StatusCode))
            {
                throw new HttpRequestException(
                    $"Weather API returned transient status code {(int)response.StatusCode}.");
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken)
                ?? throw new JsonException("Weather API returned an empty payload.");
        }
    }

    private static WeatherResponse MapWeatherResponse(
        JsonDocument currentDocument,
        JsonDocument forecastDocument)
    {
        using (currentDocument)
        using (forecastDocument)
        {
            var currentRoot = currentDocument.RootElement;
            var forecastRoot = forecastDocument.RootElement;

            var location = currentRoot.GetProperty("location");
            var current = currentRoot.GetProperty("current");
            var forecastDays = forecastRoot
                .GetProperty("forecast")
                .GetProperty("forecastday");

            var localTime = ParseDateTimeOffset(location.GetProperty("localtime").GetString());

            return new WeatherResponse(
                new WeatherLocationResponse(
                    location.GetProperty("name").GetString() ?? "Moscow",
                    localTime),
                new CurrentWeatherResponse(
                    GetDecimal(current, "temp_c"),
                    current.GetProperty("condition").GetProperty("text").GetString() ?? string.Empty,
                    NormalizeIconUrl(current.GetProperty("condition").GetProperty("icon").GetString())),
                MapHourly(forecastDays, localTime),
                MapDaily(forecastDays));
        }
    }

    private static IReadOnlyList<HourlyForecastResponse> MapHourly(
        JsonElement forecastDays,
        DateTimeOffset localTime)
    {
        var today = DateOnly.FromDateTime(localTime.DateTime);
        var tomorrow = today.AddDays(1);
        var hourly = new List<HourlyForecastResponse>();

        foreach (var forecastDay in forecastDays.EnumerateArray())
        {
            var date = DateOnly.ParseExact(
                forecastDay.GetProperty("date").GetString() ?? string.Empty,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

            if (date != today && date != tomorrow)
            {
                continue;
            }

            foreach (var hour in forecastDay.GetProperty("hour").EnumerateArray())
            {
                var hourTime = ParseDateTimeOffset(hour.GetProperty("time").GetString());
                var hourDate = DateOnly.FromDateTime(hourTime.DateTime);
                var isRemainingHourInCurrentDay = hourDate == today && hourTime >= localTime;
                var isHourInNextDay = hourDate == tomorrow;

                if (!isRemainingHourInCurrentDay && !isHourInNextDay)
                {
                    continue;
                }

                hourly.Add(
                    new HourlyForecastResponse(
                        hourTime,
                        GetDecimal(hour, "temp_c"),
                        hour.GetProperty("condition").GetProperty("text").GetString() ?? string.Empty,
                        NormalizeIconUrl(hour.GetProperty("condition").GetProperty("icon").GetString())));
            }
        }

        return hourly;
    }

    private static IReadOnlyList<DailyForecastResponse> MapDaily(JsonElement forecastDays)
    {
        var daily = new List<DailyForecastResponse>();

        foreach (var forecastDay in forecastDays.EnumerateArray())
        {
            var day = forecastDay.GetProperty("day");

            daily.Add(
                new DailyForecastResponse(
                    DateOnly.ParseExact(
                        forecastDay.GetProperty("date").GetString() ?? string.Empty,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture),
                    GetDecimal(day, "maxtemp_c"),
                    GetDecimal(day, "mintemp_c"),
                    day.GetProperty("condition").GetProperty("text").GetString() ?? string.Empty,
                    NormalizeIconUrl(day.GetProperty("condition").GetProperty("icon").GetString())));
        }

        return daily;
    }

    private static string BuildCurrentRequestUri(WeatherApiOptions weatherApiOptions) =>
        FormattableString.Invariant(
            $"current.json?key={weatherApiOptions.ApiKey}&q={weatherApiOptions.Latitude},{weatherApiOptions.Longitude}");

    private static string BuildForecastRequestUri(WeatherApiOptions weatherApiOptions) =>
        FormattableString.Invariant(
            $"forecast.json?key={weatherApiOptions.ApiKey}&q={weatherApiOptions.Latitude},{weatherApiOptions.Longitude}&days=3");

    private static string NormalizeIconUrl(string? iconUrl)
    {
        if (string.IsNullOrWhiteSpace(iconUrl))
        {
            return string.Empty;
        }

        return iconUrl.StartsWith("//", StringComparison.Ordinal)
            ? $"https:{iconUrl}"
            : iconUrl;
    }

    private static decimal GetDecimal(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetDecimal();

    private static bool IsTransient(Exception exception) =>
        exception switch
        {
            TimeoutException => true,
            HttpRequestException httpRequestException when httpRequestException.StatusCode is null => true,
            HttpRequestException httpRequestException when
                httpRequestException.StatusCode is { } statusCode && IsTransientStatusCode(statusCode) => true,
            _ => false,
        };

    private static bool IsTransientStatusCode(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.RequestTimeout
        || statusCode == HttpStatusCode.TooManyRequests
        || (int)statusCode >= 500;

    private static DateTimeOffset ParseDateTimeOffset(string? value) =>
        new(
            DateTime.ParseExact(
                value ?? string.Empty,
                "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None),
            MoscowUtcOffset);
}
