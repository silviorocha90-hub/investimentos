import { useMemo } from 'react'
import { SectionTitle } from '../../components/SectionTitle'
import type {
  Dashboard,
  EvolucaoInvestidor,
} from '../../types/dashboard'
import {
  formatarMesCurto,
  formatarMilhares,
  formatarMilharesInteiros,
  formatarMoeda,
  normalizarDataEvolucao,
  obterCorInvestidor,
  ordenarDecrescente,
} from '../../utils/formatters'

type SimpleMetric = {
  label: string
  value: number
}

interface DashboardViewProps {
  dashboard: Dashboard | null

  carteiras: ReadonlyArray<{
    nome: string
    dashboard: Dashboard
  }>

  evolucao: readonly EvolucaoInvestidor[]
  apiDisponivel: boolean
  erro: string | null
}

function PortfolioReturnBars({
  rentabilidade,
  entradas,
  saidas,
  resultado,
}: {
  rentabilidade: number
  entradas: number
  saidas: number
  resultado: number
}) {
  const fluxoMax = Math.max(entradas, saidas, 1)
  const rentabilidadeWidth = Math.min(Math.abs(rentabilidade), 100)

  const rows = [
    {
      label: 'Rentabilidade',
      subtitle: 'Resultado ' + formatarMoeda(resultado),
      value:
        rentabilidade.toLocaleString('pt-BR', {
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        }) + '%',
      width: rentabilidadeWidth,
      tone: rentabilidade >= 0 ? 'return-positive' : 'return-negative',
    },
    {
      label: 'Entradas',
      subtitle: 'Capital líquido na carteira',
      value: formatarMoeda(entradas),
      width: (entradas / fluxoMax) * 100,
      tone: 'return-entry',
    },
    {
      label: 'Saídas',
      subtitle: 'Capital líquido retirado',
      value: formatarMoeda(saidas),
      width: (saidas / fluxoMax) * 100,
      tone: 'return-exit',
    },
  ]

  return (
    <div className="portfolio-return-list">
      {rows.map((row) => (
        <div className="portfolio-return-row" key={row.label}>
          <div className="portfolio-return-head">
            <div>
              <strong>{row.label}</strong>
              <small>{row.subtitle}</small>
            </div>
            <span>{row.value}</span>
          </div>
          <div className="portfolio-return-track">
            <i
              className={row.tone}
              style={{
                width: `${Math.max(
                  row.width,
                  row.width > 0 ? 2 : 0,
                )}%`,
              }}
            />
          </div>
        </div>
      ))}
    </div>
  )
}

function InvestorAppliedBars({
  items,
}: {
  items: readonly {
    nome: string
    valor: number
  }[]
}) {
  const max = Math.max(
    ...items.map((item) => item.valor),
    1,
  )

  const itensOrdenados = [
    ...items,
  ].sort(
    (a, b) =>
      b.valor - a.valor,
  )

  return (
    <div className="investor-applied-bars">
      {itensOrdenados.map(
        (item) => (
          <div
            className="investor-applied-item"
            key={item.nome}
          >
            <strong>
              {formatarMilharesInteiros(
                item.valor,
              )}
            </strong>

            <div className="investor-applied-track">
              <i
                style={{
                  height: `${Math.max(
                    (item.valor / max) *
                      100,
                    3,
                  )}%`,
                }}
              />
            </div>

            <span>
              {item.nome}
            </span>
          </div>
        ),
      )}
    </div>
  )
}

