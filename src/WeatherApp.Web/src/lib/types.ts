export interface WeatherLocation {
  name: string;
  localTime: string;
}

export interface CurrentWeather {
  temperatureC: number;
  conditionText: string;
  iconUrl: string;
}

export interface HourlyForecast {
  time: string;
  temperatureC: number;
  conditionText: string;
  iconUrl: string;
}

export interface DailyForecast {
  date: string;
  maxTemperatureC: number;
  minTemperatureC: number;
  conditionText: string;
  iconUrl: string;
}

export interface WeatherResponse {
  location: WeatherLocation;
  current: CurrentWeather;
  hourly: HourlyForecast[];
  daily: DailyForecast[];
}

export interface ApiProblemDetails {
  title?: string;
  detail?: string;
}
