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
- xUnit

## Запуск через Docker Compose

```bash
docker compose up --build
```

После запуска:
- frontend: `http://localhost:4173`
- backend: `http://localhost:5184`
- healthcheck: `http://localhost:5184/api/health`

## Как проверить приложение

После `docker compose up --build`:

1. Откройте `http://localhost:4173`.
2. Убедитесь, что отображаются текущая погода, почасовой прогноз и прогноз на 3 дня.
3. Проверьте, что frontend успешно получает данные с backend.
4. Проверьте backend health endpoint:

```bash
curl http://localhost:5184/api/health
```

5. Проверьте погодный endpoint:

```bash
curl http://localhost:5184/api/weather
```

Логи:

```bash
docker compose logs -f
```

## Локальный запуск без Docker

Backend:

```bash
dotnet run --project src/WeatherApp.Api
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

- город зафиксирован на координатах Москвы: `55.7558, 37.6173`;
- frontend в Docker обслуживается через `nginx` и проксирует `/api/*` в `backend`.
