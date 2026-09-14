import {
  Modal,
} from './Modal'

interface ConfirmDialogProps {
  open: boolean
  title: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
  loading?: boolean
  onConfirm: () => void
  onCancel: () => void
}

export function ConfirmDialog({
  open,
  title,
  message,
  confirmLabel = 'Confirmar',
  cancelLabel = 'Cancelar',
  loading = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  if (!open) {
    return null
  }

  return (
    <Modal
      title={title}
      onClose={onCancel}
      closeDisabled={loading}
      footer={
        <>
          <button
            type="button"
            className="admin-clear-button"
            onClick={onCancel}
            disabled={loading}
          >
            {cancelLabel}
          </button>

          <button
            type="button"
            className="admin-primary-button"
            onClick={onConfirm}
            disabled={loading}
          >
            {loading
              ? 'Processando...'
              : confirmLabel}
          </button>
        </>
      }
    >
      <p>
        {message}
      </p>
    </Modal>
  )
}