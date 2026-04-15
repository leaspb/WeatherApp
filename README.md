# WeatherApp

Погодное веб-приложение для Москвы с frontend на React + TypeScript и backend на .NET 9 Minimal API.

## Задание

Написать погодное веб-приложение на .Net Framework, использовать можно любые библиотеки помогающие решить поставленную задача. В приложение должен быть интерфейс, и бекенд.
UI
• Отобразить один экран с погодной информацией: текущая, почасовая (показывать оставшиеся часы из текущего дня и все часы следующего), прогноз погоды на 3 дня.
• Обработать показ загрузки и ошибку, если что-то пошло не так, с кнопкой повторного запроса
• По дизайну никаких ограничений нет, все на ваш вкус.
Геолокация и запросы
• Геолокацию зафиксировать на использование города Москва
• Данные получать из запросов API:
http://api.weatherapi.com/v1/current.json?key=fa8b3df74d4042b9aa7135114252304&q=LAT,LON
http://api.weatherapi.com/v1/forecast.json?key=fa8b3df74d4042b9aa7135114252304&q=LAT,LON&days=3
Реализация графической составляющей на усмотрения кандидата, оформление должно быть понятным, не запутанным, но соответствовать тз.

## Стек

- React 19 + TypeScript + Vite
- ASP.NET Core Minimal API (.NET 9)
- Docker Compose
- Swagger UI / OpenAPI
- xUnit

## Запуск через Docker Compose

1. Создайте `.env` на основе `.env.example`.
2. Укажите `WEATHER_API_KEY`.
3. Запустите проект:

```bash
docker compose up --build
```

`frontend` стартует после того, как `backend` пройдет healthcheck.

После запуска:
- frontend: `http://localhost:4173`
- backend: `http://localhost:5184`
- healthcheck: `http://localhost:5184/api/health`
- weather endpoint: `http://localhost:5184/api/weather`
- Swagger UI: `http://localhost:5184/swagger`
- OpenAPI JSON: `http://localhost:5184/openapi/v1.json`

Остановить контейнеры:

```bash
docker compose down
```

Логи:

```bash
docker compose logs -f
```

## Локальный запуск без Docker

Backend:

```bash
WeatherApi__ApiKey=your-key dotnet run --project src/WeatherApp.Api
```

Frontend:

```bash
cd src/WeatherApp.Web
npm install
npm run dev
```

В dev-режиме frontend работает на `http://localhost:4173` и проксирует `/api` на `http://localhost:5184`.

## Тесты

Все тесты:

```bash
dotnet test WeatherApp.sln
```

Отдельно:

```bash
dotnet test tests/WeatherApp.UnitTests/WeatherApp.UnitTests.csproj
dotnet test tests/WeatherApp.IntegrationTests/WeatherApp.IntegrationTests.csproj
```

## Что важно знать

- API-ключ не хранится в репозитории и должен передаваться через `WeatherApi__ApiKey` или `.env`;
- город зафиксирован на координатах Москвы: `55.7558, 37.6173`;
- frontend в Docker обслуживается через `nginx` и проксирует `/api/*` в `backend`.
