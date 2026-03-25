namespace WeatherApp.UnitTests;

using Microsoft.Extensions.Caching.Memory;
using WeatherApp.Api.Configuration;
using WeatherApp.Api.Services;

public sealed class WeatherServiceTests
{
    [Fact]
    public async Task GetWeatherAsync_ReturnsRemainingCurrentDayHoursAndNextDayHours()
    {
        var handler = new StubHttpMessageHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost"),
        };

        var service = new WeatherService(
            httpClient,
            new MemoryCache(new MemoryCacheOptions()),
            new WeatherApiCircuitBreaker(),
            new WeatherApiOptions
            {
                ApiKey = "test-key",
                BaseUrl = "http://localhost/",
                Latitude = 55.7558m,
                Longitude = 37.6173m,
            });

        var result = await service.GetWeatherAsync(CancellationToken.None);

        Assert.Equal("Moscow", result.Location.Name);
        Assert.Equal(3, result.Daily.Count);
        Assert.Equal(27, result.Hourly.Count);
        Assert.Equal(new DateTime(2024, 9, 17, 21, 0, 0), result.Hourly.First().Time);
        Assert.Equal(new DateTime(2024, 9, 18, 23, 0, 0), result.Hourly.Last().Time);
        Assert.Equal("https://cdn.weatherapi.com/weather/64x64/day/116.png", result.Current.IconUrl);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task GetWeatherAsync_UsesCacheForRepeatedCalls()
    {
        var handler = new StubHttpMessageHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost"),
        };

        var service = new WeatherService(
            httpClient,
            new MemoryCache(new MemoryCacheOptions()),
            new WeatherApiCircuitBreaker(),
            new WeatherApiOptions
            {
                ApiKey = "test-key",
                BaseUrl = "http://localhost/",
                CacheDurationMinutes = 5,
            });

        _ = await service.GetWeatherAsync(CancellationToken.None);
        _ = await service.GetWeatherAsync(CancellationToken.None);

        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task GetWeatherAsync_RetriesTransientFailuresBeforeSucceeding()
    {
        var handler = new StubHttpMessageHandler();
        handler.EnqueueTransientStatusCode(System.Net.HttpStatusCode.ServiceUnavailable);
        handler.EnqueueTransientStatusCode(System.Net.HttpStatusCode.ServiceUnavailable);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost"),
        };

        var service = new WeatherService(
            httpClient,
            new MemoryCache(new MemoryCacheOptions()),
            new WeatherApiCircuitBreaker(),
            new WeatherApiOptions
            {
                ApiKey = "test-key",
                BaseUrl = "http://localhost/",
                RetryCount = 2,
            });

        var result = await service.GetWeatherAsync(CancellationToken.None);

        Assert.Equal("Moscow", result.Location.Name);
        Assert.Equal(4, handler.RequestCount);
    }

    [Fact]
    public async Task GetWeatherAsync_OpensCircuitBreakerAfterConfiguredFailures()
    {
        var handler = new StubHttpMessageHandler();
        handler.EnqueueTransientStatusCode(System.Net.HttpStatusCode.ServiceUnavailable);
        handler.EnqueueTransientStatusCode(System.Net.HttpStatusCode.ServiceUnavailable);
        handler.EnqueueTransientStatusCode(System.Net.HttpStatusCode.ServiceUnavailable);
        handler.EnqueueTransientStatusCode(System.Net.HttpStatusCode.ServiceUnavailable);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost"),
        };

        var service = new WeatherService(
            httpClient,
            new MemoryCache(new MemoryCacheOptions()),
            new WeatherApiCircuitBreaker(),
            new WeatherApiOptions
            {
                ApiKey = "test-key",
                BaseUrl = "http://localhost/",
                RetryCount = 0,
                CircuitBreakerFailureThreshold = 2,
                CircuitBreakerDurationSeconds = 30,
            });

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetWeatherAsync(CancellationToken.None));
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetWeatherAsync(CancellationToken.None));
        await Assert.ThrowsAsync<WeatherCircuitBreakerOpenException>(() => service.GetWeatherAsync(CancellationToken.None));

        Assert.Equal(4, handler.RequestCount);
    }

    [Fact]
    public async Task GetWeatherAsync_DoesNotOpenCircuitBreakerForUnauthorizedResponse()
    {
        var handler = new StubHttpMessageHandler();
        handler.EnqueueStatusCode(System.Net.HttpStatusCode.Unauthorized);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost"),
        };

        var service = new WeatherService(
            httpClient,
            new MemoryCache(new MemoryCacheOptions()),
            new WeatherApiCircuitBreaker(),
            new WeatherApiOptions
            {
                ApiKey = "test-key",
                BaseUrl = "http://localhost/",
                RetryCount = 0,
                CircuitBreakerFailureThreshold = 1,
                CircuitBreakerDurationSeconds = 30,
            });

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetWeatherAsync(CancellationToken.None));

        var result = await service.GetWeatherAsync(CancellationToken.None);

        Assert.Equal("Moscow", result.Location.Name);
        Assert.Equal(4, handler.RequestCount);
    }

    [Fact]
    public async Task GetWeatherAsync_ThrowsTimeoutException_WhenRequestExceedsTimeout()
    {
        var handler = new StubHttpMessageHandler
        {
            ResponseDelay = TimeSpan.FromMilliseconds(1200),
        };

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost"),
        };

        var service = new WeatherService(
            httpClient,
            new MemoryCache(new MemoryCacheOptions()),
            new WeatherApiCircuitBreaker(),
            new WeatherApiOptions
            {
                ApiKey = "test-key",
                BaseUrl = "http://localhost/",
                RetryCount = 0,
                RequestTimeoutSeconds = 1,
                CircuitBreakerFailureThreshold = 5,
            });

        await Assert.ThrowsAsync<TimeoutException>(
            () => service.GetWeatherAsync(CancellationToken.None));
    }
}
