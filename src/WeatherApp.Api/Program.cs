using System.Net;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using WeatherApp.Api.Contracts;
using WeatherApp.Api.Services;
using WeatherApp.Api.Services.Weather;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "WeatherApp API",
                Version = "v1",
                Description = "API для получения погодных данных по Москве.",
            });
    });
builder.Services.AddWeatherServices(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(
    options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherApp API v1");
        options.RoutePrefix = "swagger";
    });

app.MapGet(
    "/api/health",
    () => Results.Ok(new
    {
        Status = "Healthy",
        Service = "WeatherApp.Api",
    }))
    .WithName("GetHealth")
    .WithSummary("Проверка доступности API.")
    .WithDescription("Возвращает статус backend-приложения.")
    .Produces(StatusCodes.Status200OK);

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
        catch (WeatherProviderPayloadException)
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
    .WithName("GetWeather")
    .WithSummary("Получение погодных данных по Москве.")
    .WithDescription("Возвращает текущую погоду, почасовой прогноз и прогноз на 3 дня.")
    .Produces<WeatherResponse>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .ProducesProblem(StatusCodes.Status502BadGateway)
    .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

app.Run();

public partial class Program;
