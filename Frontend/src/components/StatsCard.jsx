export default function StatsCard({ label, value, hint }) {
  return (
    <article className="metric-card" aria-label={label}>
      <p>{label}</p>
      <h4>{value}</h4>
      <span>{hint}</span>
    </article>
  );
}
