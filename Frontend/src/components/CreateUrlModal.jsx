import { useState } from "react";
import Modal from "./Modal";

export default function CreateUrlModal({ open, creating, onClose, onCreate }) {
  const [form, setForm] = useState({ shortUrlCode: "", longUrl: "", description: "" });

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  }

  function handleSubmit(event) {
    event.preventDefault();
    onCreate(form);
  }

  function resetAndClose() {
    setForm({ shortUrlCode: "", longUrl: "", description: "" });
    onClose();
  }

  return (
    <Modal
      open={open}
      onClose={resetAndClose}
      title="Create New Short Link"
      subtitle="All fields are required by the current backend contract."
      actions={
        <>
          <button className="button button-secondary" onClick={resetAndClose} type="button">
            Cancel
          </button>
          <button className="button button-primary" form="create-url-form" type="submit" disabled={creating}>
            {creating ? "Creating..." : "Create Link"}
          </button>
        </>
      }
    >
      <form id="create-url-form" className="stack-sm" onSubmit={handleSubmit}>
        <label className="field">
          <span>Short Code</span>
          <input
            name="shortUrlCode"
            type="text"
            placeholder="launch-2026"
            value={form.shortUrlCode}
            onChange={handleChange}
            required
          />
        </label>

        <label className="field">
          <span>Long URL</span>
          <input
            name="longUrl"
            type="url"
            placeholder="https://example.com/campaign"
            value={form.longUrl}
            onChange={handleChange}
            required
          />
        </label>

        <label className="field">
          <span>Description</span>
          <textarea
            name="description"
            placeholder="Campaign landing page"
            value={form.description}
            onChange={handleChange}
            rows={3}
            required
          />
        </label>
      </form>
    </Modal>
  );
}
