import { useEffect, useMemo, useState } from 'react'
import {
  obterDashboardConsolidado,
  obterDashboardPorInvestidor,
  obterEvolucaoConsolidada,
} from './api/dashboardApi'
import { listarInvestidores } from './api/investidoresApi'
import { dashboardSnapshot } from './data/dashboardSnapshot'
import type { Dashboard, EvolucaoInvestidor } from './types/dashboard'
import type { Investidor } from './types/investidor'
import './App.css'

function formatarMoeda(valor: number | null | undefined) {
  if (valor == null || Number.isNaN(valor)) {
    return '--'
  }

  return valor.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  })
}

function formatarPercentual(valor: number | null | undefined) {
  if (valor == null || Number.isNaN(valor)) {
    return '--'
  }

  return valor.toLocaleString('pt-BR', {
    style: 'percent',
    maximumFractionDigits: 1,
  })
}

function formatarMilhares(valor: number) {
  if (Math.abs(valor) < 1000) {
    return valor.toLocaleString('pt-BR', {
      maximumFractionDigits: 1,
    })
  }

  return `${(valor / 1000).toLocaleString('pt-BR', {
    maximumFractionDigits: 1,
  })}K`
}

function formatarMesCurto(data: string) {
  const dataNormalizada = data.includes('T')
    ? data
    : `${data}T00:00:00`

  return new Date(dataNormalizada)
    .toLocaleDateString('pt-BR', {
      month: 'short',
      year: '2-digit',
    })
    .replace(/\s*de\s*/i, ' ')
    .trim()
}

function ordenarDecrescente(
  a: { value: number },
  b: { value: number },
) {
  return b.value - a.value
}

function calculateSeriesBounds(values: readonly number[]) {
  if (values.length === 0) {
    return { min: 0, max: 1 }
  }

  const minValue = Math.min(...values)
  const maxValue = Math.max(...values)
  const min = minValue >= 0 ? 0 : minValue
  const max = maxValue
  const range = max - min || 1
  const margin = range * 0.12 || 1

  if (minValue === maxValue) {
    return {
      min: min === 0 ? 0 : min - 1,
      max: max === 0 ? 1 : max + 1,
    }
  }

  return { min: min === 0 ? 0 : min - margin, max: max + margin }
}

function buildLinePath(
  values: readonly number[],
  width: number,
  height: number,
  padding: number,
) {
  if (values.length === 0) {
    return {
      linePath: '',
      areaPath: '',
      points: [] as Array<{ x: number; y: number }>,
    }
  }

  const { min, max } = calculateSeriesBounds(values)
  const chartWidth = width - padding * 2
  const chartHeight = height - padding * 2
  const safeStep = values.length > 1 ? chartWidth / (values.length - 1) : 0
  const range = max - min || 1

  const points = values.map((value, index) => {
    const x = padding + safeStep * index
    const normalized = (value - min) / range
    const y = padding + chartHeight - normalized * chartHeight

    return { x, y }
  })

  const linePath = points
    .map((point, index) =>
      `${index === 0 ? 'M' : 'L'} ${point.x} ${point.y}`,
    )
    .join(' ')

  const areaPath = [
    `M ${points[0].x} ${height - padding}`,
    ...points.map((point) => `L ${point.x} ${point.y}`),
    `L ${points[points.length - 1].x} ${height - padding}`,
    'Z',
  ].join(' ')

  return { linePath, areaPath, points }
}

type SimpleMetric = {
  label: string
  value: number
}

function MetricBarList({
  items,
  formatter,
  tone = 'calmo',
}: {
  items: readonly SimpleMetric[]
  formatter: (value: number) => string
  tone?: 'calmo' | 'vibrante'
}) {
  const max = Math.max(...items.map((item) => item.value), 1)

  return (
    <div className="metric-bar-list">
      {items.map((item) => {
        const percentual = (item.value / max) * 100

        return (
          <div className="metric-bar-item" key={item.label}>
            <div className="metric-bar-head">
              <strong>{item.label}</strong>
              <span>{formatter(item.value)}</span>
            </div>

            <div className={`metric-bar-track ${tone}`}>
              <div
                className="metric-bar-fill"
                style={{ width: `${Math.max(percentual, 2)}%` }}
              />
            </div>
          </div>
        )
      })}
    </div>
  )
}

