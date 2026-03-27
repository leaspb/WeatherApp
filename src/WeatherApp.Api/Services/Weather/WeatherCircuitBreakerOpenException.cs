namespace WeatherApp.Api.Services.Weather;

public sealed class WeatherCircuitBreakerOpenException : Exception
{
    public WeatherCircuitBreakerOpenException() : base("Weather API circuit breaker is open.")
    {
    }
}
