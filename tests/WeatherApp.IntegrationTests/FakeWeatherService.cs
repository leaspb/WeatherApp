namespace WeatherApp.IntegrationTests;

using WeatherApp.Api.Abstractions;
using WeatherApp.Api.Contracts;

public sealed class FakeWeatherService : IWeatherService
{
    public Task<WeatherResponse> GetWeatherAsync(CancellationToken cancellationToken)
    {
        var response = new WeatherResponse(
            new WeatherLocationResponse("Moscow", new DateTimeOffset(2024, 9, 17, 20, 30, 0, TimeSpan.FromHours(3))),
            new CurrentWeatherResponse(16.4m, "Cloudy", "https://cdn.weatherapi.com/weather/64x64/day/116.png"),
            [
                new HourlyForecastResponse(
                    new DateTimeOffset(2024, 9, 17, 21, 0, 0, TimeSpan.FromHours(3)),
                    14.1m,
                    "Clear",
                    "https://cdn.weatherapi.com/weather/64x64/night/113.png"),
            ],
            [
                new DailyForecastResponse(
                    new DateOnly(2024, 9, 17),
                    18.0m,
                    10.0m,
                    "Partly cloudy",
                    "https://cdn.weatherapi.com/weather/64x64/day/116.png"),
            ]);

        return Task.FromResult(response);
    }
}
