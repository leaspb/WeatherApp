namespace WeatherApp.IntegrationTests;

public sealed class StubHttpClientFactory(ScriptedWeatherApiMessageHandler messageHandler) : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new HttpClient(messageHandler)
        {
            BaseAddress = new Uri("http://weather.test/"),
        };
    }
}
