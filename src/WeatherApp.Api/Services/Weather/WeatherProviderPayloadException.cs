namespace WeatherApp.Api.Services.Weather;

public sealed class WeatherProviderPayloadException(string message, Exception? innerException = null)
    : Exception(message, innerException);
