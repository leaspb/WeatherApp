namespace WeatherApp.Api.Services;

public sealed class WeatherApiCircuitBreaker
{
    private readonly Lock syncRoot = new();
    private int failureCount;
    private DateTimeOffset? brokenUntilUtc;

    public void EnsureRequestAllowed(DateTimeOffset utcNow)
    {
        lock (syncRoot)
        {
            if (brokenUntilUtc is null)
            {
                return;
            }

            if (utcNow >= brokenUntilUtc.Value)
            {
                brokenUntilUtc = null;
                failureCount = 0;
                return;
            }

            throw new WeatherCircuitBreakerOpenException();
        }
    }

    public void RecordSuccess()
    {
        lock (syncRoot)
        {
            failureCount = 0;
            brokenUntilUtc = null;
        }
    }

    public void RecordFailure(
        DateTimeOffset utcNow,
        int failureThreshold,
        TimeSpan breakDuration)
    {
        lock (syncRoot)
        {
            failureCount++;

            if (failureCount < failureThreshold)
            {
                return;
            }

            brokenUntilUtc = utcNow.Add(breakDuration);
        }
    }
}
