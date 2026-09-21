import type { Investidor } from '../types/investidor'

interface PageHeaderProps {
  titulo: string
  investidores: readonly Investidor[]
  selectedInvestor: string
  onSelectInvestor: (nome: string) => void
  incluirTodos?: boolean
  rotuloTodos?: string
}

export function PageHeader({
  titulo,
  investidores,
  selectedInvestor,
  onSelectInvestor,
  incluirTodos = false,
  rotuloTodos = 'Todos',
}: PageHeaderProps) {
  return (
    <div className="portfolio-toolbar page-header">
      <div className="page-header-content">
        <h1 className="page-header-title">{titulo}</h1>

        <label className="investor-select-label">
          <span>Investidor</span>

          <select
            value={selectedInvestor}
            onChange={(event) =>
              onSelectInvestor(event.target.value)
            }
          >
            {incluirTodos ? (
              <option value="TOTAL">{rotuloTodos}</option>
            ) : null}

            {investidores.map((investidor) => (
              <option
                key={investidor.id}
                value={investidor.nome}
              >
                {investidor.nome}
              </option>
            ))}
          </select>
        </label>
      </div>
    </div>
  )
}