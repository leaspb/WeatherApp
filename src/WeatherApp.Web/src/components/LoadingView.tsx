export function LoadingView() {
  return (
    <div className="shell">
      <div className="ambient ambient-left" />
      <div className="ambient ambient-right" />
      <main className="layout">
        <section className="hero hero-loading">
          <div className="skeleton skeleton-pill" />
          <div className="skeleton skeleton-title" />
          <div className="skeleton skeleton-copy" />
          <div className="skeleton skeleton-copy short" />
        </section>
        <section className="panel">
          <div className="panel-header">
            <div className="skeleton skeleton-subtitle" />
          </div>
          <div className="hourly-grid">
            {Array.from({ length: 8 }, (_, index) => (
              <div className="hour-card" key={index}>
                <div className="skeleton skeleton-mini" />
                <div className="skeleton skeleton-mini icon" />
                <div className="skeleton skeleton-mini short" />
              </div>
            ))}
          </div>
        </section>
      </main>
    </div>
  );
}
