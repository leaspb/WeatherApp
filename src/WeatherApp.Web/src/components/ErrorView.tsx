interface ErrorViewProps {
  isRetrying: boolean;
  message: string;
  onRetry: () => void;
}

export function ErrorView({ isRetrying, message, onRetry }: ErrorViewProps) {
  return (
    <div className="shell">
      <div className="ambient ambient-left" />
      <div className="ambient ambient-right" />
      <main className="layout">
        <section className="hero hero-error">
          <p className="eyebrow">Weather App Moscow</p>
          <h1>Погода временно недоступна</h1>
          <p className="hero-copy">{message}</p>
          <button className="retry-button" disabled={isRetrying} onClick={onRetry} type="button">
            {isRetrying ? "Повторяем..." : "Повторить запрос"}
          </button>
        </section>
      </main>
    </div>
  );
}
