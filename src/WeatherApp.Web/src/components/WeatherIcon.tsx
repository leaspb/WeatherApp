interface WeatherIconProps {
  alt: string;
  className: string;
  src: string;
}

export function WeatherIcon({ alt, className, src }: WeatherIconProps) {
  if (src.trim() === "") {
    return <span aria-hidden="true" className={`${className} weather-icon-fallback`} />;
  }

  return <img alt={alt} className={className} src={src} />;
}