function DistributionPieChart({
  items,
  colorForItem,
}: {
  items: readonly SimpleMetric[]

  colorForItem?: (
    label: string,
    index: number,
  ) => string
}) {
  const cores = [
    '#63e6c8',
    '#ffbf69',
    '#8da7ff',
    '#f783ac',
    '#b197fc',
    '#74c0fc',
  ]

  const total = items.reduce(
    (sum, item) =>
      sum +
      Math.max(item.value, 0),
    0,
  )

  let acumulado = 0

  const fatias = items.map(
    (item, index) => {
      const valor = Math.max(
        item.value,
        0,
      )

      const inicio =
        total > 0
          ? acumulado / total
          : 0

      acumulado += valor

      const fim =
        total > 0
          ? acumulado / total
          : 0

      const inicioAngulo =
        inicio * Math.PI * 2 -
        Math.PI / 2

      const fimAngulo =
        fim * Math.PI * 2 -
        Math.PI / 2

      const raio = 78

      const x1 =
        100 +
        raio *
          Math.cos(inicioAngulo)

      const y1 =
        100 +
        raio *
          Math.sin(inicioAngulo)

      const x2 =
        100 +
        raio *
          Math.cos(fimAngulo)

      const y2 =
        100 +
        raio *
          Math.sin(fimAngulo)

      const grandeArco =
        fim - inicio > 0.5
          ? 1
          : 0

      const caminho =
        total === 0
          ? ''
          : `M 100 100 L ${x1} ${y1} A ${raio} ${raio} 0 ${grandeArco} 1 ${x2} ${y2} Z`

      return {
        ...item,

        percentual:
          total > 0
            ? (valor / total) * 100
            : 0,

        caminho,

        cor:
          colorForItem?.(
            item.label,
            index,
          ) ??
          cores[
            index %
              cores.length
          ],
      }
    },
  )

  return (
    <div className="distribution-chart">
      <div className="distribution-pie-wrap">
        <svg
          viewBox="0 0 200 200"
          className="distribution-pie"
          role="img"
          aria-label="Distribuição da carteira"
        >
          {fatias.map(
            (fatia) => (
              <path
                key={fatia.label}
                d={fatia.caminho}
                fill={fatia.cor}
                stroke="var(--panel)"
                strokeWidth="1.5"
              />
            ),
          )}

          <circle
            cx="100"
            cy="100"
            r="42"
            fill="var(--panel-strong)"
          />

          <text
            x="100"
            y="94"
            textAnchor="middle"
            className="distribution-pie-total-label"
          >
            Total
          </text>

          <text
            x="100"
            y="113"
            textAnchor="middle"
            className="distribution-pie-total"
          >
            {formatarMoeda(total)}
          </text>
        </svg>
      </div>

      <div className="distribution-legend">
        {fatias.map(
          (fatia) => (
            <div
              className="distribution-legend-row"
              key={fatia.label}
            >
              <span className="distribution-legend-name">
                <i
                  style={{
                    backgroundColor:
                      fatia.cor,
                  }}
                />

                {fatia.label}
              </span>

              <span className="distribution-legend-value">
                {fatia.percentual.toLocaleString(
                  'pt-BR',
                  {
                    maximumFractionDigits: 1,
                  },
                )}
                %

                <small>
                  {formatarMoeda(
                    fatia.value,
                  )}
                </small>
              </span>
            </div>
          ),
        )}
      </div>
    </div>
  )
}

