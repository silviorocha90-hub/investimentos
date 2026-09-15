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
  salvando: boolean

  onNovaOpcao: () => void

  onEditar: (
    opcao: OperacaoOpcao,
  ) => void

  onExcluir: (
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
  salvando,
  onNovaOpcao,
  onEditar,
  onExcluir,
}: OpcoesTabelaProps) {
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

        <button
          type="button"
          className="options-new-button"
          disabled={salvando}
          onClick={onNovaOpcao}
        >
          + Nova Opção
        </button>
      </div>

      {opcoes.length > 0 ? (
        <div className="options-table-wrap">
          <table className="data-table options-table">
            <thead>
              <tr>
                <th>Vencimento</th>
                <th>Ticker</th>
                <th>Tipo</th>
                <th>Status</th>
                <th>Qtd.</th>
                <th>Strike</th>
                <th>Resultado Bruto</th>
                <th>Regime</th>
                <th>IR</th>
                <th>Resultado Líquido</th>
                <th>Ações</th>
              </tr>
            </thead>

            <tbody>
              {opcoes.map(
                (opcao) => {
                  const resultadoBruto =
                    opcao.resultadoBruto

                  const resultadoLiquido =
                    opcao.resultadoLiquido

                  return (
                    <tr key={opcao.id}>
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

                      <td>
                        {opcao.regimeTributario ===
                        'DAY_TRADE'
                          ? 'Day Trade'
                          : 'Comum'}
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

                      <td>
                        <div className="options-row-actions">
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
                        </div>
                      </td>
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

          <button
            type="button"
            className="options-new-button"
            disabled={salvando}
            onClick={onNovaOpcao}
          >
            + Nova Opção
          </button>
        </div>
      )}
    </article>
  )
}