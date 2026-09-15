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

  const resultadoBruto =
    opcoes.reduce(
      (total, opcao) =>
        total +
        (opcao.resultadoBruto ??
          0),
      0,
    )

  const irEstimado =
    opcoes.reduce(
      (total, opcao) =>
        total +
        (opcao.irEstimado ??
          0),
      0,
    )

  const resultadoLiquido =
    opcoes.reduce(
      (total, opcao) =>
        total +
        (opcao.resultadoLiquido ??
          0),
      0,
    )

  const percentualPremioGanho =
    premioTotal !== 0
      ? (resultadoLiquido /
          premioTotal) *
        100
      : 0

  return (
    <div className="options-summary-grid">
      <article className="options-summary-card options-summary-highlight">
        <span>
          Resultado Líquido
        </span>

        <strong
          className={
            resultadoLiquido >= 0
              ? 'positive'
              : 'negative'
          }
        >
          {formatarMoeda(
            resultadoLiquido,
          )}
        </strong>

        <small>
          Bruto{' '}
          {formatarMoeda(
            resultadoBruto,
          )}
        </small>
      </article>

      <article className="options-summary-card">
        <span>
          IR Estimado
        </span>

        <strong>
          {formatarMoeda(
            irEstimado,
          )}
        </strong>

        <small>
          Estimativa por operação
        </small>
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

        <small>
          Sobre o resultado líquido
        </small>
      </article>
    </div>
  )
}