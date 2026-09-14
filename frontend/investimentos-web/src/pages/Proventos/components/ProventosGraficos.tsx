import type {
  Provento,
} from '../../../types/dashboard'
import {
  formatarMoeda,
} from '../../../utils/formatters'

interface ProventosGraficosProps {
  proventos: readonly Provento[]
  ano: number
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

export function ProventosGraficos({
  proventos,
  ano,
}: ProventosGraficosProps) {
  const mensais =
    MESES.map(
      (mes, index) => ({
        mes,

        valor: proventos
          .filter((item) => {
            const data =
              new Date(
                item.dataPagamento,
              )

            return (
              data.getFullYear() ===
                ano &&
              data.getMonth() ===
                index
            )
          })
          .reduce(
            (total, item) =>
              total +
              item.valorRecebido,
            0,
          ),
      }),
    )

  const maiorMes =
    Math.max(
      ...mensais.map(
        (item) =>
          item.valor,
      ),
      1,
    )

  const porAtivo =
    Array.from(
      proventos
        .filter(
          (item) =>
            new Date(
              item.dataPagamento,
            ).getFullYear() ===
            ano,
        )
        .reduce(
          (mapa, item) => {
            mapa.set(
              item.ticker,
              (mapa.get(
                item.ticker,
              ) ?? 0) +
                item.valorRecebido,
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
          b.valor -
          a.valor,
      )
      .slice(0, 8)

  const maiorAtivo =
    Math.max(
      ...porAtivo.map(
        (item) =>
          item.valor,
      ),
      1,
    )

  return (
    <div className="proventos-graficos">
      <article className="panel proventos-chart-card">
        <header className="proventos-chart-header">
          <div>
            <span>
              Evolução
            </span>

            <strong>
              Proventos por mês
            </strong>
          </div>

          <small>
            {ano}
          </small>
        </header>

        <div className="proventos-month-chart">
          {mensais.map(
            (item) => (
              <div
                className="proventos-month-column"
                key={
                  item.mes
                }
                title={`${item.mes}: ${formatarMoeda(
                  item.valor,
                )}`}
              >
                <div className="proventos-month-value">
                  {item.valor > 0
                    ? formatarMoeda(
                        item.valor,
                      )
                    : ''}
                </div>

                <div className="proventos-month-track">
                  <i
                    style={{
                      height: `${
                        item.valor >
                        0
                          ? Math.max(
                              (item.valor /
                                maiorMes) *
                                100,
                              4,
                            )
                          : 0
                      }%`,
                    }}
                  />
                </div>

                <span>
                  {item.mes}
                </span>
              </div>
            ),
          )}
        </div>
      </article>

      <article className="panel proventos-chart-card">
        <header className="proventos-chart-header">
          <div>
            <span>
              Ranking
            </span>

            <strong>
              Proventos por ativo
            </strong>
          </div>

          <small>
            Top {porAtivo.length}
          </small>
        </header>

        {porAtivo.length >
        0 ? (
          <div className="proventos-ranking">
            {porAtivo.map(
              (
                item,
                index,
              ) => (
                <div
                  className="proventos-ranking-row"
                  key={
                    item.ticker
                  }
                >
                  <span className="proventos-ranking-position">
                    {index +
                      1}
                  </span>

                  <strong>
                    {
                      item.ticker
                    }
                  </strong>

                  <div className="proventos-ranking-track">
                    <i
                      style={{
                        width: `${Math.max(
                          (item.valor /
                            maiorAtivo) *
                            100,
                          3,
                        )}%`,
                      }}
                    />
                  </div>

                  <span className="proventos-ranking-value">
                    {formatarMoeda(
                      item.valor,
                    )}
                  </span>
                </div>
              ),
            )}
          </div>
        ) : (
          <div className="proventos-chart-empty">
            Nenhum provento no ano.
          </div>
        )}
      </article>
    </div>
  )
}