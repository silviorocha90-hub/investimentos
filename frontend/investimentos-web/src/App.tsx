import { useEffect, useMemo, useState } from 'react'
import {
  obterDashboardConsolidado,
  obterDashboardPorInvestidor,
  obterEvolucaoConsolidada,
  obterOperacoes,
  atualizarOperacao,
  criarOperacao,
} from './api/dashboardApi'
import { listarInvestidores } from './api/investidoresApi'
import { dashboardSnapshot } from './data/dashboardSnapshot'
import type { Dashboard, EvolucaoInvestidor, OperacaoCarteira } from './types/dashboard'
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

function formatarDataCurta(data: string) {
  return new Date(`${data.slice(0, 10)}T00:00:00`).toLocaleDateString(
    'pt-BR',
    { day: '2-digit', month: '2-digit', year: 'numeric' },
  )
}

function formatarNumeroInteiro(valor: number) {
  return valor.toLocaleString('pt-BR', { maximumFractionDigits: 2 })
}

function normalizarDataEvolucao(data: string) {
  return data.slice(0, 10)
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

function formatarMilharesInteiros(valor: number) {
  if (Math.abs(valor) < 1000) {
    return valor.toLocaleString('pt-BR', { maximumFractionDigits: 0 })
  }

  return `${Math.round(valor / 1000).toLocaleString('pt-BR')}K`
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
          <strong>{formatarMilharesInteiros(item.valor)}</strong>
          <div className="investor-applied-track">
            <i style={{ height: `${Math.max((item.valor / max) * 100, 3)}%` }} />
          </div>
          <span>{item.nome}</span>
        </div>
      ))}
    </div>
  )
}

