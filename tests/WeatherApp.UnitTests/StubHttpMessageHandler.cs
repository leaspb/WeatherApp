namespace WeatherApp.UnitTests;

using System.Collections.Concurrent;
using System.Net;
using System.Text;

public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly ConcurrentQueue<HttpStatusCode> transientStatusCodes = new();
    private readonly ConcurrentQueue<HttpStatusCode> statusCodes = new();
    private int requestCount;

    public int RequestCount => requestCount;

    public TimeSpan ResponseDelay { get; set; }

    public void EnqueueTransientStatusCode(HttpStatusCode statusCode)
    {
        transientStatusCodes.Enqueue(statusCode);
    }

    public void EnqueueStatusCode(HttpStatusCode statusCode)
    {
        statusCodes.Enqueue(statusCode);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref requestCount);

        if (ResponseDelay > TimeSpan.Zero)
        {
            await Task.Delay(ResponseDelay, cancellationToken);
        }

        if (transientStatusCodes.TryDequeue(out var transientStatusCode))
        {
            return new HttpResponseMessage(transientStatusCode);
        }

        if (statusCodes.TryDequeue(out var statusCode))
        {
            return new HttpResponseMessage(statusCode);
        }

        var content = request.RequestUri?.AbsolutePath switch
        {
            "/current.json" => CreateJsonResponse(CurrentWeatherJson),
            "/forecast.json" => CreateJsonResponse(ForecastWeatherJson),
            _ => new HttpResponseMessage(HttpStatusCode.NotFound),
        };

        return content;
    }

    private static HttpResponseMessage CreateJsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    private const string CurrentWeatherJson =
        """
        {
          "location": {
            "name": "Moscow",
            "localtime": "2024-09-17 20:30"
          },
          "current": {
            "temp_c": 16.4,
            "condition": {
              "text": "Cloudy",
              "icon": "//cdn.weatherapi.com/weather/64x64/day/116.png"
            }
          }
        }
        """;

    private const string ForecastWeatherJson =
        """
        {
          "forecast": {
            "forecastday": [
              {
                "date": "2024-09-17",
                "day": {
                  "maxtemp_c": 18.0,
                  "mintemp_c": 10.0,
                  "condition": {
                    "text": "Partly cloudy",
                    "icon": "//cdn.weatherapi.com/weather/64x64/day/116.png"
                  }
                },
                "hour": [
                  { "time": "2024-09-17 19:00", "temp_c": 15.0, "condition": { "text": "Cloudy", "icon": "//cdn.weatherapi.com/weather/64x64/day/116.png" } },
                  { "time": "2024-09-17 20:00", "temp_c": 14.8, "condition": { "text": "Cloudy", "icon": "//cdn.weatherapi.com/weather/64x64/day/116.png" } },
                  { "time": "2024-09-17 21:00", "temp_c": 14.1, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } },
                  { "time": "2024-09-17 22:00", "temp_c": 13.4, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } },
                  { "time": "2024-09-17 23:00", "temp_c": 12.7, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } }
                ]
              },
              {
                "date": "2024-09-18",
                "day": {
                  "maxtemp_c": 19.0,
                  "mintemp_c": 11.0,
                  "condition": {
                    "text": "Sunny",
                    "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png"
                  }
                },
                "hour": [
                  { "time": "2024-09-18 00:00", "temp_c": 11.0, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } },
                  { "time": "2024-09-18 01:00", "temp_c": 10.9, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } },
                  { "time": "2024-09-18 02:00", "temp_c": 10.7, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } },
                  { "time": "2024-09-18 03:00", "temp_c": 10.5, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } },
                  { "time": "2024-09-18 04:00", "temp_c": 10.3, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } },
                  { "time": "2024-09-18 05:00", "temp_c": 10.1, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } },
                  { "time": "2024-09-18 06:00", "temp_c": 10.5, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 07:00", "temp_c": 11.2, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 08:00", "temp_c": 12.5, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 09:00", "temp_c": 13.9, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 10:00", "temp_c": 15.1, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 11:00", "temp_c": 16.0, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 12:00", "temp_c": 17.0, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 13:00", "temp_c": 17.8, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 14:00", "temp_c": 18.2, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 15:00", "temp_c": 18.6, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 16:00", "temp_c": 18.4, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 17:00", "temp_c": 17.9, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 18:00", "temp_c": 17.0, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 19:00", "temp_c": 16.2, "condition": { "text": "Sunny", "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
                  { "time": "2024-09-18 20:00", "temp_c": 15.0, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } },
                  { "time": "2024-09-18 21:00", "temp_c": 14.2, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } },
                  { "time": "2024-09-18 22:00", "temp_c": 13.5, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } },
                  { "time": "2024-09-18 23:00", "temp_c": 12.9, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } }
                ]
              },
              {
                "date": "2024-09-19",
                "day": {
                  "maxtemp_c": 17.0,
                  "mintemp_c": 9.0,
                  "condition": {
                    "text": "Patchy rain nearby",
                    "icon": "//cdn.weatherapi.com/weather/64x64/day/176.png"
                  }
                },
                "hour": []
              }
            ]
          }
        }
        """;
}
