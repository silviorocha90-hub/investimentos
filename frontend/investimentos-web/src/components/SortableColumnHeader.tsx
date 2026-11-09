import './SortableColumnHeader.css'

interface Props {
  label: string
  direction?: 'asc' | 'desc'
  onSort: () => void
}

export function SortableColumnHeader({ label, direction, onSort }: Props) {
  const next = direction === 'asc' ? 'decrescente' : 'crescente'
  return <th scope="col" aria-sort={direction === 'asc' ? 'ascending' : direction === 'desc' ? 'descending' : 'none'}>
    <button type="button" className={`sortable-column-trigger${direction ? ' is-sorted' : ''}`}
      onClick={onSort} aria-label={`Ordenar por ${label} em ordem ${next}`}>
      <span>{label}</span><span className="sortable-column-icon" aria-hidden="true">{direction === 'asc' ? '▲' : direction === 'desc' ? '▼' : '↕'}</span>
    </button>
  </th>
}