function DistributionPieChart({
  items,
}: {
  items: readonly SimpleMetric[]
}) {
  const cores = ['#63e6c8', '#ffbf69', '#8da7ff', '#f783ac', '#b197fc', '#74c0fc']
  const total = items.reduce((sum, item) => sum + Math.max(item.value, 0), 0)
  let acumulado = 0

  const fatias = items.map((item, index) => {
    const valor = Math.max(item.value, 0)
    const inicio = total > 0 ? acumulado / total : 0
    acumulado += valor
    const fim = total > 0 ? acumulado / total : 0
    const inicioAngulo = inicio * Math.PI * 2 - Math.PI / 2
    const fimAngulo = fim * Math.PI * 2 - Math.PI / 2
    const raio = 78
    const x1 = 100 + raio * Math.cos(inicioAngulo)
    const y1 = 100 + raio * Math.sin(inicioAngulo)
    const x2 = 100 + raio * Math.cos(fimAngulo)
    const y2 = 100 + raio * Math.sin(fimAngulo)
    const grandeArco = fim - inicio > 0.5 ? 1 : 0
    const caminho = total === 0
      ? ''
      : `M 100 100 L ${x1} ${y1} A ${raio} ${raio} 0 ${grandeArco} 1 ${x2} ${y2} Z`

    return {
      ...item,
      percentual: total > 0 ? (valor / total) * 100 : 0,
      caminho,
      cor: cores[index % cores.length],
    }
  })

  return (
    <div className="distribution-chart">
      <div className="distribution-pie-wrap">
        <svg viewBox="0 0 200 200" className="distribution-pie" role="img" aria-label="Distribuição do patrimônio por investidor">
          {fatias.map((fatia) => (
            <path key={fatia.label} d={fatia.caminho} fill={fatia.cor} stroke="var(--panel)" strokeWidth="1.5" />
          ))}
          <circle cx="100" cy="100" r="42" fill="var(--panel-strong)" />
          <text x="100" y="96" textAnchor="middle" className="distribution-pie-total-label">Total</text>
          <text x="100" y="112" textAnchor="middle" className="distribution-pie-total">{formatarMoeda(total)}</text>
        </svg>
      </div>

      <div className="distribution-legend">
        {fatias.map((fatia) => (
          <div className="distribution-legend-row" key={fatia.label}>
            <span className="distribution-legend-name">
              <i style={{ backgroundColor: fatia.cor }} />
              {fatia.label}
            </span>
            <span className="distribution-legend-value">
              {fatia.percentual.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%
              <small>{formatarMoeda(fatia.value)}</small>
            </span>
          </div>
        ))}
      </div>
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
    new Set(
      series.flatMap((item) =>
        item.pontos.map((ponto) => normalizarDataEvolucao(ponto.data)),
      ),
    ),
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
          const pontos = serie.pontos
            .map((ponto) => ({
              data: normalizarDataEvolucao(ponto.data),
              carteira: ponto.carteira,
            }))
            .sort((a, b) => a.data.localeCompare(b.data))
            .map((ponto) => {
            const valor = ponto.carteira
            const valorNormalizado = (valor - min) / range

            return {
              x:
                datas.length > 1
                  ? paddingLeft + datas.indexOf(ponto.data) * step
                    : paddingLeft,
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

function OperacoesView({
  investidores,
  selectedInvestor,
  onSelectInvestor,
  operacoes = [],
  onSaveOperation,
}: {
  investidores: readonly Investidor[]
  selectedInvestor: string
  onSelectInvestor: (nome: string) => void
  operacoes?: readonly OperacaoCarteira[]
  onSaveOperation: (operacao: Pick<OperacaoCarteira, 'id' | 'data' | 'quantidade' | 'precoUnitario' | 'taxas'>) => Promise<void>
}) {
  const [operacaoEmEdicao, setOperacaoEmEdicao] = useState<string | null>(null)
  const [salvandoOperacao, setSalvandoOperacao] = useState(false)
  const [deletandoOperacao, setDeletandoOperacao] = useState(false)

  // Estados para formulário de nova operação
  const [formNovaOperacao, setFormNovaOperacao] = useState({
    ticker: '',
    tipoOperacaoCodigo: 'COMPRA',
    quantidade: '',
    precoUnitario: '',
    taxas: '0',
    data: new Date().toISOString().split('T')[0],
  })
  const [criandoOperacao, setCriandoOperacao] = useState(false)
  const [erroOperacao, setErroOperacao] = useState<string | null>(null)

  const handleDeleteOperation = async (operacaoId: string) => {
    if (!confirm('Tem certeza que deseja deletar esta operação? Esta ação não pode ser desfeita.')) {
      return
    }
    try {
      setDeletandoOperacao(true)
      const response = await fetch(`${import.meta.env.VITE_API_URL ?? 'http://localhost:5001'}/api/operacoes/${operacaoId}`, {
        method: 'DELETE',
      })
      if (!response.ok) {
        throw new Error(`Erro ao deletar operação: ${response.status}`)
      }
      // Recarregar operações após delete
      window.location.reload()
    } catch (error) {
      console.error('Erro ao deletar operação:', error)
      alert('Erro ao deletar operação!')
    } finally {
      setDeletandoOperacao(false)
    }
  }

  const handleAdicionarOperacao = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    setErroOperacao(null)

    // Validação básica
    if (!formNovaOperacao.ticker.trim()) {
      setErroOperacao('Ticker é obrigatório')
      return
    }
    if (!formNovaOperacao.quantidade || Number(formNovaOperacao.quantidade) <= 0) {
      setErroOperacao('Quantidade deve ser maior que 0')
      return
    }
    if (!formNovaOperacao.precoUnitario || Number(formNovaOperacao.precoUnitario) <= 0) {
      setErroOperacao('Preço unitário deve ser maior que 0')
      return
    }

    try {
      setCriandoOperacao(true)
      const investidor = investidores.find((item) => item.nome === selectedInvestor)
      if (!investidor) {
        setErroOperacao('Investidor não selecionado')
        return
      }

      await criarOperacao(investidor.id, {
        data: formNovaOperacao.data,
        ticker: formNovaOperacao.ticker.toUpperCase(),
        tipoOperacaoCodigo: formNovaOperacao.tipoOperacaoCodigo,
        quantidade: Number(formNovaOperacao.quantidade),
        precoUnitario: Number(formNovaOperacao.precoUnitario),
        taxas: Number(formNovaOperacao.taxas) || 0,
      })

      // Recarregar operações após criar
      window.location.reload()
    } catch (error) {
      console.error('Erro ao criar operação:', error)
      setErroOperacao(error instanceof Error ? error.message : 'Erro ao criar operação')
    } finally {
      setCriandoOperacao(false)
    }
  }

  return (
    <section className="portfolio-view operacoes-view">
      <div className="portfolio-toolbar">
        <div>
          <h1>Operações</h1>
        </div>

        <label className="investor-select-label">
          <span>Investidor</span>
          <select value={selectedInvestor} onChange={(event) => onSelectInvestor(event.target.value)}>
            {investidores.map((investidor) => (
              <option key={investidor.id} value={investidor.nome}>{investidor.nome}</option>
            ))}
          </select>
        </label>
      </div>

      <article className="panel portfolio-add-operation">
        <SectionTitle title="Adicionar Nova Operação" />
        <form onSubmit={handleAdicionarOperacao} style={{ padding: '1rem', borderTop: '1px solid var(--border)', background: 'rgba(99, 230, 200, 0.04)' }}>
          {erroOperacao && (
            <div style={{ color: '#ff6b6b', marginBottom: '1rem', fontSize: '0.9rem' }}>
              ⚠️ {erroOperacao}
            </div>
          )}

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '1rem', marginBottom: '1rem' }}>
            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.85rem', color: 'var(--text-muted)' }}>Ticker *</label>
              <input
                type="text"
                placeholder="Ex: PETR3"
                value={formNovaOperacao.ticker}
                onChange={(e) => setFormNovaOperacao({ ...formNovaOperacao, ticker: e.target.value })}
                style={{ width: '100%', padding: '0.5rem', border: '1px solid var(--border)', borderRadius: '4px' }}
                disabled={criandoOperacao}
              />
            </div>

            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.85rem', color: 'var(--text-muted)' }}>Tipo *</label>
              <select
                value={formNovaOperacao.tipoOperacaoCodigo}
                onChange={(e) => setFormNovaOperacao({ ...formNovaOperacao, tipoOperacaoCodigo: e.target.value })}
                style={{ width: '100%', padding: '0.5rem', border: '1px solid var(--border)', borderRadius: '4px' }}
                disabled={criandoOperacao}
              >
                <option value="COMPRA">COMPRA</option>
                <option value="VENDA">VENDA</option>
              </select>
            </div>

            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.85rem', color: 'var(--text-muted)' }}>Quantidade *</label>
              <input
                type="number"
                placeholder="0.0000"
                min="0.0001"
                step="any"
                value={formNovaOperacao.quantidade}
                onChange={(e) => setFormNovaOperacao({ ...formNovaOperacao, quantidade: e.target.value })}
                style={{ width: '100%', padding: '0.5rem', border: '1px solid var(--border)', borderRadius: '4px' }}
                disabled={criandoOperacao}
              />
            </div>

            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.85rem', color: 'var(--text-muted)' }}>Preço Unitário *</label>
              <input
                type="number"
                placeholder="0.00"
                min="0"
                step="0.01"
                value={formNovaOperacao.precoUnitario}
                onChange={(e) => setFormNovaOperacao({ ...formNovaOperacao, precoUnitario: e.target.value })}
                style={{ width: '100%', padding: '0.5rem', border: '1px solid var(--border)', borderRadius: '4px' }}
                disabled={criandoOperacao}
              />
            </div>

            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.85rem', color: 'var(--text-muted)' }}>Taxas</label>
              <input
                type="number"
                placeholder="0.00"
                min="0"
                step="0.01"
                value={formNovaOperacao.taxas}
                onChange={(e) => setFormNovaOperacao({ ...formNovaOperacao, taxas: e.target.value })}
                style={{ width: '100%', padding: '0.5rem', border: '1px solid var(--border)', borderRadius: '4px' }}
                disabled={criandoOperacao}
              />
            </div>

            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.85rem', color: 'var(--text-muted)' }}>Data *</label>
              <input
                type="date"
                value={formNovaOperacao.data}
                onChange={(e) => setFormNovaOperacao({ ...formNovaOperacao, data: e.target.value })}
                style={{ width: '100%', padding: '0.5rem', border: '1px solid var(--border)', borderRadius: '4px' }}
                disabled={criandoOperacao}
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={criandoOperacao}
            style={{
              padding: '0.6rem 1.5rem',
              background: criandoOperacao ? '#ccc' : 'var(--primary)',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: criandoOperacao ? 'not-allowed' : 'pointer',
              fontSize: '0.9rem',
              fontWeight: '500',
            }}
          >
            {criandoOperacao ? 'Adicionando...' : '✓ Adicionar Operação'}
          </button>
        </form>
      </article>

      <article className="panel portfolio-operations" style={{ display: 'flex', flexDirection: 'column', height: 'auto' }}>
        <SectionTitle
          title={`Operações de ${selectedInvestor}`}
          badge={`${operacoes.length} operações`}
        />

        {operacoes.length > 0 ? (
          <div className="table-wrap compact" style={{ flex: 1, overflowY: 'auto', height: '600px', marginTop: '1rem' }}>
            <table className="data-table positions-table">
              <thead>
                <tr>
                  <th>Data</th>
                  <th>Ativo</th>
                  <th>Tipo</th>
                  <th className="align-right">Quantidade</th>
                  <th className="align-right">Preço</th>
                  <th className="align-right">Taxas</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {operacoes.map((operacao) => {
                  const editando = operacaoEmEdicao === operacao.id

                  return (
                    <tr key={operacao.id}>
                      {editando ? (
                        <>
                          <td><input name="data" type="date" defaultValue={operacao.data.slice(0, 10)} /></td>
                          <td><span className="ticker">{operacao.ticker}</span></td>
                          <td>{operacao.tipoOperacao}</td>
                          <td className="align-right"><input name="quantidade" type="number" min="0.0001" step="any" defaultValue={operacao.quantidade} /></td>
                          <td className="align-right"><input name="precoUnitario" type="number" min="0" step="0.01" defaultValue={operacao.precoUnitario} /></td>
                          <td className="align-right"><input name="taxas" type="number" min="0" step="0.01" defaultValue={operacao.taxas} /></td>
                          <td className="operation-actions">
                            <button className="table-action primary" type="button" disabled={salvandoOperacao} onClick={async (event) => {
                              const row = (event.currentTarget as HTMLElement).closest('tr')
                              const formData = new FormData()
                              row?.querySelectorAll<HTMLInputElement>('input').forEach((input) => formData.set(input.name, input.value))
                              setSalvandoOperacao(true)
                              try {
                                await onSaveOperation({
                                  id: operacao.id,
                                  data: String(formData.get('data')),
                                  quantidade: Number(formData.get('quantidade')),
                                  precoUnitario: Number(formData.get('precoUnitario')),
                                  taxas: Number(formData.get('taxas')),
                                })
                                setOperacaoEmEdicao(null)
                              } finally {
                                setSalvandoOperacao(false)
                              }
                            }}>Salvar</button>
                            <button className="table-action" type="button" onClick={() => setOperacaoEmEdicao(null)}>Cancelar</button>
                          </td>
                        </>
                      ) : (
                        <>
                          <td>{formatarDataCurta(operacao.data)}</td>
                          <td><span className="ticker">{operacao.ticker}</span></td>
                          <td>{operacao.tipoOperacao}</td>
                          <td className="align-right">{formatarNumeroInteiro(operacao.quantidade)}</td>
                          <td className="align-right">{formatarMoeda(operacao.precoUnitario)}</td>
                          <td className="align-right">{formatarMoeda(operacao.taxas)}</td>
                          <td className="operation-actions" style={{ whiteSpace: 'nowrap' }}>
                            <button className="table-action" type="button" onClick={() => setOperacaoEmEdicao(operacao.id)}>Editar</button>
                            <button className="table-action" type="button" style={{ color: '#ff6b6b' }} disabled={deletandoOperacao} onClick={() => handleDeleteOperation(operacao.id)}>Deletar</button>
                          </td>
                        </>
                      )}
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="empty-state"><strong>Nenhuma operação disponível</strong></div>
        )}
      </article>
    </section>
  )
}

function CarteiraView({
  investidores,
  carteiras,
  selectedInvestor,
  onSelectInvestor,
  snapshotSeries,
  saldosDisponiveis = [],
}: {
  investidores: readonly Investidor[]
  carteiras: ReadonlyArray<{ nome: string; dashboard: Dashboard }>
  selectedInvestor: string
  onSelectInvestor: (nome: string) => void
  snapshotSeries: Record<string, { data: string; carteira: number }[]>
  saldosDisponiveis?: ReadonlyArray<{ investidor: string; valor: number }>
}) {
  const carteira = carteiras.find((item) => item.nome === selectedInvestor)?.dashboard
  const pontos = snapshotSeries[selectedInvestor] ?? []
  const patrimonioBaseSnapshot = pontos
    .slice()
    .sort((a, b) => a.data.localeCompare(b.data))
    .at(-1)?.carteira ?? 0
  const patrimonioApi = carteira?.patrimonioEstimado ?? 0
  const patrimonioBase = patrimonioApi > 0 ? patrimonioApi : patrimonioBaseSnapshot
  const valorAplicado = carteira?.valorAplicado ?? 0
  const saldoConsolidado = saldosDisponiveis.find(
    (item) => item.investidor === selectedInvestor,
  )?.valor ?? 0
  const valorDisponivel = carteira
    ? carteira.posicoes.length > 0
      ? (carteira.caixaDisponivel ?? 0) || saldoConsolidado
      : patrimonioBaseSnapshot
    : saldoConsolidado || patrimonioBaseSnapshot
  const patrimonio = patrimonioBase
  const resultado = carteira?.resultadoRealizado ?? 0
  const totalProventos = carteira?.totalProventos ?? 0
  const posicoes = carteira?.posicoes ?? []
  const posicoesOrdenadas = posicoes
    .slice()
    .sort((a, b) => b.quantidade - a.quantidade)

  return (
    <section className="portfolio-view carteira-page">
      <div className="portfolio-toolbar carteira-toolbar">
        <div>
          <h1>Carteira por Investidor</h1>
        </div>

        <label className="investor-select-label">
          <span>Investidor</span>
          <select value={selectedInvestor} onChange={(event) => onSelectInvestor(event.target.value)}>
            {investidores.map((investidor) => (
              <option key={investidor.id} value={investidor.nome}>{investidor.nome}</option>
            ))}
          </select>
        </label>
      </div>

      <div className="portfolio-metrics">
        <article className="portfolio-metric accent">
          <span>Patrimônio Atual</span>
          <strong>{formatarMoeda(patrimonio)}</strong>
        </article>
        <article className="portfolio-metric">
          <span>Valor Aplicado</span>
          <strong>{formatarMoeda(valorAplicado)}</strong>
        </article>
        <article className="portfolio-metric">
          <span>Disponível</span>
          <strong>{formatarMoeda(valorDisponivel)}</strong>
        </article>
      </div>

      <article className="panel portfolio-positions">
        <SectionTitle
          title={`Posições de ${selectedInvestor}`}
          subtitle="Ativos líquidos da carteira selecionada"
          badge={`${posicoesOrdenadas.length} ativos`}
        />

        {posicoesOrdenadas.length > 0 ? (
          <div className="table-wrap compact">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Ativo</th>
                  <th>Quantidade</th>
                  <th>Preço médio</th>
                  <th>Custo total</th>
                </tr>
              </thead>
              <tbody>
                {posicoesOrdenadas.map((posicao) => (
                  <tr key={posicao.ticker}>
                    <td>
                      <span className="ticker">{posicao.ticker}</span>
                      {posicao.nome.trim().toUpperCase() !== posicao.ticker.trim().toUpperCase() ? (
                        <span className="subtle-inline"> {posicao.nome}</span>
                      ) : null}
                    </td>
                    <td className="align-right">{formatarNumeroInteiro(posicao.quantidade)}</td>
                    <td className="align-right">{formatarMoeda(posicao.precoMedio)}</td>
                    <td className="align-right strong">{formatarMoeda(posicao.custoTotal)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="empty-state"><strong>Posições detalhadas indisponíveis no snapshot local</strong></div>
        )}
      </article>

      <div className="portfolio-secondary-metrics">
        <article className="portfolio-metric">
          <span>Resultado</span>
          <strong className={resultado >= 0 ? 'positive' : 'negative'}>{formatarMoeda(resultado)}</strong>
        </article>
        <article className="portfolio-metric">
          <span>Proventos</span>
          <strong>{formatarMoeda(totalProventos)}</strong>
        </article>
      </div>
    </section>
  )
}

function App() {
  const [telaAtual, setTelaAtual] = useState<'painel' | 'carteira' | 'operacoes'>('painel')
  const [investidores, setInvestidores] = useState<Investidor[]>([])
  const [dashboard, setDashboard] = useState<Dashboard | null>(null)
  const [carteirasPorInvestidor, setCarteirasPorInvestidor] = useState<
    Array<{ nome: string; dashboard: Dashboard }>
  >([])
  const [operacoes, setOperacoes] = useState<OperacaoCarteira[]>([])
  const [evolucao, setEvolucao] = useState<EvolucaoInvestidor[]>([])
  const [carregandoInvestidores, setCarregandoInvestidores] =
    useState(true)
  const [apiDisponivel, setApiDisponivel] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [investidorSelecionado, setInvestidorSelecionado] = useState('')

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
    if (investidores.length > 0 && !investidores.some((item) => item.nome === investidorSelecionado)) {
      setInvestidorSelecionado(investidores[0].nome)
    }
  }, [investidores, investidorSelecionado])

  useEffect(() => {
    if (!apiDisponivel || !investidorSelecionado) {
      setOperacoes([])
      return
    }

    const investidor = investidores.find((item) => item.nome === investidorSelecionado)

    if (!investidor) {
      return
    }

    obterOperacoes(investidor.id)
      .then(setOperacoes)
      .catch((error) => {
        console.error(error)
        setOperacoes([])
      })
  }, [apiDisponivel, investidorSelecionado, investidores])

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
      const resultados = await Promise.allSettled(
        investidores.map(async (investidor) => ({
          nome: investidor.nome,
          dashboard: await obterDashboardPorInvestidor(investidor.id),
        })),
      )

      const sucesso = resultados.flatMap((resultado) => {
        if (resultado.status === 'fulfilled') {
          return [resultado.value]
        }

        console.error(resultado.reason)
        return []
      })

      setCarteirasPorInvestidor(sucesso)
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

  const distribuicaoInvestidores = useMemo(
    () =>
      seriesEvolucao
        .map((serie) => ({
          label: serie.investidor,
          value: serie.pontos
            .slice()
            .sort((a, b) => a.data.localeCompare(b.data))
            .at(-1)?.carteira ?? 0,
        }))
        .sort(ordenarDecrescente),
    [seriesEvolucao],
  )

  const modoSnapshot =
    !apiDisponivel || investidores.length === 0 || dashboard === null

  async function salvarOperacao(
    operacao: Pick<OperacaoCarteira, 'id' | 'data' | 'quantidade' | 'precoUnitario' | 'taxas'>,
  ) {
    await atualizarOperacao(operacao.id, operacao)

    const investidor = investidores.find((item) => item.nome === investidorSelecionado)

    if (!investidor) {
      return
    }

    const [dashboardAtualizado, operacoesAtualizadas] = await Promise.all([
      obterDashboardPorInvestidor(investidor.id),
      obterOperacoes(investidor.id),
    ])

    setCarteirasPorInvestidor((atuais) =>
      atuais.map((item) =>
        item.nome === investidorSelecionado
          ? { ...item, dashboard: dashboardAtualizado }
          : item,
      ),
    )
    setOperacoes(operacoesAtualizadas)
  }

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
          <img
            className="marca-icone"
            src="/tio-patinhas.png"
            alt="Tio Patinhas mergulhando em moedas"
          />

          <div className="marca-info">
            <strong>Silvio Rocha</strong>
            <span>Dashboard de Carteira</span>
          </div>
        </div>

        <nav className="menu">
          <button className={`menu-item ${telaAtual === 'painel' ? 'ativo' : ''}`} type="button" onClick={() => setTelaAtual('painel')}>
            <span className="menu-icone">▣</span>
            Painel
          </button>

          <button className={`menu-item ${telaAtual === 'carteira' ? 'ativo' : ''}`} type="button" onClick={() => setTelaAtual('carteira')}>
            <span className="menu-icone">◫</span>
            Carteira
          </button>

          <button className={`menu-item ${telaAtual === 'operacoes' ? 'ativo' : ''}`} type="button" onClick={() => setTelaAtual('operacoes')}>
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
        {telaAtual === 'painel' ? (
          <>
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

          <article className="panel compact-metrics distribution-panel">
            <SectionTitle
              title="Distribuição"
              subtitle="Patrimônio por investidor"
              badge={formatarMoeda(
                distribuicaoInvestidores.reduce((total, item) => total + item.value, 0),
              )}
            />

            <DistributionPieChart items={distribuicaoInvestidores} />
          </article>
        </section>
          </>
        ) : (
          telaAtual === 'carteira' ? (
            <CarteiraView
              investidores={investidores}
              carteiras={carteirasPorInvestidor}
              selectedInvestor={investidorSelecionado}
              onSelectInvestor={setInvestidorSelecionado}
              snapshotSeries={dashboardSnapshot.timelinePorPessoa}
              saldosDisponiveis={dashboard?.saldosDisponiveis ?? []}
            />
          ) : (
            <OperacoesView
              investidores={investidores}
              selectedInvestor={investidorSelecionado}
              onSelectInvestor={setInvestidorSelecionado}
              operacoes={operacoes}
              onSaveOperation={salvarOperacao}
            />
          )
        )}

      </main>
    </div>
  )
}

export default App