function InvestorAppliedBars({
  items,
}: {
  items: readonly { nome: string; valor: number }[]
}) {
  const max = Math.max(...items.map((item) => item.valor), 1)
  const itensOrdenados = [...items].sort((a, b) => b.valor - a.valor)

  return (
    <div className="investor-applied-bars">
      {itensOrdenados.map((item) => (
        <div className="investor-applied-item" key={item.nome}>
          <strong>{formatarMilhares(item.valor)}</strong>
          <div className="investor-applied-track">
            <i style={{ height: `${Math.max((item.valor / max) * 100, 3)}%` }} />
          </div>
          <span>{item.nome}</span>
        </div>
      ))}
    </div>
  )
}

function TrendChart({
  points,
  color = 'var(--accent)',
}: {
  points: readonly { data: string; carteira: number }[]
  color?: string
}) {
  const width = 760
  const height = 70
  const paddingLeft = 56
  const paddingTop = 12
  const paddingRight = 18
  const paddingBottom = 18
  const values = points.map((point) => point.carteira)
  const { min, max } = calculateSeriesBounds(values)
  const { linePath, areaPath, points: svgPoints } = buildLinePath(
    values,
    width,
    height,
    paddingTop,
  )

  const xLabels = points
    .filter((_, index) =>
      index === 0 ||
      index === points.length - 1 ||
      index === Math.floor(points.length / 2),
    )
    .map((point, index, arr) => ({
      label: formatarMesCurto(point.data),
      position:
        arr.length === 1
          ? 0
          : index === 0
            ? 0
            : index === arr.length - 1
              ? 100
              : 50,
    }))

  const yLabels = [max, (max + min) / 2, min]

  return (
    <div className="trend-chart">
      <svg viewBox={`0 0 ${width} ${height}`} className="trend-chart-svg">
        <defs>
          <linearGradient id="trendFill" x1="0" x2="0" y1="0" y2="1">
            <stop offset="0%" stopColor={color} stopOpacity="0.25" />
            <stop offset="100%" stopColor={color} stopOpacity="0.02" />
          </linearGradient>
        </defs>

        {yLabels.map((label, index) => {
          const y =
            paddingTop +
            ((height - paddingTop - paddingBottom) / (yLabels.length - 1)) * index

          return (
            <g key={label}>
              <line
                x1={paddingLeft}
                x2={width - paddingRight}
                y1={y}
                y2={y}
                className="trend-grid-line"
              />
              <text x={paddingLeft - 10} y={y + 4} className="trend-axis-label">
                {formatarMoeda(label)}
              </text>
            </g>
          )
        })}

        <path d={areaPath} fill="url(#trendFill)" />
        <path
          d={linePath}
          fill="none"
          stroke={color}
          strokeWidth="3"
          strokeLinecap="round"
          strokeLinejoin="round"
        />

        {svgPoints.map((point, index) => (
          <circle
            key={`${point.x}-${point.y}-${index}`}
            cx={point.x}
            cy={point.y}
            r="3.8"
            fill={color}
          />
        ))}
      </svg>

      <div className="trend-axis">
        {xLabels.map((item) => (
          <span
            key={`${item.label}-${item.position}`}
            style={{ left: `${item.position}%` }}
          >
            {item.label}
          </span>
        ))}
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
    new Set(series.flatMap((item) => item.pontos.map((ponto) => ponto.data))),
  ).sort()
  const valores = series.flatMap((item) => item.pontos.map((ponto) => ponto.carteira))
  const { min, max } = calculateSeriesBounds(valores)
  const range = max - min || 1
  const chartWidth = width - paddingLeft - paddingRight
  const chartHeight = height - paddingTop - paddingBottom
  const step = datas.length > 1 ? chartWidth / (datas.length - 1) : 0
  const cores = ['#63e6c8', '#ffbf69', '#8da7ff', '#f783ac', '#b197fc', '#74c0fc']

  return (
    <div className="trend-chart">
      <svg viewBox={`0 0 ${width} ${height}`} className="trend-chart-svg">
        {[
          { label: max, y: paddingTop },
          { label: (max + min) / 2, y: height / 2 },
          { label: min, y: height - paddingBottom },
        ].map((item) => (
          <g key={item.y}>
            <line x1={paddingLeft} x2={width - paddingRight} y1={item.y} y2={item.y} className="trend-grid-line" />
            <text
              x={paddingLeft - 8}
              y={item.y + 3}
              textAnchor="end"
              className="trend-axis-label"
            >
              {formatarMilhares(item.label)}
            </text>
          </g>
        ))}

        {series.map((serie, serieIndex) => {
          const pontos = datas.map((data, index) => {
            const valor = serie.pontos.find((item) => item.data === data)?.carteira ?? 0
            const valorNormalizado = (valor - min) / range

            return {
              x: paddingLeft + index * step,
              y: height - paddingBottom - chartHeight * valorNormalizado,
            }
          })
          const linePath = pontos
            .map((point, index) => `${index === 0 ? 'M' : 'L'} ${point.x} ${point.y}`)
            .join(' ')

          return (
            <g key={serie.investidor}>
              <path
                d={linePath}
                fill="none"
                stroke={cores[serieIndex % cores.length]}
                strokeWidth="2.5"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
              {pontos.map((point, index) => (
                <circle
                  key={`${serie.investidor}-${index}`}
                  cx={point.x}
                  cy={point.y}
                  r="2.8"
                  fill={cores[serieIndex % cores.length]}
                />
              ))}
            </g>
          )
        })}
      </svg>

      <div className="trend-axis">
        {datas
          .map((data, index) => ({ data, index }))
          .filter(({ index }) => index === 0 || index === datas.length - 1 || index === Math.floor(datas.length / 2))
          .map(({ data, index }) => (
            <span
              key={data}
              style={{ left: `${index === 0 ? 0 : index === datas.length - 1 ? 100 : 50}%` }}
            >
              {formatarMesCurto(data)}
            </span>
          ))}
      </div>

      <div className="trend-legend">
        {series.map((linha, index) => (
          <span key={linha.investidor}><i style={{ backgroundColor: cores[index % cores.length] }} />{linha.investidor}</span>
        ))}
      </div>
    </div>
  )
}