function MultiTrendChart({
  series,
}: {
  series: readonly EvolucaoInvestidor[]
}) {
  const width = 760
  const height = 70

  const paddingLeft = 56
  const paddingTop = 12
  const paddingRight = 18
  const paddingBottom = 18

  const datas = Array.from(
    new Set(
      series.flatMap(
        (item) =>
          item.pontos.map(
            (ponto) =>
              normalizarDataEvolucao(
                ponto.data,
              ),
          ),
      ),
    ),
  ).sort()

  const valores =
    series.flatMap(
      (item) =>
        item.pontos.map(
          (ponto) =>
            ponto.carteira,
        ),
    )

  if (
    datas.length === 0 ||
    valores.length === 0
  ) {
    return (
      <div className="empty-state">
        <strong>
          Evolução indisponível
        </strong>
      </div>
    )
  }

  const minValue =
    Math.min(...valores)

  const maxValue =
    Math.max(...valores)

  const min =
    minValue >= 0
      ? 0
      : minValue

  const max =
    maxValue === min
      ? min + 1
      : maxValue

  const range =
    max - min || 1

  const chartWidth =
    width -
    paddingLeft -
    paddingRight

  const chartHeight =
    height -
    paddingTop -
    paddingBottom

  const step =
    datas.length > 1
      ? chartWidth /
        (datas.length - 1)
      : 0

  const yLabels = [
    max,
    (max + min) / 2,
    min,
  ]

  return (
    <div className="trend-chart">
      <svg
        viewBox={`0 0 ${width} ${height}`}
        className="trend-chart-svg"
      >
        {yLabels.map(
          (label, index) => {
            const y =
              paddingTop +
              (chartHeight /
                Math.max(
                  yLabels.length - 1,
                  1,
                )) *
                index

            return (
              <g
                key={`${label}-${index}`}
              >
                <line
                  x1={paddingLeft}
                  x2={
                    width -
                    paddingRight
                  }
                  y1={y}
                  y2={y}
                  className="trend-grid-line"
                />

                <text
                  x={
                    paddingLeft - 8
                  }
                  y={y + 3}
                  textAnchor="end"
                  className="trend-axis-label"
                >
                  {formatarMilhares(
                    label,
                  )}
                </text>
              </g>
            )
          },
        )}

        {series.map(
          (serie) => {
            const pontos =
              serie.pontos
                .map(
                  (ponto) => ({
                    data:
                      normalizarDataEvolucao(
                        ponto.data,
                      ),

                    carteira:
                      ponto.carteira,
                  }),
                )
                .sort(
                  (a, b) =>
                    a.data.localeCompare(
                      b.data,
                    ),
                )
                .map(
                  (ponto) => {
                    const valorNormalizado =
                      (ponto.carteira -
                        min) /
                      range

                    const dataIndex =
                      datas.indexOf(
                        ponto.data,
                      )

                    return {
                      x:
                        datas.length > 1
                          ? paddingLeft +
                            dataIndex *
                              step
                          : paddingLeft,

                      y:
                        height -
                        paddingBottom -
                        chartHeight *
                          valorNormalizado,
                    }
                  },
                )

            const linePath =
              pontos
                .map(
                  (
                    point,
                    index,
                  ) =>
                    `${
                      index === 0
                        ? 'M'
                        : 'L'
                    } ${point.x} ${point.y}`,
                )
                .join(' ')

            return (
              <g
                key={
                  serie.investidor
                }
              >
                <path
                  d={linePath}
                  fill="none"
                  stroke={obterCorInvestidor(
                    serie.investidor,
                  )}
                  strokeWidth="1.75"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />

                {pontos.map(
                  (
                    point,
                    index,
                  ) => (
                    <circle
                      key={`${serie.investidor}-${index}`}
                      cx={point.x}
                      cy={point.y}
                      r="2.8"
                      fill={obterCorInvestidor(
                        serie.investidor,
                      )}
                    />
                  ),
                )}
              </g>
            )
          },
        )}
      </svg>

      <div className="trend-axis">
        {datas
          .map(
            (
              data,
              index,
            ) => ({
              data,
              index,
            }),
          )
          .filter(
            ({ index }) =>
              index === 0 ||
              index ===
                datas.length - 1 ||
              index ===
                Math.floor(
                  datas.length / 2,
                ),
          )
          .map(
            ({
              data,
              index,
            }) => (
              <span
                key={data}
                style={{
                  left: `${
                    index === 0
                      ? 0
                      : index ===
                          datas.length -
                            1
                        ? 100
                        : 50
                  }%`,
                }}
              >
                {formatarMesCurto(
                  data,
                )}
              </span>
            ),
          )}
      </div>

      <div className="trend-legend">
        {series.map(
          (linha) => (
            <span
              key={
                linha.investidor
              }
            >
              <i
                style={{
                  backgroundColor:
                    obterCorInvestidor(
                      linha.investidor,
                    ),
                }}
              />

              {linha.investidor}
            </span>
          ),
        )}
      </div>
    </div>
  )
}

function PatrimonioGauge({
  valor,
  meta,
}: {
  valor: number
  meta: number
}) {
  const progresso =
    meta > 0
      ? Math.min(
          Math.max(
            valor / meta,
            0,
          ),
          1,
        )
      : 0

  const comprimentoArco =
    Math.PI * 90

  return (
    <div className="patrimonio-gauge">
      <svg
        viewBox="0 0 220 122"
        role="img"
        aria-label="Progresso do patrimônio total"
      >
        <path
          d="M 20 100 A 90 90 0 0 1 200 100"
          className="gauge-track"
          pathLength={
            comprimentoArco
          }
        />

        <path
          d="M 20 100 A 90 90 0 0 1 200 100"
          className="gauge-progress"
          pathLength={
            comprimentoArco
          }
          style={{
            strokeDasharray: `${
              comprimentoArco *
              progresso
            } ${comprimentoArco}`,
          }}
        />
      </svg>

      <div className="gauge-value">
        {formatarMoeda(valor)}
      </div>

      <div className="gauge-target">
        Meta {formatarMoeda(meta)}
      </div>
    </div>
  )
}

