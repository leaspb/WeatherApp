namespace WeatherApp.IntegrationTests;

using System.Collections.Concurrent;
using System.Net;
using System.Text;

public sealed class ScriptedWeatherApiMessageHandler : HttpMessageHandler
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<Func<HttpResponseMessage>>> _responses = new();
    private int _requestCount;

    public int RequestCount => _requestCount;

    public TimeSpan ResponseDelay { get; set; }

    public void EnqueueJsonResponse(string path, string json)
    {
        var queue = _responses.GetOrAdd(path, static _ => new ConcurrentQueue<Func<HttpResponseMessage>>());
        queue.Enqueue(
            () => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
    }

    public void EnqueueStatusCode(string path, HttpStatusCode statusCode)
    {
        var queue = _responses.GetOrAdd(path, static _ => new ConcurrentQueue<Func<HttpResponseMessage>>());
        queue.Enqueue(() => new HttpResponseMessage(statusCode));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _requestCount);

        if (ResponseDelay > TimeSpan.Zero)
        {
            await Task.Delay(ResponseDelay, cancellationToken);
        }

        var path = request.RequestUri?.AbsolutePath ?? string.Empty;

        if (_responses.TryGetValue(path, out var queue) && queue.TryDequeue(out var factory))
        {
            return factory();
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }
}
