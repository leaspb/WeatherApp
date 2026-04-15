using WeatherApp.Api.Services.Weather;

namespace WeatherApp.IntegrationTests;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WeatherApp.Api.Services.Weather;

public sealed class WeatherAppApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(
            services =>
            {
                services.PostConfigure<WeatherApiOptions>(
                    options =>
                    {
                        options.ApiKey = "test-key";
                    });
                services.AddSingleton<IWeatherService, FakeWeatherService>();
            });
    }
}
