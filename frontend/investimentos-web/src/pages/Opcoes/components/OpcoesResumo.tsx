import type {
  OperacaoOpcao,
} from '../../../types/dashboard'
import {
  formatarMoeda,
} from '../../../utils/formatters'

interface OpcoesResumoProps {
  opcoes: readonly OperacaoOpcao[]
}

function formatarPercentual(
  valor: number,
) {
  return `${valor.toLocaleString(
    'pt-BR',
    {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    },
  )}%`
}

export function OpcoesResumo({
  opcoes,
}: OpcoesResumoProps) {
  const premioTotal =
    opcoes.reduce(
      (total, opcao) =>
        total +
        opcao.premioTotal,
      0,
    )

  const resultadoTotal =
    opcoes.reduce(
      (total, opcao) =>
        total +
        (opcao.resultadoInformado ??
          opcao.resultadoFinal ??
          0),
      0,
    )

  const percentualPremioGanho =
    premioTotal !== 0
      ? (resultadoTotal /
          premioTotal) *
        100
      : 0

  return (
    <div className="options-summary-grid">
      <article className="options-summary-card options-summary-highlight">
        <span>
          Resultado
        </span>

        <strong
          className={
            resultadoTotal >= 0
              ? 'positive'
              : 'negative'
          }
        >
          {formatarMoeda(
            resultadoTotal,
          )}
        </strong>
      </article>

      <article className="options-summary-card">
        <span>
          % do Prêmio Ganho
        </span>

        <strong
          className={
            percentualPremioGanho >= 0
              ? 'positive'
              : 'negative'
          }
        >
          {formatarPercentual(
            percentualPremioGanho,
          )}
        </strong>
      </article>
    </div>
  )
}