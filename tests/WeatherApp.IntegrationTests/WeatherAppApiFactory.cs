using WeatherApp.Api.Services.Weather;

namespace WeatherApp.IntegrationTests;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public sealed class WeatherAppApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(
            services =>
            {
                services.AddSingleton<IWeatherService, FakeWeatherService>();
            });
    }
}
