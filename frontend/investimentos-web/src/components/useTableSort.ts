import { useState } from 'react'

export interface SortColumn<T> {
  id: string
  label: string
  value: (row: T) => string | number | null | undefined
}

export interface TableSort {
  column: string
  direction: 'asc' | 'desc'
}

const collator = new Intl.Collator('pt-BR', { numeric: true, sensitivity: 'base' })
const empty = (value: string | number | null | undefined) => value == null || (typeof value === 'number' && !Number.isFinite(value))

export function sortTableRows<T>(rows: readonly T[], columns: readonly SortColumn<T>[], sort: TableSort | null) {
  const column = columns.find(item => item.id === sort?.column)
  if (!column || !sort) return [...rows]
  const direction = sort.direction === 'asc' ? 1 : -1
  return rows.map((row, index) => ({ row, index })).sort((a, b) => {
    const left = column.value(a.row), right = column.value(b.row)
    // Ausências de cotação ou resultado ficam no fim em ambas as direções.
    if (empty(left) || empty(right)) return empty(left) && empty(right) ? a.index - b.index : empty(left) ? 1 : -1
    const comparison = typeof left === 'number' && typeof right === 'number'
      ? left - right : collator.compare(String(left), String(right))
    return comparison * direction || a.index - b.index
  }).map(item => item.row)
}

export function useTableSort<T>(rows: readonly T[], columns: readonly SortColumn<T>[], scope = '', initialSort: TableSort | null = null) {
  const [state, setState] = useState<{ scope: string; sort: TableSort | null }>({ scope, sort: initialSort })
  const sort = state.scope === scope ? state.sort : initialSort
  function toggle(column: string) {
    setState(previous => {
      const current = previous.scope === scope ? previous.sort : initialSort
      return { scope, sort: { column, direction: current?.column === column && current.direction === 'asc' ? 'desc' : 'asc' } }
    })
  }
  return { sortedRows: sortTableRows(rows, columns, sort), sort, toggle }
}

export function dateSortValue(value: string | null | undefined): number | null {
  if (!value) return null
  const timestamp = Date.parse(value)
  return Number.isFinite(timestamp) ? timestamp : null
}
