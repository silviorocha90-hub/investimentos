import type {
  OperacaoOpcao,
} from '../../../types/dashboard'
import {
  formatarMoeda,
} from '../../../utils/formatters'

interface Props {
  opcoes: readonly OperacaoOpcao[]
}

const quantidade =
  new Intl.NumberFormat('pt-BR', {
    maximumFractionDigits: 0,
  })

const percentual =
  new Intl.NumberFormat('pt-BR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })

export function OpcoesDashboard({
  opcoes,
}: Props) {
  const ativas =
    opcoes.filter(
      (opcao) =>
        opcao.estaAtiva,
    )

  const capitalPut =
    ativas.reduce(
      (total, opcao) =>
        total +
        opcao.capitalComprometidoPut,
      0,
    )

  const acoesCall =
    ativas.reduce(
      (total, opcao) =>
        total +
        opcao.acoesComprometidasCall,
      0,
    )

  const premioAtivo =
    ativas.reduce(
      (total, opcao) =>
        total +
        opcao.premioRecebidoAtivo,
      0,
    )

  const retornoCapital =
    capitalPut > 0
      ? premioAtivo /
        capitalPut *
        100
      : 0

  return (
    <>
      <div className="options-advanced-grid">
        <article className="options-summary-card options-summary-highlight">
          <span>Capital em PUT</span>
          <strong>
            {formatarMoeda(capitalPut)}
          </strong>
          <small>
            Strike × quantidade comprometida
          </small>
        </article>

        <article className="options-summary-card">
          <span>Ações em CALL</span>
          <strong>
            {quantidade.format(
              acoesCall,
            )}
          </strong>
          <small>
            Cobertura atualmente comprometida
          </small>
        </article>

        <article className="options-summary-card">
          <span>Prêmios ativos</span>
          <strong className="positive">
            {formatarMoeda(
              premioAtivo,
            )}
          </strong>
          <small>
            Prêmio recebido menos taxas
          </small>
        </article>

        <article className="options-summary-card">
          <span>
            Retorno / capital PUT
          </span>
          <strong className="positive">
            {percentual.format(
              retornoCapital,
            )}%
          </strong>
          <small>
            Prêmios ativos sobre capital comprometido
          </small>
        </article></div></>
  )
}
