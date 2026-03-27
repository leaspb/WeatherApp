using System.Net;
using System.Text.Json;
using WeatherApp.Api.Services;
using WeatherApp.Api.Services.Weather;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddWeatherServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet(
    "/api/health",
    () => Results.Ok(new
    {
        Status = "Healthy",
        Service = "WeatherApp.Api",
    }))
    .WithName("GetHealth");

app.MapGet(
    "/api/weather",
    async Task<IResult> (
        IWeatherService weatherService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var weather = await weatherService.GetWeatherAsync(cancellationToken);
            return Results.Ok(weather);
        }
        catch (WeatherCircuitBreakerOpenException)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Weather provider circuit breaker is open.");
        }
        catch (HttpRequestException exception) when (
            exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Weather service configuration is invalid.");
        }
        catch (HttpRequestException exception) when (exception.StatusCode is not null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Weather provider returned an invalid response.");
        }
        catch (HttpRequestException)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Weather provider is unavailable.");
        }
        catch (TimeoutException)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Weather provider timed out.");
        }
        catch (JsonException)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Weather provider returned an invalid response.");
        }
        catch (InvalidOperationException)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Weather service configuration is invalid.");
        }
    })
    .WithName("GetWeather");

app.Run();

public partial class Program;
