import { Modal } from './Modal.tsx';

interface ConfirmDialogProps {
  isOpen: boolean;
  title: string;
  description: string;
  confirmLabel: string;
  isPending?: boolean;
  error?: string | null;
  onClose: () => void;
  onConfirm: () => void;
}

export function ConfirmDialog({
  isOpen,
  title,
  description,
  confirmLabel,
  isPending = false,
  error = null,
  onClose,
  onConfirm,
}: ConfirmDialogProps) {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title}>
      <p className="confirm-dialog-copy">{description}</p>
      {error && <div className="alert alert-error" role="alert">{error}</div>}
      <div className="modal-actions">
        <button type="button" className="btn btn-outline" onClick={onClose} disabled={isPending}>
          Cancelar
        </button>
        <button type="button" className="btn btn-danger" onClick={onConfirm} disabled={isPending}>
          {isPending ? 'Excluindo…' : confirmLabel}
        </button>
      </div>
    </Modal>
  );
}
