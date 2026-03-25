namespace WeatherApp.IntegrationTests;

public static class WeatherApiPayloads
{
    public const string CurrentWeatherJson =
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

    public const string ForecastWeatherJson =
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
                  { "time": "2024-09-17 21:00", "temp_c": 14.1, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } }
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
                  { "time": "2024-09-18 00:00", "temp_c": 11.0, "condition": { "text": "Clear", "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png" } }
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
