interface LoadingStateProps {
  message?: string
  className?: string
}

export function LoadingState({
  message = 'Carregando...',
  className = '',
}: LoadingStateProps) {
  return (
    <div
      className={`admin-state ${className}`.trim()}
      role="status"
      aria-live="polite"
    >
      {message}
    </div>
  )
}