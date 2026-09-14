import {
  formatarDataCurta,
} from '../../../utils/formatters'

interface OpcoesFiltrosProps {
  tickers: readonly string[]
  vencimentos: readonly string[]

  ticker: string
  tipo: string
  natureza: string
  status: string
  vencimento: string

  onTickerChange: (
    valor: string,
  ) => void

  onTipoChange: (
    valor: string,
  ) => void

  onNaturezaChange: (
    valor: string,
  ) => void

  onStatusChange: (
    valor: string,
  ) => void

  onVencimentoChange: (
    valor: string,
  ) => void

  onLimpar: () => void
}

export function OpcoesFiltros({
  tickers,
  vencimentos,
  ticker,
  tipo,
  natureza,
  status,
  vencimento,
  onTickerChange,
  onTipoChange,
  onNaturezaChange,
  onStatusChange,
  onVencimentoChange,
  onLimpar,
}: OpcoesFiltrosProps) {
  return (
    <article className="panel options-filters-panel">
      <div className="options-filter-heading">
        <strong>
          Filtros
        </strong>
      </div>

      <div className="options-filters">
        <label>
          <span>
            Ticker
          </span>

          <select
            value={ticker}
            onChange={(event) =>
              onTickerChange(
                event.target.value,
              )
            }
          >
            <option value="TODOS">
              Todos
            </option>

            {tickers.map(
              (item) => (
                <option
                  key={item}
                  value={item}
                >
                  {item}
                </option>
              ),
            )}
          </select>
        </label>

        <label>
          <span>
            Tipo
          </span>

          <select
            value={tipo}
            onChange={(event) =>
              onTipoChange(
                event.target.value,
              )
            }
          >
            <option value="TODOS">
              Todos
            </option>

            <option value="CALL">
              CALL
            </option>

            <option value="PUT">
              PUT
            </option>
          </select>
        </label>

        <label>
          <span>
            Natureza
          </span>

          <select
            value={natureza}
            onChange={(event) =>
              onNaturezaChange(
                event.target.value,
              )
            }
          >
            <option value="TODOS">
              Todas
            </option>

            <option value="COMPRA">
              COMPRA
            </option>

            <option value="VENDA">
              VENDA
            </option>
          </select>
        </label>

        <label>
          <span>
            Status
          </span>

          <select
            value={status}
            onChange={(event) =>
              onStatusChange(
                event.target.value,
              )
            }
          >
            <option value="TODOS">
              Todos
            </option>

            <option value="EXECUTADA">
              EXECUTADA
            </option>

            <option value="ENCERRADA">
              ENCERRADA
            </option>
          </select>
        </label>

        <label>
          <span>
            Vencimento
          </span>

          <select
            value={vencimento}
            onChange={(event) =>
              onVencimentoChange(
                event.target.value,
              )
            }
          >
            <option value="TODOS">
              Todos
            </option>

            {vencimentos.map(
              (item) => (
                <option
                  key={item}
                  value={item}
                >
                  {formatarDataCurta(
                    item,
                  )}
                </option>
              ),
            )}
          </select>
        </label>

        <button
          type="button"
          className="options-clear-button"
          onClick={onLimpar}
        >
          Limpar filtros
        </button>
      </div>
    </article>
  )
}