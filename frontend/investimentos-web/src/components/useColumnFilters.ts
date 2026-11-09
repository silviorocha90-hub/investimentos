import { useState } from 'react'

export interface FilterColumn<T> {
  id: string
  label: string
  value: (row: T) => string
}

export type ColumnFilters = Readonly<Record<string, readonly string[]>>

export function filterTableRows<T>(rows: readonly T[], columns: readonly FilterColumn<T>[], filters: ColumnFilters) {
  return rows.filter(row => columns.every(column =>
    !filters[column.id]?.length || filters[column.id].includes(column.value(row)),
  ))
}

export function useColumnFilters<T>(rows: readonly T[], columns: readonly FilterColumn<T>[], scope = '') {
  const [state, setState] = useState<{ scope: string; filters: ColumnFilters }>({ scope, filters: {} })
  const filters = state.scope === scope ? state.filters : {}
  const filteredRows = filterTableRows(rows, columns, filters)
  const values = Object.fromEntries(columns.map(column => [
    column.id, [...new Set(rows.map(row => column.value(row)))].sort((a, b) => a.localeCompare(b, 'pt-BR', { numeric: true })),
  ]))

  function setFilter(id: string, selected: readonly string[]) {
    setState(previous => {
      const next = { ...(previous.scope === scope ? previous.filters : {}) }
      if (selected.length) next[id] = selected
      else delete next[id]
      return { scope, filters: next }
    })
  }

  return { filteredRows, filters, values, setFilter, clear: () => setState({ scope, filters: {} }), active: Object.keys(filters).length }
}

