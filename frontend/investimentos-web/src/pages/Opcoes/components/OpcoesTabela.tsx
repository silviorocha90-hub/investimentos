import { SortableColumnHeader } from '../../../components/SortableColumnHeader'
import { dateSortValue, useTableSort } from '../../../components/useTableSort'
import type { SortColumn } from '../../../components/useTableSort'
import {
  useEffect,
  useState,
} from 'react'

import type {
  OperacaoOpcao,
} from '../../../types/dashboard'

import {
  formatarDataCurta,
  formatarMoeda,
  formatarNumeroInteiro,
} from '../../../utils/formatters'

interface OpcoesTabelaProps {
  opcoes: readonly OperacaoOpcao[]
  salvando?: boolean
  modoAdministracao?: boolean

  onNovaOpcao?: () => void

  onEditar?: (
    opcao: OperacaoOpcao,
  ) => void

  onExcluir?: (
    opcao: OperacaoOpcao,
  ) => void
}

function formatarPercentual(
  valor: number,
) {
  return `${valor.toLocaleString(
    'pt-BR',
    {
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    },
  )}%`
}

const dataReferencia = (opcao: OperacaoOpcao) => opcao.dataFinalizacao
  ? formatarDataCurta(opcao.dataFinalizacao)
  : opcao.situacao === 'EXECUTADA' ? formatarDataCurta(opcao.dataOperacao) : '—'
const colunasOpcoes: readonly SortColumn<OperacaoOpcao>[] = [
  { id: 'referencia', label: 'Data de Referência', value: o => dateSortValue(o.dataFinalizacao ?? (o.situacao === 'EXECUTADA' ? o.dataOperacao : null)) },
  { id: 'vencimento', label: 'Vencimento', value: o => dateSortValue(o.vencimento) },
  { id: 'ticker', label: 'Ticker', value: o => o.tickerOpcao },
  { id: 'tipo', label: 'Tipo', value: o => o.tipoOpcao },
  { id: 'status', label: 'Status', value: o => o.situacao },
  { id: 'quantidade', label: 'Qtd.', value: o => o.quantidade },
  { id: 'strike', label: 'Strike', value: o => o.strike },
  { id: 'bruto', label: 'Resultado Bruto', value: o => o.resultadoBruto },
  { id: 'ir', label: 'IR', value: o => o.resultadoBruto == null ? null : o.irEstimado },
  { id: 'liquido', label: 'Resultado Líquido', value: o => o.resultadoLiquido },
]

