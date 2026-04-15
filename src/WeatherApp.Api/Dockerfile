FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/WeatherApp.Api/ WeatherApp.Api/
RUN dotnet restore WeatherApp.Api/WeatherApp.Api.csproj
RUN dotnet publish WeatherApp.Api/WeatherApp.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "WeatherApp.Api.dll"]
