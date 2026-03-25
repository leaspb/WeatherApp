import { formatHeroTime, formatTemperature } from "../lib/formatters";
import type { WeatherResponse } from "../lib/types";

interface CurrentWeatherHeroProps {
  weather: WeatherResponse;
  onRefresh: () => void;
}

export function CurrentWeatherHero({ weather, onRefresh }: CurrentWeatherHeroProps) {
  return (
    <section className="hero">
      <div className="hero-copy-wrap">
        <p className="eyebrow">Weather App Moscow</p>
        <h1>{weather.location.name}</h1>
        <p className="hero-copy">{formatHeroTime(weather.location.localTime)}</p>
        <div className="hero-metrics">
          <span className="hero-temperature">{formatTemperature(weather.current.temperatureC)}</span>
          <div className="hero-condition-wrap">
            <img alt={weather.current.conditionText} className="hero-icon" src={weather.current.iconUrl} />
            <span className="hero-condition">{weather.current.conditionText}</span>
          </div>
        </div>
      </div>
      <button className="ghost-button" onClick={onRefresh} type="button">
        Обновить
      </button>
    </section>
  );
}
