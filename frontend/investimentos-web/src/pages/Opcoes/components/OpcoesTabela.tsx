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

  const itensPorPagina = 10

  const totalPaginas =
    Math.max(
      1,
      Math.ceil(
        opcoes.length /
          itensPorPagina,
      ),
    )

  const paginaSegura =
    Math.min(
      pagina,
      totalPaginas,
    )

  const opcoesExibidas =
    opcoes.slice(
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
  }, [opcoes])

  return (
    <article className="panel options-table-panel">
      <div className="options-list-heading">
        <div className="options-list-title">
          <strong>
            Lista de Opções
          </strong>

          <span className="options-count-badge">
            {opcoes.length}{' '}
            {opcoes.length === 1
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
                <th>Finalização</th>
                <th>Vencimento</th>
                <th>Ticker</th>
                <th>Tipo</th>
                <th>Status</th>
                <th>Qtd.</th>
                <th>Strike</th>
                <th>
                  Resultado Bruto
                </th>
                <th>IR</th>
                <th>
                  Resultado Líquido
                </th>

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
                        {opcao.dataFinalizacao
                          ? formatarDataCurta(opcao.dataFinalizacao)
                          : opcao.situacao === 'EXECUTADA'
                            ? formatarDataCurta(opcao.dataOperacao)
                            : '—'}
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
            {opcoes.length}{' '}
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