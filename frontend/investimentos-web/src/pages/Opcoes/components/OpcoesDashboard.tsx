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

function dataLocal(
  valor: string,
) {
  return new Date(
    `${valor.slice(0, 10)}T12:00:00`,
  )
}

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

  const risco =
    ativas.filter(
      (opcao) =>
        opcao.emRiscoExercicio,
    )

  const hoje =
    new Date()
  hoje.setHours(0, 0, 0, 0)

  const vencimentos =
    ativas
      .filter(
        (opcao) =>
          dataLocal(
            opcao.vencimento,
          ) >= hoje,
      )
      .slice()
      .sort(
        (a, b) =>
          a.vencimento.localeCompare(
            b.vencimento,
          ),
      )
      .slice(0, 6)

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
        </article>

        <article className="options-summary-card">
          <span>
            Exposição a exercício
          </span>
          <strong
            className={
              risco.length > 0
                ? 'negative'
                : 'positive'
            }
          >
            {risco.length}
          </strong>
          <small>
            Opções ativas no dinheiro
          </small>
        </article>
      </div>

      <div className="options-exposure-grid">
        <article className="panel options-exposure-card">
          <header className="options-chart-header">
            <strong>
              Próximos vencimentos
            </strong>
          </header>

          {vencimentos.length > 0 ? (
            <div className="options-exposure-list">
              {vencimentos.map(
                (opcao) => (
                  <div
                    className="options-exposure-row"
                    key={opcao.id}
                  >
                    <strong>
                      {opcao.tickerOpcao}
                    </strong>
                    <span>
                      {dataLocal(
                        opcao.vencimento,
                      ).toLocaleDateString(
                        'pt-BR',
                      )}
                    </span>
                    <span>
                      {opcao.tipoOpcao}{' '}
                      {opcao.natureza}
                    </span>
                    <b>
                      {formatarMoeda(
                        opcao.strike,
                      )}
                    </b>
                  </div>
                ),
              )}
            </div>
          ) : (
            <div className="options-chart-empty">
              Nenhuma opção ativa com vencimento futuro.
            </div>
          )}
        </article>

        <article className="panel options-exposure-card">
          <header className="options-chart-header">
            <strong>
              Exposição a exercício
            </strong>
          </header>

          {risco.length > 0 ? (
            <div className="options-exposure-list">
              {risco
                .slice()
                .sort(
                  (a, b) =>
                    Math.abs(
                      b.distanciaStrikePercentual,
                    ) -
                    Math.abs(
                      a.distanciaStrikePercentual,
                    ),
                )
                .map(
                  (opcao) => (
                    <div
                      className="options-exposure-row"
                      key={opcao.id}
                    >
                      <strong>
                        {opcao.tickerOpcao}
                      </strong>
                      <span>
                        {opcao.tickerAtivo}
                      </span>
                      <span>
                        Strike{' '}
                        {formatarMoeda(
                          opcao.strike,
                        )}
                      </span>
                      <b className="negative">
                        {percentual.format(
                          opcao.distanciaStrikePercentual,
                        )}%
                      </b>
                    </div>
                  ),
                )}
            </div>
          ) : (
            <div className="options-chart-empty">
              Nenhuma opção ativa no dinheiro.
            </div>
          )}
        </article>
      </div>
    </>
  )
}
