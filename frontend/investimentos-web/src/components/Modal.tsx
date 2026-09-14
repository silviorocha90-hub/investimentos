import type {
  ReactNode,
} from 'react'

interface ModalProps {
  title: string
  subtitle?: string
  children: ReactNode
  footer?: ReactNode
  onClose: () => void
  closeDisabled?: boolean
  className?: string
}

export function Modal({
  title,
  subtitle,
  children,
  footer,
  onClose,
  closeDisabled = false,
  className = '',
}: ModalProps) {
  return (
    <div
      className="admin-modal-backdrop"
      role="presentation"
      onMouseDown={(event) => {
        if (
          event.target ===
            event.currentTarget &&
          !closeDisabled
        ) {
          onClose()
        }
      }}
    >
      <div
        className={`admin-modal ${className}`.trim()}
        role="dialog"
        aria-modal="true"
        aria-label={title}
      >
        <div className="admin-modal-header">
          <div>
            {subtitle ? (
              <span>
                {subtitle}
              </span>
            ) : null}

            <strong>
              {title}
            </strong>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={closeDisabled}
            aria-label="Fechar"
          >
            ×
          </button>
        </div>

        {children}

        {footer ? (
          <div className="admin-modal-actions">
            {footer}
          </div>
        ) : null}
      </div>
    </div>
  )
}