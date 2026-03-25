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

        services
            .AddOptions<WeatherApiOptions>()
            .Bind(configuration.GetSection(WeatherApiOptions.SectionName));

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
                var options = serviceProvider.GetRequiredService<IOptions<WeatherApiOptions>>().Value;

                return new WeatherService(
                    httpClientFactory.CreateClient(WeatherApiHttpClientName),
                    memoryCache,
                    circuitBreaker,
                    options);
            });

        return services;
    }
}
