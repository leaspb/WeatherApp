import type { ApiProblemDetails, WeatherResponse } from "./types";

const defaultApiErrorMessage = "Не удалось получить погодные данные.";

export async function fetchWeather(signal: AbortSignal): Promise<WeatherResponse> {
  const response = await fetch("/api/weather", { signal });

  if (!response.ok) {
    let message = defaultApiErrorMessage;

    try {
      const problem = (await response.json()) as ApiProblemDetails;
      message = problem.title ?? problem.detail ?? message;
    } catch {
      message = defaultApiErrorMessage;
    }

    throw new Error(message);
  }

  const contentType = response.headers.get("content-type") ?? "";

  if (!contentType.includes("application/json")) {
    throw new Error(defaultApiErrorMessage);
  }

  try {
    return (await response.json()) as WeatherResponse;
  } catch {
    throw new Error(defaultApiErrorMessage);
  }
}
