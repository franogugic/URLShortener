import { formatDateTime } from "../utils/date";
import { buildRedirectUrl } from "../api/urlApi";

function InspectorLine({ label, value, link }) {
  return (
    <div className="inspector-line">
      <span>{label}</span>
      {link ? (
        <a href={link} target="_blank" rel="noreferrer">
          {value}
        </a>
      ) : (
        <strong>{value}</strong>
      )}
    </div>
  );
}

export default function UrlDetailsPanel({ urlItem, isLoading, error }) {
  return (
    <section className="panel card-surface" aria-label="URL details">
      <header className="section-header">
        <h3>Inspector</h3>
        <p className="muted">Details of the selected URL.</p>
      </header>

      {isLoading ? <p className="muted">Loading details...</p> : null}
      {error ? <p className="error-text">{error}</p> : null}

      {!isLoading && !error && !urlItem ? (
        <div className="empty-block">
          <h4>No selection</h4>
          <p>Select an inventory item to inspect it.</p>
        </div>
      ) : null}

      {!isLoading && !error && urlItem ? (
        <div className="inspector-stack">
          <InspectorLine label="Short Code" value={urlItem.shortUrlCode} />
          <InspectorLine label="Clicks" value={urlItem.clicks} />
          <InspectorLine label="Created" value={formatDateTime(urlItem.createdAt)} />
          <InspectorLine label="Description" value={urlItem.description || "-"} />
          <InspectorLine label="Long URL" value={urlItem.longUrl} link={urlItem.longUrl} />
          <InspectorLine
            label="Redirect URL"
            value={buildRedirectUrl(urlItem.shortUrlCode)}
            link={buildRedirectUrl(urlItem.shortUrlCode)}
          />
        </div>
      ) : null}
    </section>
  );
}
