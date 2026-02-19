import Modal from "./Modal";

export default function DeleteUrlModal({ open, urlItem, deleting, onClose, onConfirm }) {
  return (
    <Modal
      open={open}
      onClose={onClose}
      title="Delete Short Link"
      subtitle="This action permanently removes the selected URL entry."
      actions={
        <>
          <button className="button button-secondary" type="button" onClick={onClose}>
            Cancel
          </button>
          <button className="button button-danger" type="button" onClick={onConfirm} disabled={deleting}>
            {deleting ? "Deleting..." : "Delete"}
          </button>
        </>
      }
    >
      <p>
        You are about to delete <strong>{urlItem?.shortUrlCode || "this link"}</strong>.
      </p>
    </Modal>
  );
}
