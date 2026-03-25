namespace WeatherApp.Api.Services;

public sealed class WeatherCircuitBreakerOpenException : InvalidOperationException
{
    public WeatherCircuitBreakerOpenException()
        : base("Weather API circuit breaker is open.")
    {
    }
}
