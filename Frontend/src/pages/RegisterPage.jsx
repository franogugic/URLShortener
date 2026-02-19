import { useState } from "react";
import { Link, Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "../context/useAuth";

export default function RegisterPage() {
  const { user, register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ username: "", password: "" });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (user) return <Navigate to="/app" replace />;

  async function handleSubmit(event) {
    event.preventDefault();
    setError("");
    setSuccess("");
    setIsSubmitting(true);

    try {
      await register(form.username, form.password);
      setSuccess("Account created. Redirecting to login...");
      setTimeout(() => navigate("/login"), 700);
    } catch (submitError) {
      setError(submitError.message || "Registration failed.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="entry-layout">
      <section className="entry-card">
        <header>
          <p>FORGE ACCESS</p>
          <h1>Create Account</h1>
          <span>Provision a new user for the workspace.</span>
        </header>

        <form className="stack-sm" onSubmit={handleSubmit}>
          <label className="field">
            <span>Username</span>
            <input
              type="text"
              value={form.username}
              onChange={(event) => setForm((prev) => ({ ...prev, username: event.target.value }))}
              required
            />
          </label>

          <label className="field">
            <span>Password</span>
            <input
              type="password"
              value={form.password}
              onChange={(event) => setForm((prev) => ({ ...prev, password: event.target.value }))}
              required
            />
          </label>

          {error ? <p className="error-text">{error}</p> : null}
          {success ? <p className="success-text">{success}</p> : null}

          <button className="button button-primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Creating..." : "Create"}
          </button>
        </form>

        <footer>
          <p className="muted compact">
            Already have an account? <Link to="/login">Sign in</Link>
          </p>
        </footer>
      </section>
    </div>
  );
}
