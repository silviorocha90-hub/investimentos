import type {
  Provento,
} from '../../../types/dashboard'

import {
  formatarMoeda,
  formatarNumeroInteiro,
} from '../../../utils/formatters'

interface ProventosTabelaProps {
  proventos: readonly Provento[]
  pagina: number
  itensPorPagina: number

  onPaginaChange: (
    pagina: number,
  ) => void

  modoAdministracao?: boolean

  onEditar?: (
    provento: Provento,
  ) => void

  onExcluir?: (
    provento: Provento,
  ) => void
}

function formatarData(
  valor?: string | null,
) {
  if (!valor) {
    return '—'
  }

  return new Date(
    valor,
  ).toLocaleDateString(
    'pt-BR',
    {
      timeZone: 'UTC',
    },
  )
}

export function ProventosTabela({
  proventos,
  pagina,
  itensPorPagina,
  onPaginaChange,
  modoAdministracao = false,
  onEditar,
  onExcluir,
}: ProventosTabelaProps) {
  const totalPaginas =
    Math.max(
      1,
      Math.ceil(
        proventos.length /
          itensPorPagina,
      ),
    )

  const paginaSegura =
    Math.min(
      pagina,
      totalPaginas,
    )

  const inicio =
    (paginaSegura - 1) *
    itensPorPagina

  const itens =
    proventos.slice(
      inicio,
      inicio +
        itensPorPagina,
    )

  return (
    <>
      <div className="proventos-table-wrap">
        <table className="proventos-table">
          <thead>
            <tr>
              <th>
                Pagamento
              </th>

              <th>
                Ativo
              </th>

              <th>
                Tipo
              </th>

              <th>
                Qtd.
              </th>

              <th>
                Valor/un.
              </th>

              <th>
                Recebido
              </th>

              <th>
                IR
              </th>

              {modoAdministracao ? (
                <th>
                  Ações
                </th>
              ) : null}
            </tr>
          </thead>

          <tbody>
            {itens.map(
              (item) => (
                <tr
                  key={
                    item.id
                  }
                >
                  <td>
                    {formatarData(
                      item.dataPagamento,
                    )}
                  </td>

                  <td>
                    <span className="ticker">
                      {
                        item.ticker
                      }
                    </span>
                  </td>

                  <td>
                    <span
                      className={`provento-tipo provento-tipo-${item.tipo.toLowerCase()}`}
                    >
                      {item.tipo === 'JCP' ? 'Juros' : item.tipo}
                    </span>
                  </td>

                  <td className="align-right">
                    {formatarNumeroInteiro(
                      item.quantidadeBase,
                    )}
                  </td>

                  <td className="align-right">
                    {formatarMoeda(
                      item.valorPorUnidade,
                    )}
                  </td>

                  <td className={`align-right provento-recebido ${item.valorRecebido >= 0 ? 'positive' : 'negative'}`}>
                    {formatarMoeda(
                      item.valorRecebido,
                    )}
                  </td>

                  <td className={`align-right ${item.impostoRetido > 0 ? 'negative' : ''}`}>
                    {formatarMoeda(
                      item.impostoRetido,
                    )}
                  </td>

                  {modoAdministracao ? (
                    <td>
                      <div className="admin-row-actions">
                        {onEditar ? (
                          <button
                            type="button"
                            className="admin-action"
                            onClick={() =>
                              onEditar(
                                item,
                              )
                            }
                          >
                            Editar
                          </button>
                        ) : null}

                        {onExcluir ? (
                          <button
                            type="button"
                            className="admin-action admin-action-danger"
                            onClick={() =>
                              onExcluir(
                                item,
                              )
                            }
                          >
                            Excluir
                          </button>
                        ) : null}
                      </div>
                    </td>
                  ) : null}
                </tr>
              ),
            )}
          </tbody>
        </table>
      </div>

      {proventos.length ===
      0 ? (
        <div className="proventos-empty">
          <strong>
            Nenhum provento encontrado
          </strong>

          <span>
            Ajuste os filtros para consultar outros registros.
          </span>
        </div>
      ) : null}

      {totalPaginas >
      1 ? (
        <div className="proventos-pagination">
          <span>
            {proventos.length}{' '}
            registros
          </span>

          <div>
            <button
              type="button"
              disabled={
                paginaSegura <=
                1
              }
              onClick={() =>
                onPaginaChange(
                  paginaSegura -
                    1,
                )
              }
            >
              ‹
            </button>

            <strong>
              {paginaSegura}{' '}
              /{' '}
              {totalPaginas}
            </strong>

            <button
              type="button"
              disabled={
                paginaSegura >=
                totalPaginas
              }
              onClick={() =>
                onPaginaChange(
                  paginaSegura +
                    1,
                )
              }
            >
              ›
            </button>
          </div>
        </div>
      ) : null}
    </>
  )
}