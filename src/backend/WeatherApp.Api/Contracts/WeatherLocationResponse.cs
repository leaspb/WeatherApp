namespace WeatherApp.Api.Contracts;

public sealed record WeatherLocationResponse(
    string Name,
    DateTime LocalTime);
