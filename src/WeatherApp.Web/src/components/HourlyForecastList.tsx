import { formatHour, formatTemperature } from "../lib/formatters";
import type { HourlyForecast } from "../lib/types";

interface HourlyForecastListProps {
  items: HourlyForecast[];
}

export function HourlyForecastList({ items }: HourlyForecastListProps) {
  return (
    <section className="panel">
      <div className="panel-header">
        <p className="panel-eyebrow">Hourly</p>
        <h2>Оставшиеся часы и завтрашний день</h2>
      </div>
      <div className="hourly-grid">
        {items.map((item) => (
          <article className="hour-card" key={item.time}>
            <span className="hour-time">{formatHour(item.time)}</span>
            <img alt={item.conditionText} className="weather-icon" src={item.iconUrl} />
            <strong className="hour-temperature">{formatTemperature(item.temperatureC)}</strong>
            <span className="hour-condition">{item.conditionText}</span>
          </article>
        ))}
      </div>
    </section>
  );
}
