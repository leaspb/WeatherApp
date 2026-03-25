namespace WeatherApp.Api.Contracts;

public sealed record HourlyForecastResponse(
    DateTime Time,
    decimal TemperatureC,
    string ConditionText,
    string IconUrl);
