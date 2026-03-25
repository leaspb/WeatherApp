import { startTransition, useEffect, useRef, useState } from "react";
import { CurrentWeatherHero } from "./components/CurrentWeatherHero";
import { DailyForecastList } from "./components/DailyForecastList";
import { ErrorView } from "./components/ErrorView";
import { HourlyForecastList } from "./components/HourlyForecastList";
import { LoadingView } from "./components/LoadingView";
import { fetchWeather } from "./lib/api";
import type { WeatherResponse } from "./lib/types";

type LoadState = "idle" | "loading" | "success" | "error";

function App() {
  const [state, setState] = useState<LoadState>("idle");
  const [weather, setWeather] = useState<WeatherResponse | null>(null);
  const [errorMessage, setErrorMessage] = useState("Не удалось получить погодные данные.");
  const activeRequestControllerRef = useRef<AbortController | null>(null);

  async function runLoad(signal: AbortSignal): Promise<void> {
    setState("loading");
    setErrorMessage("Не удалось получить погодные данные.");

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
      setErrorMessage(error instanceof Error ? error.message : "Не удалось получить погодные данные.");
      setState("error");
    }
  }

  function startLoad(): void {
    activeRequestControllerRef.current?.abort();

    const controller = new AbortController();
    activeRequestControllerRef.current = controller;

    void runLoad(controller.signal);
  }

  useEffect(() => {
    startLoad();

    return () => {
      activeRequestControllerRef.current?.abort();
    };
  }, []);

  if (state === "loading" || state === "idle") {
    return <LoadingView />;
  }

  if (state === "error" || weather === null) {
    return <ErrorView message={errorMessage} onRetry={startLoad} />;
  }

  return (
    <div className="shell">
      <div className="ambient ambient-left" />
      <div className="ambient ambient-right" />
      <main className="layout">
        <CurrentWeatherHero onRefresh={startLoad} weather={weather} />
        <HourlyForecastList items={weather.hourly} />
        <DailyForecastList items={weather.daily} />
      </main>
    </div>
  );
}

export default App;
