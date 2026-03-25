namespace WeatherApp.Api.Abstractions;

using WeatherApp.Api.Contracts;

public interface IWeatherService
{
    Task<WeatherResponse> GetWeatherAsync(CancellationToken cancellationToken);
}
