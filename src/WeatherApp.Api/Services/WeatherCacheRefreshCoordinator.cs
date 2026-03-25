namespace WeatherApp.Api.Services;

public sealed class WeatherCacheRefreshCoordinator
{
    public SemaphoreSlim RefreshLock { get; } = new(1, 1);
}
