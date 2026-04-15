import { formatDayLabel, formatTemperature } from "../lib/formatters";
import { WeatherIcon } from "./WeatherIcon";
import type { DailyForecast } from "../lib/types";

interface DailyForecastListProps {
  items: DailyForecast[];
}

export function DailyForecastList({ items }: DailyForecastListProps) {
  return (
    <section className="panel">
      <div className="panel-header">
        <p className="panel-eyebrow">3 Days</p>
        <h2>Прогноз на три дня</h2>
      </div>
      <div className="daily-list">
        {items.map((item) => (
          <article className="day-row" key={item.date}>
            <div className="day-main">
              <span className="day-label">{formatDayLabel(item.date)}</span>
              <span className="day-condition">{item.conditionText}</span>
            </div>
            <div className="day-meta">
              <WeatherIcon alt={item.conditionText} className="weather-icon" src={item.iconUrl} />
              <span className="day-range">
                {formatTemperature(item.maxTemperatureC)} / {formatTemperature(item.minTemperatureC)}
              </span>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}
