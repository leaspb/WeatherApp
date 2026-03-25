const moscowTimeZone = "Europe/Moscow";

const hourFormatter = new Intl.DateTimeFormat("ru-RU", {
  hour: "2-digit",
  minute: "2-digit",
  timeZone: moscowTimeZone,
});

const dayLabelFormatter = new Intl.DateTimeFormat("ru-RU", {
  weekday: "short",
  day: "numeric",
  month: "short",
  timeZone: moscowTimeZone,
});

const heroTimeFormatter = new Intl.DateTimeFormat("ru-RU", {
  weekday: "long",
  day: "numeric",
  month: "long",
  hour: "2-digit",
  minute: "2-digit",
  timeZone: moscowTimeZone,
});

export function formatTemperature(value: number): string {
  return `${Math.round(value)}°`;
}

export function formatHour(value: string): string {
  return hourFormatter.format(new Date(value));
}

export function formatDayLabel(value: string): string {
  return dayLabelFormatter.format(new Date(`${value}T12:00:00+03:00`));
}

export function formatHeroTime(value: string): string {
  return heroTimeFormatter.format(new Date(value));
}
