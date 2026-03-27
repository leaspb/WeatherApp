namespace WeatherApp.Api.Services.Weather;

public sealed class WeatherApiCircuitBreaker
{
    private readonly Lock _syncRoot = new();
    private int _failureCount;
    private DateTimeOffset? _brokenUntilUtc;

    public void EnsureRequestAllowed(DateTimeOffset utcNow)
    {
        lock (_syncRoot)
        {
            if (_brokenUntilUtc is null)
            {
                return;
            }

            if (utcNow >= _brokenUntilUtc.Value)
            {
                _brokenUntilUtc = null;
                _failureCount = 0;
                return;
            }

            throw new WeatherCircuitBreakerOpenException();
        }
    }

    public void RecordSuccess()
    {
        lock (_syncRoot)
        {
            _failureCount = 0;
            _brokenUntilUtc = null;
        }
    }

    public void RecordFailure(
        DateTimeOffset utcNow,
        int failureThreshold,
        TimeSpan breakDuration)
    {
        lock (_syncRoot)
        {
            _failureCount++;

            if (_failureCount < failureThreshold)
            {
                return;
            }

            _brokenUntilUtc = utcNow.Add(breakDuration);
        }
    }
}
