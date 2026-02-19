import { buildRedirectUrl } from "../api/urlApi";
import { formatDateTime } from "../utils/date";

export default function UrlTable({ urls, selectedId, onSelect, onDeleteClick }) {
  return (
    <section className="panel card-surface" aria-label="URL inventory">
      <header className="section-header">
        <h3>Inventory</h3>
        <p className="muted">All generated links for this account.</p>
      </header>

      {!urls.length ? (
        <div className="empty-block">
          <h4>Nothing here yet</h4>
          <p>Create your first link from the form.</p>
        </div>
      ) : (
        <div className="inventory-list">
          {urls.map((item) => (
            <article
              key={item.id}
              className={item.id === selectedId ? "inventory-item active" : "inventory-item"}
              onClick={() => onSelect(item.id)}
            >
              <div className="inventory-main">
                <a href={buildRedirectUrl(item.shortUrlCode)} target="_blank" rel="noreferrer">
                  {item.shortUrlCode}
                </a>
                <p>{item.description || "No description"}</p>
              </div>

              <div className="inventory-meta">
                <span>{item.clicks} hits</span>
                <span>{formatDateTime(item.createdAt)}</span>
              </div>

              <button
                className="button button-danger-soft"
                type="button"
                onClick={(event) => {
                  event.stopPropagation();
                  onDeleteClick(item);
                }}
              >
                Remove
              </button>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