function SectionTitle({
  title,
  subtitle,
  badge,
}: {
  title: string
  subtitle?: string
  badge?: string
}) {
  return (
    <div className="section-title">
      <div>
        <h2>{title}</h2>
        {subtitle ? <p>{subtitle}</p> : null}
      </div>
      {badge ? <span className="badge">{badge}</span> : null}
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
  const progresso = Math.min(Math.max(valor / meta, 0), 1)
  const comprimentoArco = Math.PI * 90

  return (
    <div className="patrimonio-gauge">
      <svg viewBox="0 0 220 122" role="img" aria-label="Progresso do patrimônio total">
        <path
          d="M 20 100 A 90 90 0 0 1 200 100"
          className="gauge-track"
          pathLength={comprimentoArco}
        />
        <path
          d="M 20 100 A 90 90 0 0 1 200 100"
          className="gauge-progress"
          pathLength={comprimentoArco}
          style={{ strokeDasharray: `${comprimentoArco * progresso} ${comprimentoArco}` }}
        />
      </svg>
      <div className="gauge-value">{formatarMoeda(valor)}</div>
      <div className="gauge-target">Meta {formatarMoeda(meta)}</div>
    </div>
  )
}

function App() {
  const [investidores, setInvestidores] = useState<Investidor[]>([])
  const [dashboard, setDashboard] = useState<Dashboard | null>(null)
  const [carteirasPorInvestidor, setCarteirasPorInvestidor] = useState<
    Array<{ nome: string; dashboard: Dashboard }>
  >([])
  const [evolucao, setEvolucao] = useState<EvolucaoInvestidor[]>([])
  const [carregandoInvestidores, setCarregandoInvestidores] =
    useState(true)
  const [apiDisponivel, setApiDisponivel] = useState(true)
  const [erro, setErro] = useState<string | null>(null)

  useEffect(() => {
    async function carregarInvestidores() {
      try {
        setCarregandoInvestidores(true)

        const dados = await listarInvestidores()

        if (dados.length > 0) {
          setInvestidores(dados)
          setApiDisponivel(true)
          setErro(null)
          return
        }

        throw new Error('Lista vazia')
      } catch (error) {
        console.error(error)

        setApiDisponivel(false)
        setErro(null)
        setInvestidores(
          dashboardSnapshot.investidores.map((nome) => ({
            id: nome,
            nome,
          })),
        )
      } finally {
        setCarregandoInvestidores(false)
      }
    }

    carregarInvestidores()
  }, [])

  useEffect(() => {
    if (!apiDisponivel) {
      return
    }

    const controller = new AbortController()

    async function carregarDashboard() {
      try {
        setErro(null)

        const [dados, dadosEvolucao] = await Promise.all([
          obterDashboardConsolidado(),
          obterEvolucaoConsolidada(),
        ])

        if (!controller.signal.aborted) {
          setDashboard(dados)
          setEvolucao(dadosEvolucao)
        }
      } catch (error) {
        console.error(error)

        if (!controller.signal.aborted) {
          setApiDisponivel(false)
          setDashboard(null)
          setEvolucao([])
          setErro(null)
        }
      }
    }

    carregarDashboard()

    return () => controller.abort()
  }, [apiDisponivel])

  useEffect(() => {
    if (!apiDisponivel || investidores.length === 0) {
      return
    }

    async function carregarCarteirasPorInvestidor() {
      try {
        const resultados = await Promise.all(
          investidores.map(async (investidor) => ({
            nome: investidor.nome,
            dashboard: await obterDashboardPorInvestidor(investidor.id),
          })),
        )

        setCarteirasPorInvestidor(resultados)
      } catch (error) {
        console.error(error)
        setCarteirasPorInvestidor([])
      }
    }

    carregarCarteirasPorInvestidor()
  }, [apiDisponivel, investidores])

  const patrimonioConsolidado = useMemo(() => {
    const series = Object.values(dashboardSnapshot.timelinePorPessoa)
    const ultimaData = series
      .flatMap((serie) => serie.map((ponto) => ponto.data))
      .sort()
      .at(-1)

    return series.reduce(
      (total, serie) =>
        total + (serie.find((ponto) => ponto.data === ultimaData)?.carteira ?? 0),
      0,
    )
  }, [])

  const evolucaoLocal = useMemo(
    () =>
      Object.entries(dashboardSnapshot.timelinePorPessoa).map(
        ([investidor, pontos]) => ({
          investidor,
          pontos,
        }),
      ),
    [],
  )

  const seriesEvolucao = evolucao.length > 0 ? evolucao : evolucaoLocal

  const valorAplicado = useMemo(
    () =>
      dashboardSnapshot.analise.reduce(
        (total, item) => total + item.investido,
        0,
      ),
    [],
  )

  const totalDisponivelSnapshot = useMemo(
    () =>
      dashboardSnapshot.valoresDisponiveis.reduce(
        (total, item) => total + item.value,
        0,
      ),
    [],
  )

  const totalDisponivel = dashboard?.caixaDisponivel ?? totalDisponivelSnapshot
  const totalProventos = dashboard?.totalProventos ?? dashboardSnapshot.proventos.total
  const resultadoRealizado = dashboard?.resultadoRealizado ?? 0
  const premioOpcoes = dashboard?.premioLiquidoOpcoes ?? 0
  const patrimonioEstimado = dashboard?.patrimonioEstimado ?? patrimonioConsolidado
  const valorAplicadoBackend = dashboard?.valorAplicado ?? valorAplicado

  const distribuicaoTipos = useMemo(() => {
    const mapa = new Map<string, number>()

    dashboardSnapshot.quotes.forEach((quote) => {
      mapa.set(
        quote.tipo,
        (mapa.get(quote.tipo) ?? 0) + quote.precoAtual,
      )
    })

    return Array.from(mapa.entries())
      .map(([label, value]) => ({ label, value }))
      .sort(ordenarDecrescente)
  }, [])

  const modoSnapshot =
    !apiDisponivel || investidores.length === 0 || dashboard === null

  if (carregandoInvestidores) {
    return (
      <main className="estado-pagina">
        <div className="loader" />
        <p>Carregando investidores...</p>
      </main>
    )
  }

  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="marca">
          <div className="marca-icone">IX</div>

          <div>
            <strong>Investimentos</strong>
            <span>Dashboard de carteira</span>
          </div>
        </div>

        <nav className="menu">
          <button className="menu-item ativo" type="button">
            <span className="menu-icone">▣</span>
            Painel
          </button>

          <button className="menu-item" type="button">
            <span className="menu-icone">◫</span>
            Carteira
          </button>

          <button className="menu-item" type="button">
            <span className="menu-icone">↕</span>
            Operações
          </button>

          <button className="menu-item" type="button">
            <span className="menu-icone">◌</span>
            Opções
          </button>

          <button className="menu-item" type="button">
            <span className="menu-icone">$</span>
            Proventos
          </button>
        </nav>

        <div className="sidebar-rodape">
          <span>{dashboardSnapshot.referencia}</span>
          <small>{modoSnapshot ? 'Snapshot local' : 'API conectada'}</small>
        </div>
      </aside>

      <main className="main">
        <header className="hero">
          <div className="hero-summary">
            <div className="summary-card summary-card-accent">
              <span>Patrimônio Total</span>
              <PatrimonioGauge valor={patrimonioEstimado} meta={1000000} />
            </div>

            <div className="summary-card">
              <span>Valor Aplicado BTG</span>
              <strong className={valorAplicadoBackend >= 0 ? 'positive' : 'negative'}>
                {formatarMoeda(valorAplicadoBackend)}
              </strong>
              <InvestorAppliedBars
                items={carteirasPorInvestidor.map((item) => ({
                  nome: item.nome,
                  valor: item.dashboard.valorAplicado,
                }))}
              />
            </div>

            <div className="summary-card">
              <span>Caixa Disponível BTG</span>
              <strong className={totalDisponivel >= 0 ? 'positive' : 'negative'}>
                {formatarMoeda(totalDisponivel)}
              </strong>
              <InvestorAppliedBars
                items={(dashboard?.saldosDisponiveis ?? []).map((item) => ({
                  nome: item.investidor,
                  valor: item.valor,
                }))}
              />
            </div>

            <div className="summary-card">
              <span>Prêmio Líquido de Opções</span>
              <div className="summary-card-tax">
                <small>Impostos pagos</small>
                <strong>{formatarMoeda(dashboard?.descontosFiscais ?? 0)}</strong>
              </div>
              <strong className={premioOpcoes >= 0 ? 'positive' : 'negative'}>
                {formatarMoeda(premioOpcoes)}
              </strong>
              <InvestorAppliedBars
                items={carteirasPorInvestidor.map((item) => ({
                  nome: item.nome,
                  valor: item.dashboard.premioLiquidoOpcoes,
                }))}
              />
            </div>

            <div className="summary-card">
              <span>Resultado da carteira</span>
              <strong className={resultadoRealizado >= 0 ? 'positive' : 'negative'}>
                {formatarMoeda(resultadoRealizado)}
              </strong>
              <InvestorAppliedBars
                items={carteirasPorInvestidor.map((item) => ({
                  nome: item.nome,
                  valor: item.dashboard.resultadoRealizado,
                }))}
              />
            </div>

            <div className="summary-card">
              <span>Proventos + Dividendos</span>
              <strong className={totalProventos >= 0 ? 'positive' : 'negative'}>
                {formatarMoeda(totalProventos)}
              </strong>
              <InvestorAppliedBars
                items={carteirasPorInvestidor.map((item) => ({
                  nome: item.nome,
                  valor: item.dashboard.totalProventos,
                }))}
              />
            </div>
          </div>
        </header>

        {erro ? <div className="info-banner">{erro}</div> : null}

        <section className="dashboard-grid">
          <article className="panel panel-wide">
            <SectionTitle title="Evolução da carteira" />

            {seriesEvolucao.length > 0 ? (
              <MultiTrendChart series={seriesEvolucao} />
            ) : (
              <>
                <TrendChart points={[]} />
                <div className="empty-state"><strong>Evolução indisponível no banco</strong></div>
              </>
            )}
          </article>

          <article className="panel compact-metrics">
            <SectionTitle
              title="Rentabilidade"
              subtitle="Comparação com benchmarks da planilha"
              badge="Carteira"
            />

            <MetricBarList
              items={dashboardSnapshot.rentabilidade}
              formatter={formatarPercentual}
              tone="vibrante"
            />
          </article>

          <article className="panel compact-metrics">
            <SectionTitle
              title="Distribuição"
              subtitle="Tipos de ativo presentes na aba Cotações Ativos"
              badge="Tipos"
            />

            <MetricBarList
              items={distribuicaoTipos}
              formatter={formatarMoeda}
            />
          </article>
        </section>

      </main>
    </div>
  )
}

export default App
