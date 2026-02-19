export default function AppShell({ user, activeView, onChangeView, onLogout, children }) {
  return (
    <div className="forge-layout">
      <aside className="forge-nav" aria-label="Primary navigation">
        <div className="forge-logo">
          <span>FL</span>
          <div>
            <p>FORGE</p>
            <strong>Link Workspace</strong>
          </div>
        </div>

        <div className="forge-nav-buttons">
          <button
            className={activeView === "dashboard" ? "forge-nav-btn active" : "forge-nav-btn"}
            type="button"
            onClick={() => onChangeView("dashboard")}
          >
            Workspace
          </button>
          <button
            className={activeView === "settings" ? "forge-nav-btn active" : "forge-nav-btn"}
            type="button"
            onClick={() => onChangeView("settings")}
          >
            Account
          </button>
        </div>

        <div className="forge-user-card">
          <p>Signed in as</p>
          <strong>{user?.username}</strong>
          <button className="button button-secondary" type="button" onClick={onLogout}>
            Sign Out
          </button>
        </div>
      </aside>

      <section className="forge-stage">{children}</section>
    </div>
  );
}
