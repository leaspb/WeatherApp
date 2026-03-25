namespace WeatherApp.Api.Contracts;

public sealed record CurrentWeatherResponse(
    decimal TemperatureC,
    string ConditionText,
    string IconUrl);
