import type {
  OperacaoOpcao,
} from '../../../types/dashboard'
import {
  formatarMoeda,
} from '../../../utils/formatters'

interface OpcoesGraficosProps {
  opcoes: readonly OperacaoOpcao[]
}

const MESES = [
  'Jan',
  'Fev',
  'Mar',
  'Abr',
  'Mai',
  'Jun',
  'Jul',
  'Ago',
  'Set',
  'Out',
  'Nov',
  'Dez',
]

function obterResultado(
  opcao: OperacaoOpcao,
) {
  return (
    opcao.resultadoInformado ??
    opcao.resultadoFinal ??
    0
  )
}

function obterDataReferencia(
  opcao: OperacaoOpcao,
) {
  return new Date(
    opcao.dataFinalizacao ??
      opcao.dataOperacao,
  )
}

export function OpcoesGraficos({
  opcoes,
}: OpcoesGraficosProps) {
  const anos =
    opcoes
      .map((opcao) =>
        obterDataReferencia(
          opcao,
        ).getFullYear(),
      )
      .filter(
        (ano) =>
          Number.isFinite(ano),
      )

  const ano =
    anos.length > 0
      ? Math.max(...anos)
      : new Date().getFullYear()

  const mensais =
    MESES.map(
      (mes, index) => ({
        mes,

        valor: opcoes
          .filter((opcao) => {
            const data =
              obterDataReferencia(
                opcao,
              )

            return (
              data.getFullYear() ===
                ano &&
              data.getMonth() ===
                index
            )
          })
          .reduce(
            (total, opcao) =>
              total +
              obterResultado(
                opcao,
              ),
            0,
          ),
      }),
    )

  const maiorMes =
    Math.max(
      ...mensais.map((item) =>
        Math.abs(item.valor),
      ),
      1,
    )

  const porTicker =
    Array.from(
      opcoes
        .filter(
          (opcao) =>
            obterDataReferencia(
              opcao,
            ).getFullYear() ===
            ano,
        )
        .reduce(
          (mapa, opcao) => {
            mapa.set(
              opcao.tickerOpcao,
              (mapa.get(
                opcao.tickerOpcao,
              ) ?? 0) +
                obterResultado(
                  opcao,
                ),
            )

            return mapa
          },
          new Map<
            string,
            number
          >(),
        ),
    )
      .map(
        ([ticker, valor]) => ({
          ticker,
          valor,
        }),
      )
      .sort(
        (a, b) =>
          Math.abs(b.valor) -
          Math.abs(a.valor),
      )
      .slice(0, 8)

  const maiorTicker =
    Math.max(
      ...porTicker.map((item) =>
        Math.abs(item.valor),
      ),
      1,
    )

  return (
    <div className="options-charts-grid">
      <article className="panel options-chart-card">
        <header className="options-chart-header">
          <strong>
            Evolução
          </strong>
        </header>

        <div className="options-month-chart">
          {mensais.map(
            (item) => {
              const percentual =
                item.valor !== 0
                  ? Math.max(
                      (Math.abs(
                        item.valor,
                      ) /
                        maiorMes) *
                        100,
                      4,
                    )
                  : 0

              return (
                <div
                  className="options-month-column"
                  key={item.mes}
                  title={`${item.mes}: ${formatarMoeda(
                    item.valor,
                  )}`}
                >
                  <div
                    className={`options-month-value ${
                      item.valor < 0
                        ? 'negative'
                        : ''
                    }`}
                  >
                    {item.valor !== 0
                      ? formatarMoeda(
                          item.valor,
                        )
                      : ''}
                  </div>

                  <div className="options-month-track">
                    <i
                      className={
                        item.valor < 0
                          ? 'negative'
                          : ''
                      }
                      style={{
                        height: `${percentual}%`,
                      }}
                    />
                  </div>

                  <span>
                    {item.mes}
                  </span>
                </div>
              )
            },
          )}
        </div>
      </article>

      <article className="panel options-chart-card">
        <header className="options-chart-header">
          <strong>
            Ranking
          </strong>
        </header>

        {porTicker.length > 0 ? (
          <div className="options-ranking">
            {porTicker.map(
              (
                item,
                index,
              ) => (
                <div
                  className="options-ranking-row"
                  key={item.ticker}
                >
                  <span className="options-ranking-position">
                    {index + 1}
                  </span>

                  <strong>
                    {item.ticker}
                  </strong>

                  <div className="options-ranking-track">
                    <i
                      className={
                        item.valor < 0
                          ? 'negative'
                          : ''
                      }
                      style={{
                        width: `${Math.max(
                          (Math.abs(
                            item.valor,
                          ) /
                            maiorTicker) *
                            100,
                          3,
                        )}%`,
                      }}
                    />
                  </div>

                  <span
                    className={`options-ranking-value ${
                      item.valor < 0
                        ? 'negative'
                        : ''
                    }`}
                  >
                    {formatarMoeda(
                      item.valor,
                    )}
                  </span>
                </div>
              ),
            )}
          </div>
        ) : (
          <div className="options-chart-empty">
            Nenhum resultado no ano.
          </div>
        )}
      </article>
    </div>
  )
}