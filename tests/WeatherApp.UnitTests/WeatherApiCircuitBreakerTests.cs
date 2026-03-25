namespace WeatherApp.UnitTests;

using WeatherApp.Api.Services;

public sealed class WeatherApiCircuitBreakerTests
{
    [Fact]
    public void EnsureRequestAllowed_AllowsRequestsAgain_AfterBreakWindowExpires()
    {
        var circuitBreaker = new WeatherApiCircuitBreaker();
        var failureTime = new DateTimeOffset(2026, 3, 25, 12, 0, 0, TimeSpan.Zero);

        circuitBreaker.RecordFailure(failureTime, failureThreshold: 1, breakDuration: TimeSpan.FromSeconds(30));

        Assert.Throws<WeatherCircuitBreakerOpenException>(
            () => circuitBreaker.EnsureRequestAllowed(failureTime.AddSeconds(10)));

        var resetException = Record.Exception(
            () => circuitBreaker.EnsureRequestAllowed(failureTime.AddSeconds(31)));

        Assert.Null(resetException);
    }
}
