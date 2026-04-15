import { startTransition, useEffect, useEffectEvent, useRef, useState } from "react";
import { CurrentWeatherHero } from "./components/CurrentWeatherHero";
import { DailyForecastList } from "./components/DailyForecastList";
import { ErrorView } from "./components/ErrorView";
import { HourlyForecastList } from "./components/HourlyForecastList";
import { LoadingView } from "./components/LoadingView";
import { fetchWeather } from "./lib/api";
import type { WeatherResponse } from "./lib/types";

type LoadState = "idle" | "loading" | "success" | "error";
const defaultErrorMessage = "Не удалось получить погодные данные.";

function App() {
  const [state, setState] = useState<LoadState>("idle");
  const [weather, setWeather] = useState<WeatherResponse | null>(null);
  const [errorMessage, setErrorMessage] = useState(defaultErrorMessage);
  const activeRequestControllerRef = useRef<AbortController | null>(null);
  const hasStartedInitialLoadRef = useRef(false);

  const runLoad = useEffectEvent(async (signal: AbortSignal): Promise<void> => {
    setState("loading");
    setErrorMessage(defaultErrorMessage);

    try {
      const response = await fetchWeather(signal);

      startTransition(() => {
        setWeather(response);
        setState("success");
      });
    } catch (error) {
      if (signal.aborted) {
        return;
      }

      setWeather(null);
      setErrorMessage(error instanceof Error ? error.message : defaultErrorMessage);
      setState("error");
    }
  });

  const startLoad = useEffectEvent((): void => {
    if (activeRequestControllerRef.current !== null) {
      return;
    }

    const controller = new AbortController();
    activeRequestControllerRef.current = controller;

    void runLoad(controller.signal).finally(() => {
      if (activeRequestControllerRef.current === controller) {
        activeRequestControllerRef.current = null;
      }
    });
  });

  useEffect(() => {
    if (hasStartedInitialLoadRef.current) {
      return;
    }

    hasStartedInitialLoadRef.current = true;
    startLoad();

    return () => {
      activeRequestControllerRef.current?.abort();
      activeRequestControllerRef.current = null;
    };
  }, []);

  if (weather === null && (state === "loading" || state === "idle")) {
    return <LoadingView />;
  }

  if (state === "error" || weather === null) {
    return <ErrorView isRetrying={false} message={errorMessage} onRetry={startLoad} />;
  }

  const isRefreshing = state === "loading";

  return (
    <div className="shell">
      <div className="ambient ambient-left" />
      <div className="ambient ambient-right" />
      <main className="layout">
        <CurrentWeatherHero isRefreshing={isRefreshing} onRefresh={startLoad} weather={weather} />
        <HourlyForecastList items={weather.hourly} />
        <DailyForecastList items={weather.daily} />
      </main>
    </div>
  );
}

export default App;
