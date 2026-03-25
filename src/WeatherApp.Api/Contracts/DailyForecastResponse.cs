namespace WeatherApp.Api.Contracts;

public sealed record DailyForecastResponse(
    DateOnly Date,
    decimal MaxTemperatureC,
    decimal MinTemperatureC,
    string ConditionText,
    string IconUrl);
