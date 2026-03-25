namespace WeatherApp.Api.DependencyInjection;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WeatherApp.Api.Abstractions;
using WeatherApp.Api.Configuration;
using WeatherApp.Api.Services;

public static class ServiceCollectionExtensions
{
    private const string WeatherApiHttpClientName = "WeatherApi";

    public static IServiceCollection AddWeatherServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<WeatherApiCircuitBreaker>();
        services.AddSingleton<WeatherCacheRefreshCoordinator>();

        services
            .AddOptions<WeatherApiOptions>()
            .Bind(configuration.GetSection(WeatherApiOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "Weather API key is not configured.")
            .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _), "Weather API base URL is invalid.")
            .Validate(o => o.RequestTimeoutSeconds > 0, "Weather API timeout must be positive.")
            .Validate(o => o.CacheDurationMinutes > 0, "Weather API cache duration must be positive.")
            .Validate(o => o.CircuitBreakerFailureThreshold > 0, "Weather API circuit breaker threshold must be positive.")
            .Validate(o => o.CircuitBreakerDurationSeconds > 0, "Weather API circuit breaker duration must be positive.")
            .ValidateOnStart();

        services.AddHttpClient(
            WeatherApiHttpClientName,
            static (serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<WeatherApiOptions>>().Value;
                httpClient.BaseAddress = new Uri(options.BaseUrl);
            });

        services.AddScoped<IWeatherService>(
            serviceProvider =>
            {
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
                var circuitBreaker = serviceProvider.GetRequiredService<WeatherApiCircuitBreaker>();
                var cacheRefreshCoordinator = serviceProvider.GetRequiredService<WeatherCacheRefreshCoordinator>();
                var options = serviceProvider.GetRequiredService<IOptions<WeatherApiOptions>>().Value;

                return new WeatherService(
                    httpClientFactory.CreateClient(WeatherApiHttpClientName),
                    memoryCache,
                    circuitBreaker,
                    cacheRefreshCoordinator,
                    options);
            });

        return services;
    }
}
