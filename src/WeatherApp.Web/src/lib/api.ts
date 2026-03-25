import type { ApiProblemDetails, WeatherResponse } from "./types";

export async function fetchWeather(signal: AbortSignal): Promise<WeatherResponse> {
  const response = await fetch("/api/weather", { signal });

  if (!response.ok) {
    let message = "Не удалось получить погодные данные.";

    try {
      const problem = (await response.json()) as ApiProblemDetails;
      message = problem.title ?? problem.detail ?? message;
    } catch {
      message = "Не удалось получить погодные данные.";
    }

    throw new Error(message);
  }

  return (await response.json()) as WeatherResponse;
}
