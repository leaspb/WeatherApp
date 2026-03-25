namespace WeatherApp.Api.Contracts;

public sealed record WeatherResponse(
    WeatherLocationResponse Location,
    CurrentWeatherResponse Current,
    IReadOnlyList<HourlyForecastResponse> Hourly,
    IReadOnlyList<DailyForecastResponse> Daily);
