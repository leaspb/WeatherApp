namespace WeatherApp.IntegrationTests;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using WeatherApp.Api.Configuration;

public sealed class WeatherApiHttpClientPathFactory : WebApplicationFactory<Program>
{
    public ScriptedWeatherApiMessageHandler MessageHandler { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(
            services =>
            {
                services.RemoveAll<IHttpClientFactory>();

                services.AddSingleton(MessageHandler);
                services.AddSingleton<IHttpClientFactory, StubHttpClientFactory>();

                services.PostConfigure<WeatherApiOptions>(
                    options =>
                    {
                        options.BaseUrl = "http://weather.test/";
                        options.ApiKey = "test-key";
                        options.RetryCount = 1;
                        options.RequestTimeoutSeconds = 1;
                        options.CacheDurationMinutes = 1;
                        options.CircuitBreakerFailureThreshold = 2;
                        options.CircuitBreakerDurationSeconds = 30;
                    });
            });
    }
}
