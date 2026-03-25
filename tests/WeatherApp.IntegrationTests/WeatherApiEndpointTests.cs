namespace WeatherApp.IntegrationTests;

using System.Net;
using System.Net.Http.Json;
using WeatherApp.Api.Contracts;

public sealed class WeatherApiEndpointTests : IClassFixture<WeatherAppApiFactory>
{
    private readonly WeatherAppApiFactory factory;

    public WeatherApiEndpointTests(WeatherAppApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWeather_ReturnsNormalizedPayload()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/weather");
        var payload = await response.Content.ReadFromJsonAsync<WeatherResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Moscow", payload.Location.Name);
        Assert.Single(payload.Hourly);
        Assert.Single(payload.Daily);
    }
}
