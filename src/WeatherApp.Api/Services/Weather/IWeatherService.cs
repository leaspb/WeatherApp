using WeatherApp.Api.Contracts;

namespace WeatherApp.Api.Services.Weather;

public interface IWeatherService
{
    Task<WeatherResponse> GetWeatherAsync(CancellationToken cancellationToken);
}