export function OpcoesTabela({
  opcoes,
  salvando = false,
  modoAdministracao = false,
  onNovaOpcao,
  onEditar,
  onExcluir,
}: OpcoesTabelaProps) {
  const [pagina, setPagina] =
    useState(1)

  const ordenacao = useTableSort(opcoes, colunasOpcoes)
  const opcoesOrdenadas = ordenacao.sortedRows
  const itensPorPagina = 10

  const totalPaginas =
    Math.max(
      1,
      Math.ceil(
        opcoesOrdenadas.length /
          itensPorPagina,
      ),
    )

  const paginaSegura =
    Math.min(
      pagina,
      totalPaginas,
    )

  const opcoesExibidas =
    opcoesOrdenadas.slice(
      (paginaSegura - 1) *
        itensPorPagina,
      paginaSegura *
        itensPorPagina,
    )

  useEffect(() => {
    if (pagina > totalPaginas) {
      setPagina(totalPaginas)
    }
  }, [pagina, totalPaginas])

  useEffect(() => {
    setPagina(1)
  }, [opcoes, ordenacao.sort])

  return (
    <article className="panel options-table-panel">
      <div className="options-list-heading">
        <div className="options-list-title">
          <strong>
            Lista de Opções
          </strong>

          <span className="options-count-badge">
            {opcoesOrdenadas.length}{' '}
            {opcoesOrdenadas.length === 1
              ? 'opção'
              : 'opções'}
          </span>
        </div>

        {modoAdministracao &&
        onNovaOpcao ? (
          <button
            type="button"
            className="options-new-button"
            disabled={salvando}
            onClick={onNovaOpcao}
          >
            + Nova Opção
          </button>
        ) : null}
      </div>

      {opcoes.length > 0 ? (
        <div className="options-table-wrap">
          <table className="data-table options-table">
            <thead>
              <tr>
                {colunasOpcoes.map(coluna => <SortableColumnHeader key={coluna.id} label={coluna.label} direction={ordenacao.sort?.column === coluna.id ? ordenacao.sort.direction : undefined} onSort={() => { ordenacao.toggle(coluna.id); setPagina(1) }} />)}

                {modoAdministracao ? (
                  <th>Ações</th>
                ) : null}
              </tr>
            </thead>

            <tbody>
              {opcoesExibidas.map(
                (opcao) => {
                  const resultadoBruto =
                    opcao.resultadoBruto

                  const resultadoLiquido =
                    opcao.resultadoLiquido

                  return (
                    <tr key={opcao.id}>
                      <td>
                        {dataReferencia(opcao)}
                      </td>

                      <td>
                        {formatarDataCurta(
                          opcao.vencimento,
                        )}
                      </td>

                      <td>
                        <span className="ticker">
                          {
                            opcao.tickerOpcao
                          }
                        </span>
                      </td>

                      <td>
                        <span
                          className={`option-type-chip ${opcao.tipoOpcao.toLowerCase()}`}
                        >
                          {
                            opcao.tipoOpcao
                          }
                        </span>
                      </td>

                      <td>
                        <span
                          className={`option-status-chip ${opcao.situacao.toLowerCase()}`}
                        >
                          {
                            opcao.situacao
                          }
                        </span>
                      </td>

                      <td className="align-right">
                        {formatarNumeroInteiro(
                          opcao.quantidade,
                        )}
                      </td>

                      <td className="align-right">
                        {formatarMoeda(
                          opcao.strike,
                        )}
                      </td>

                      <td
                        className={`align-right strong ${
                          (resultadoBruto ??
                            0) >= 0
                            ? 'positive'
                            : 'negative'
                        }`}
                      >
                        {resultadoBruto != null
                          ? formatarMoeda(
                              resultadoBruto,
                            )
                          : '—'}
                      </td>

                      <td className="align-right">
                        {opcao.resultadoBruto != null
                          ? `${formatarMoeda(
                              opcao.irEstimado,
                            )} (${formatarPercentual(
                              opcao.aliquotaIr,
                            )})`
                          : '—'}
                      </td>

                      <td
                        className={`align-right strong ${
                          (resultadoLiquido ??
                            0) >= 0
                            ? 'positive'
                            : 'negative'
                        }`}
                      >
                        {resultadoLiquido != null
                          ? formatarMoeda(
                              resultadoLiquido,
                            )
                          : '—'}
                      </td>

                      {modoAdministracao ? (
                        <td>
                          <div className="options-row-actions">
                            {onEditar ? (
                              <button
                                type="button"
                                className="options-edit-button"
                                onClick={() =>
                                  onEditar(
                                    opcao,
                                  )
                                }
                              >
                                Editar
                              </button>
                            ) : null}

                            {onExcluir ? (
                              <button
                                type="button"
                                className="options-delete-button"
                                disabled={
                                  salvando
                                }
                                onClick={() =>
                                  onExcluir(
                                    opcao,
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
                  )
                },
              )}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="empty-state options-empty-state">
          <strong>
            Nenhuma opção encontrada
          </strong>

          {modoAdministracao &&
          onNovaOpcao ? (
            <button
              type="button"
              className="options-new-button"
              disabled={salvando}
              onClick={onNovaOpcao}
            >
              + Nova Opção
            </button>
          ) : (
            <span>
              Não há opções para os filtros selecionados.
            </span>
          )}
        </div>
      )}

      {totalPaginas > 1 ? (
        <div className="proventos-pagination admin-table-pagination options-pagination">
          <span>
            {opcoesOrdenadas.length}{' '}
            registros
          </span>

          <div>
            <button
              type="button"
              disabled={
                paginaSegura <= 1
              }
              onClick={() =>
                setPagina((atual) =>
                  Math.max(
                    1,
                    atual - 1,
                  ),
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
                setPagina((atual) =>
                  Math.min(
                    totalPaginas,
                    atual + 1,
                  ),
                )
              }
            >
              ›
            </button>
          </div>
        </div>
      ) : null}
    </article>
  )
}