namespace WeatherApp.Api.Contracts;

public sealed record HourlyForecastResponse(
    DateTimeOffset Time,
    decimal TemperatureC,
    string ConditionText,
    string IconUrl);