export function DashboardView({
  dashboard,
  carteiras,
  evolucao,
  erro,
}: DashboardViewProps) {
  /*
   * Todos os indicadores financeiros
   * atuais vêm da API.
   *
   * Não usamos mais snapshot ou
   * histórico patrimonial como fallback
   * para patrimônio, caixa ou valor
   * aplicado.
   */

  const patrimonioEstimado =
    dashboard?.patrimonioEstimado ??
    0

  const valorAplicado =
    dashboard?.valorAplicado ??
    0

  const totalDisponivel =
    dashboard?.caixaDisponivel ??
    0

  const totalProventos =
    dashboard?.totalProventos ??
    0

  const resultadoRealizado =
    dashboard?.resultadoRealizado ??
    0

  const premioOpcoes =
    dashboard?.premioLiquidoOpcoes ??
    0

  /*
   * Indicadores sempre referentes ao ano atual.
   * O backend filtra as operações pelo ano corrente.
   */
  const entradas =
    dashboard?.entradasAno ??
    0

  const saidas =
    dashboard?.saidasAno ??
    0

  const rentabilidade =
    dashboard?.rentabilidadeAno ??
    0

  /*
   * DISTRIBUIÇÃO POR INVESTIDOR
   *
   * Usa o patrimônio ATUAL retornado
   * pelo dashboard individual:
   *
   * Valor Aplicado
   * + Previdência
   * + Caixa
   *
   * Não utiliza mais HistoricoPatrimonio.
   */
  const distribuicaoInvestidores =
    useMemo(
      () =>
        carteiras
          .map((item) => ({
            label: item.nome,

            value:
              item.dashboard
                .patrimonioEstimado ??
              0,
          }))
          .filter(
            (item) =>
              item.value !== 0,
          )
          .sort(
            ordenarDecrescente,
          ),
      [carteiras],
    )

   /*
   * DISTRIBUIÇÃO DO PATRIMÔNIO
   *
   * Inclui:
   * - investimentos por tipo
   * - Previdência
   * - Disponível
   *
   * O total da distribuição passa a
   * representar o patrimônio atual.
   */
  const distribuicaoTipos =
    useMemo(
      () => {
        const tipos =
          (
            dashboard
              ?.distribuicaoPorTipo ??
            []
          )
            .map((item) => ({
              label:
                item.nome ===
                'Fundo Imobiliário'
                  ? 'FIIs'
                  : item.nome,

              value:
                item.valor,
            }))
            .filter(
              (item) =>
                item.value > 0,
            )

        const disponivel =
          dashboard?.caixaDisponivel ??
          0

        if (disponivel > 0) {
          tipos.push({
            label: 'Disponível',
            value: disponivel,
          })
        }

        return tipos.sort(
          ordenarDecrescente,
        )
      },
      [dashboard],
    )
    
  /*
   * Histórico patrimonial é utilizado
   * SOMENTE no gráfico de evolução.
   */
  const seriesEvolucao =
    useMemo(
      () =>
        [...evolucao]
          .map((serie) => ({
            ...serie,

            pontos: [
              ...serie.pontos,
            ].sort(
              (a, b) =>
                a.data.localeCompare(
                  b.data,
                ),
            ),
          })),
      [evolucao],
    )

  return (
    <>
      <header className="hero">
        <div className="hero-summary">
          <div className="summary-card summary-card-accent">
            <span>
              Patrimônio
            </span>

            <PatrimonioGauge
              valor={
                patrimonioEstimado
              }
              meta={1000000}
            />
          </div>

          <div className="summary-card">
            <span>
              Valor Aplicado
            </span>

            <strong
              className={
                valorAplicado >= 0
                  ? 'positive'
                  : 'negative'
              }
            >
              {formatarMoeda(
                valorAplicado,
              )}
            </strong>

            <InvestorAppliedBars
              items={carteiras.map(
                (item) => ({
                  nome: item.nome,

                  valor:
                    item.dashboard
                      .valorAplicado,
                }),
              )}
            />
          </div>

          <div className="summary-card">
            <span>
              Caixa Disponível
            </span>

            <strong
              className={
                totalDisponivel >= 0
                  ? 'positive'
                  : 'negative'
              }
            >
              {formatarMoeda(
                totalDisponivel,
              )}
            </strong>

            <InvestorAppliedBars
              items={(
                dashboard
                  ?.saldosDisponiveis ??
                []
              ).map(
                (item) => ({
                  nome:
                    item.investidor,

                  valor:
                    item.valor,
                }),
              )}
            />
          </div>

          <div className="summary-card">
            <span>
              Prêmio Líquido de Opções
            </span>

            <div className="summary-card-tax">
              <small
                style={{
                  color:
                    'var(--negative)',
                }}
              >
                Impostos pagos
              </small>

              <strong>
                {formatarMoeda(
                  dashboard
                    ?.descontosFiscais ??
                    0,
                )}
              </strong>
            </div>

            <strong
              className={
                premioOpcoes >= 0
                  ? 'positive'
                  : 'negative'
              }
            >
              {formatarMoeda(
                premioOpcoes,
              )}
            </strong>

            <InvestorAppliedBars
              items={carteiras.map(
                (item) => ({
                  nome: item.nome,

                  valor:
                    item.dashboard
                      .premioLiquidoOpcoes,
                }),
              )}
            />
          </div>

          <div className="summary-card">
            <span>
              Resultado da Carteira
            </span>

            <strong
              className={
                resultadoRealizado >=
                0
                  ? 'positive'
                  : 'negative'
              }
            >
              {formatarMoeda(
                resultadoRealizado,
              )}
            </strong>

            <InvestorAppliedBars
              items={carteiras.map(
                (item) => ({
                  nome: item.nome,

                  valor:
                    item.dashboard
                      .resultadoRealizado,
                }),
              )}
            />
          </div>

          <div className="summary-card">
            <span>
              Proventos e Dividendos
            </span>

            <strong
              className={
                totalProventos >= 0
                  ? 'positive'
                  : 'negative'
              }
            >
              {formatarMoeda(
                totalProventos,
              )}
            </strong>

            <InvestorAppliedBars
              items={carteiras.map(
                (item) => ({
                  nome: item.nome,

                  valor:
                    item.dashboard
                      .totalProventos,
                }),
              )}
            />
          </div>
        </div>
      </header>

      {erro ? (
        <div className="info-banner">
          {erro}
        </div>
      ) : null}

      <section className="dashboard-grid">
        <article className="panel compact-metrics">
          <SectionTitle
            title="Rentabilidade"
            badge="Carteira"
          />

          <PortfolioReturnBars
            rentabilidade={rentabilidade}
            entradas={entradas}
            saidas={saidas}
            resultado={resultadoRealizado}
          />
        </article>

        <article className="panel compact-metrics distribution-panel">
          <SectionTitle
            title="Distribuição"
            badge="Carteira"
          />

          {distribuicaoTipos.length >
          0 ? (
            <DistributionPieChart
              items={
                distribuicaoTipos
              }
            />
          ) : (
            <div className="empty-state">
              <strong>
                Distribuição
                indisponível
              </strong>
            </div>
          )}
        </article>

        <article className="panel compact-metrics distribution-panel">
          <SectionTitle
            title="Distribuição"
            badge="Investidor"
          />

          {distribuicaoInvestidores
            .length > 0 ? (
            <DistributionPieChart
              items={
                distribuicaoInvestidores
              }
              colorForItem={(
                label,
              ) =>
                obterCorInvestidor(
                  label,
                )
              }
            />
          ) : (
            <div className="empty-state">
              <strong>
                Distribuição
                indisponível
              </strong>
            </div>
          )}
        </article>

        <article className="panel panel-wide">
          <SectionTitle
            title="Evolução da Carteira"
          />

          {seriesEvolucao.length >
          0 ? (
            <MultiTrendChart
              series={
                seriesEvolucao
              }
            />
          ) : (
            <div className="empty-state">
              <strong>
                Evolução indisponível
                no banco
              </strong>
            </div>
          )}
        </article>
      </section>
    </>
  )
}