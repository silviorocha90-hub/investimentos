import type {
  ReactNode,
} from 'react'

export interface DataTableColumn<T> {
  key: string
  header: ReactNode
  render: (
    item: T,
  ) => ReactNode
  className?: string
}

interface DataTableProps<T> {
  data: T[]
  columns:
    DataTableColumn<T>[]
  getRowKey: (
    item: T,
  ) => string | number
  emptyMessage?: string
  className?: string
}

export function DataTable<T>({
  data,
  columns,
  getRowKey,
  emptyMessage =
    'Nenhum registro encontrado.',
  className = '',
}: DataTableProps<T>) {
  return (
    <div className="admin-table-wrap">
      <table
        className={`data-table admin-table ${className}`.trim()}
      >
        <thead>
          <tr>
            {columns.map(
              (column) => (
                <th
                  key={
                    column.key
                  }
                  className={
                    column.className
                  }
                >
                  {
                    column.header
                  }
                </th>
              ),
            )}
          </tr>
        </thead>

        <tbody>
          {data.map(
            (item) => (
              <tr
                key={
                  getRowKey(
                    item,
                  )
                }
              >
                {columns.map(
                  (column) => (
                    <td
                      key={
                        column.key
                      }
                      className={
                        column.className
                      }
                    >
                      {column.render(
                        item,
                      )}
                    </td>
                  ),
                )}
              </tr>
            ),
          )}

          {data.length === 0 ? (
            <tr>
              <td
                colSpan={
                  columns.length
                }
              >
                {emptyMessage}
              </td>
            </tr>
          ) : null}
        </tbody>
      </table>
    </div>
  )
}