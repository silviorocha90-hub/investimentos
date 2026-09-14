interface ErrorStateProps {
  message: string
  onRetry?: () => void
  retryLabel?: string
  className?: string
}

export function ErrorState({
  message,
  onRetry,
  retryLabel = 'Tentar novamente',
  className = '',
}: ErrorStateProps) {
  return (
    <div
      className={`admin-state admin-error ${className}`.trim()}
      role="alert"
    >
      <span>
        {message}
      </span>

      {onRetry ? (
        <button
          type="button"
          onClick={onRetry}
        >
          {retryLabel}
        </button>
      ) : null}
    </div>
  )
}