namespace WeatherApp.Api.Services.Weather;

public sealed class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public string BaseUrl { get; set; } = "https://api.weatherapi.com/v1/";

    public string ApiKey { get; set; } = string.Empty;

    public decimal Latitude { get; set; } = 55.7558m;

    public decimal Longitude { get; set; } = 37.6173m;

    public int RetryCount { get; set; } = 2;

    public int RequestTimeoutSeconds { get; set; } = 10;

    public int CacheDurationMinutes { get; set; } = 5;

    public int CircuitBreakerFailureThreshold { get; set; } = 3;

    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}
