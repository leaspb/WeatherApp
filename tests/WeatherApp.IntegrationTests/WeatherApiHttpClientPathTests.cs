namespace WeatherApp.IntegrationTests;

using System.Net;
using System.Net.Http.Json;
using WeatherApp.Api.Contracts;

public sealed class WeatherApiHttpClientPathTests
{
    [Fact]
    public async Task GetWeather_UsesRealWeatherServicePath_AndReturnsNormalizedPayload()
    {
        await using var factory = new WeatherApiHttpClientPathFactory();
        factory.MessageHandler.EnqueueJsonResponse("/current.json", WeatherApiPayloads.CurrentWeatherJson);
        factory.MessageHandler.EnqueueJsonResponse("/forecast.json", WeatherApiPayloads.ForecastWeatherJson);

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/weather");
        var payload = await response.Content.ReadFromJsonAsync<WeatherResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Moscow", payload.Location.Name);
        Assert.Equal(2, factory.MessageHandler.RequestCount);
    }

    [Fact]
    public async Task GetWeather_RetriesTransientFailure_AndStillReturnsOk()
    {
        await using var factory = new WeatherApiHttpClientPathFactory();
        factory.MessageHandler.EnqueueStatusCode("/current.json", HttpStatusCode.ServiceUnavailable);
        factory.MessageHandler.EnqueueJsonResponse("/current.json", WeatherApiPayloads.CurrentWeatherJson);
        factory.MessageHandler.EnqueueJsonResponse("/forecast.json", WeatherApiPayloads.ForecastWeatherJson);

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/weather");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, factory.MessageHandler.RequestCount);
    }

    [Fact]
    public async Task GetWeather_ReturnsInternalServerError_ForUnauthorizedProviderResponse()
    {
        await using var factory = new WeatherApiHttpClientPathFactory();
        factory.MessageHandler.EnqueueStatusCode("/current.json", HttpStatusCode.Unauthorized);
        factory.MessageHandler.EnqueueJsonResponse("/forecast.json", WeatherApiPayloads.ForecastWeatherJson);

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/weather");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetWeather_ReturnsServiceUnavailable_WhenProviderTimesOut()
    {
        await using var factory = new WeatherApiHttpClientPathFactory();
        factory.MessageHandler.ResponseDelay = TimeSpan.FromSeconds(2);
        factory.MessageHandler.EnqueueJsonResponse("/current.json", WeatherApiPayloads.CurrentWeatherJson);
        factory.MessageHandler.EnqueueJsonResponse("/forecast.json", WeatherApiPayloads.ForecastWeatherJson);

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/weather");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
