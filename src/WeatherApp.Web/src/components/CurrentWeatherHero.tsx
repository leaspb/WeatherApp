import { formatHeroTime, formatTemperature } from "../lib/formatters";
import { WeatherIcon } from "./WeatherIcon";
import type { WeatherResponse } from "../lib/types";

interface CurrentWeatherHeroProps {
  isRefreshing: boolean;
  weather: WeatherResponse;
  onRefresh: () => void;
}

export function CurrentWeatherHero({ isRefreshing, weather, onRefresh }: CurrentWeatherHeroProps) {
  return (
    <section className="hero">
      <div className="hero-copy-wrap">
        <p className="eyebrow">Weather App Moscow</p>
        <h1>{weather.location.name}</h1>
        <p className="hero-copy">{formatHeroTime(weather.location.localTime)}</p>
        <div className="hero-metrics">
          <span className="hero-temperature">{formatTemperature(weather.current.temperatureC)}</span>
          <div className="hero-condition-wrap">
            <WeatherIcon
              alt={weather.current.conditionText}
              className="hero-icon"
              src={weather.current.iconUrl}
            />
            <span className="hero-condition">{weather.current.conditionText}</span>
          </div>
        </div>
      </div>
      <button className="ghost-button" disabled={isRefreshing} onClick={onRefresh} type="button">
        {isRefreshing ? "Обновляем..." : "Обновить"}
      </button>
    </section>
  );
}
