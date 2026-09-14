interface PaginationProps {
  page: number
  totalPages: number
  totalItems?: number
  itemLabel?: string
  onPageChange: (
    page: number,
  ) => void
}

export function Pagination({
  page,
  totalPages,
  totalItems,
  itemLabel = 'itens',
  onPageChange,
}: PaginationProps) {
  const safeTotalPages =
    Math.max(
      1,
      totalPages,
    )

  const safePage =
    Math.min(
      Math.max(1, page),
      safeTotalPages,
    )

  return (
    <footer className="admin-pagination">
      {totalItems != null ? (
        <span>
          {totalItems}{' '}
          {itemLabel}
        </span>
      ) : (
        <span />
      )}

      <div>
        <button
          type="button"
          disabled={
            safePage <= 1
          }
          onClick={() =>
            onPageChange(
              safePage - 1,
            )
          }
          aria-label="Página anterior"
        >
          ‹
        </button>

        <strong>
          {safePage} /{' '}
          {safeTotalPages}
        </strong>

        <button
          type="button"
          disabled={
            safePage >=
            safeTotalPages
          }
          onClick={() =>
            onPageChange(
              safePage + 1,
            )
          }
          aria-label="Próxima página"
        >
          ›
        </button>
      </div>
    </footer>
  )
}