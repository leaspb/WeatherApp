namespace WeatherApp.UnitTests;

using System.Net;
using Microsoft.Extensions.Caching.Memory;
using WeatherApp.Api.Configuration;
using WeatherApp.Api.Services;

public sealed class WeatherServiceTests
{
    [Fact]
    public async Task GetWeatherAsync_ReturnsRemainingCurrentDayHoursAndNextDayHours()
    {
        var handler = new StubHttpMessageHandler();
        var service = CreateService(handler);

        var result = await service.GetWeatherAsync(CancellationToken.None);

        Assert.Equal("Moscow", result.Location.Name);
        Assert.Equal(3, result.Daily.Count);
        Assert.Equal(27, result.Hourly.Count);
        Assert.Equal(new DateTimeOffset(2024, 9, 17, 21, 0, 0, TimeSpan.FromHours(3)), result.Hourly.First().Time);
        Assert.Equal(new DateTimeOffset(2024, 9, 18, 23, 0, 0, TimeSpan.FromHours(3)), result.Hourly.Last().Time);
        Assert.Equal("https://cdn.weatherapi.com/weather/64x64/day/116.png", result.Current.IconUrl);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task GetWeatherAsync_UsesCacheForRepeatedCalls()
    {
        var handler = new StubHttpMessageHandler();
        var service = CreateService(handler);

        _ = await service.GetWeatherAsync(CancellationToken.None);
        _ = await service.GetWeatherAsync(CancellationToken.None);

        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task GetWeatherAsync_RetriesTransientFailuresBeforeSucceeding()
    {
        var handler = new StubHttpMessageHandler();
        handler.EnqueueTransientStatusCode(HttpStatusCode.ServiceUnavailable);
        handler.EnqueueTransientStatusCode(HttpStatusCode.ServiceUnavailable);

        var service = CreateService(handler, o => o.RetryCount = 2);

        var result = await service.GetWeatherAsync(CancellationToken.None);

        Assert.Equal("Moscow", result.Location.Name);
        Assert.Equal(4, handler.RequestCount);
    }

    [Fact]
    public async Task GetWeatherAsync_OpensCircuitBreakerAfterConfiguredFailures()
    {
        var handler = new StubHttpMessageHandler();
        handler.EnqueueTransientStatusCode(HttpStatusCode.ServiceUnavailable);
        handler.EnqueueTransientStatusCode(HttpStatusCode.ServiceUnavailable);
        handler.EnqueueTransientStatusCode(HttpStatusCode.ServiceUnavailable);
        handler.EnqueueTransientStatusCode(HttpStatusCode.ServiceUnavailable);

        var service = CreateService(handler, o =>
        {
            o.RetryCount = 0;
            o.CircuitBreakerFailureThreshold = 2;
            o.CircuitBreakerDurationSeconds = 30;
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
        handler.EnqueueStatusCode(HttpStatusCode.Unauthorized);

        var service = CreateService(handler, o =>
        {
            o.RetryCount = 0;
            o.CircuitBreakerFailureThreshold = 1;
            o.CircuitBreakerDurationSeconds = 30;
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

        var service = CreateService(handler, o =>
        {
            o.RetryCount = 0;
            o.RequestTimeoutSeconds = 1;
            o.CircuitBreakerFailureThreshold = 5;
        });

        await Assert.ThrowsAsync<TimeoutException>(
            () => service.GetWeatherAsync(CancellationToken.None));
    }

    private static WeatherService CreateService(
        StubHttpMessageHandler handler,
        Action<WeatherApiOptions>? configure = null)
    {
        var options = new WeatherApiOptions
        {
            ApiKey = "test-key",
            BaseUrl = "http://localhost/",
        };
        configure?.Invoke(options);
        return new WeatherService(
            new HttpClient(handler) { BaseAddress = new Uri("http://localhost") },
            new MemoryCache(new MemoryCacheOptions()),
            new WeatherApiCircuitBreaker(),
            new WeatherCacheRefreshCoordinator(),
            options);
    }
}
