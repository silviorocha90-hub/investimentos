import { useEffect, useMemo, useState } from 'react'

import { PageHeader } from '../../components/PageHeader'
import { SectionTitle } from '../../components/SectionTitle'
import { ColumnFilterHeader, TableFilterSummary } from '../../components/ColumnFilter'
import { useColumnFilters } from '../../components/useColumnFilters'
import type { FilterColumn } from '../../components/useColumnFilters'

import type { Dashboard, PosicaoAtivo } from '../../types/dashboard'
import type { Investidor } from '../../types/investidor'

import {
  formatarMoeda,
  formatarNumeroInteiro,
} from '../../utils/formatters'

import './Carteira.css'

interface CarteiraViewProps {
  investidores: readonly Investidor[]
  dashboardConsolidado: Dashboard | null
  carteiras: ReadonlyArray<{
    nome: string
    dashboard: Dashboard
  }>
  selectedInvestor: string
  permitirTodos?: boolean
  onSelectInvestor: (nome: string) => void
  snapshotSeries: Record<string, { data: string; carteira: number }[]>
   saldosDisponiveis?: ReadonlyArray<{
    investidor: string
    valor: number
  }>
}

function classeResultado(valor: number) {
  return valor >= 0 ? 'metric-positive' : 'metric-negative'
}

function ehOutroInvestimento(ticker: string, tipoAtivoCodigo: string) {
  const tickerNormalizado = ticker.trim().toUpperCase()
  const tipoNormalizado = tipoAtivoCodigo.trim().toUpperCase()

  if (tipoNormalizado === 'PREVIDENCIA') return true

  if (
    tickerNormalizado === 'CDB NEON' ||
    tickerNormalizado === 'CDB BTG' ||
    tickerNormalizado === 'FMP ELETROBRAS'
  ) {
    return true
  }

  return (
    tickerNormalizado.includes('FMP ELETROBRAS') ||
    tickerNormalizado.includes('ELETROBRAS') ||
    tipoNormalizado === 'FMP'
  )
}

function obterCategoria(ticker: string, tipoAtivoCodigo: string) {
  const tickerNormalizado = ticker.trim().toUpperCase()
  const tipoNormalizado = tipoAtivoCodigo.trim().toUpperCase()

  if (tipoNormalizado === 'PREVIDENCIA') return 'Previdência'

  if (
    tipoNormalizado === 'FMP' ||
    tickerNormalizado.includes('FMP') ||
    tickerNormalizado.includes('ELETROBRAS')
  ) {
    return 'FMP'
  }

  if (tickerNormalizado.includes('CDB')) return 'Renda Fixa'

  return tipoAtivoCodigo
}

const percentualCarteira = (valor: number) => `${valor.toLocaleString('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 })}%`
const colunasCarteira: readonly FilterColumn<PosicaoAtivo>[] = [
  { id: 'ativo', label: 'Ativo', value: p => p.ticker },
  { id: 'tipo', label: 'Tipo', value: p => p.tipoAtivoNome },
  { id: 'quantidade', label: 'Quantidade', value: p => formatarNumeroInteiro(p.quantidade) },
  { id: 'precoMedio', label: 'Preço Médio', value: p => formatarMoeda(p.precoMedio) },
  { id: 'precoAtual', label: 'Preço Atual', value: p => p.precoAtual == null ? '-' : formatarMoeda(p.precoAtual) },
  { id: 'custo', label: 'Custo Total', value: p => formatarMoeda(p.custoTotal) },
  { id: 'valor', label: 'Valor Atual', value: p => formatarMoeda(p.valorAtual) },
  { id: 'valorizacao', label: 'Valorização', value: p => formatarMoeda(p.valorizacao) },
  { id: 'rentabilidade', label: 'Rentab.', value: p => percentualCarteira(p.rentabilidadeEconomica ?? 0) },
]
const colunasOutros: readonly FilterColumn<PosicaoAtivo>[] = [
  { id: 'investimento', label: 'Investimento', value: p => p.ticker },
  { id: 'categoria', label: 'Categoria', value: p => obterCategoria(p.ticker, p.tipoAtivoCodigo) === 'Previdência' ? 'PREV' : obterCategoria(p.ticker, p.tipoAtivoCodigo) },
  { id: 'valor', label: 'Valor Atual', value: p => formatarMoeda(p.valorAtual) },
]

export function CarteiraView({
  investidores,
  dashboardConsolidado,
  carteiras,
  selectedInvestor,
  onSelectInvestor,
  permitirTodos = true,
}: CarteiraViewProps) {
  const [filtroInvestidor, setFiltroInvestidor] = useState(
    permitirTodos ? 'TOTAL' : selectedInvestor,
  )

  useEffect(() => {
    if (permitirTodos) {
      return
    }

    const permitido = investidores.some(
      (item) => item.nome === filtroInvestidor,
    )

    if (!permitido) {
      const primeiro = investidores[0]?.nome ?? ''
      setFiltroInvestidor(primeiro)
      onSelectInvestor(primeiro)
    }
  }, [
    permitirTodos,
    investidores,
    filtroInvestidor,
    onSelectInvestor,
  ])


  const investidoresComCarteira = useMemo(() => {
    const nomes = new Set(
      carteiras
        .filter((item) =>
          item.dashboard.posicoes.some(
            (posicao) => posicao.quantidade > 0 && posicao.valorAtual > 0,
          ),
        )
        .map((item) => item.nome),
    )

    return investidores.filter((item) => nomes.has(item.nome))
  }, [investidores, carteiras])

  const carteirasSelecionadas =
    filtroInvestidor === 'TOTAL'
      ? carteiras
      : carteiras.filter((item) => item.nome === filtroInvestidor)

  const dashboardSelecionado =
    filtroInvestidor === 'TOTAL'
      ? dashboardConsolidado
      : carteirasSelecionadas[0]?.dashboard ?? null

  const posicoes = useMemo(
    () =>
      (dashboardSelecionado?.posicoes ?? [])
        .filter(
          (posicao) =>
            posicao.quantidade > 0 &&
            posicao.valorAtual > 0,
        ),
    [dashboardSelecionado],
  )

  const dashboardTotal =
    filtroInvestidor === 'TOTAL' ? dashboardConsolidado : null

  const patrimonio = dashboardTotal?.patrimonioEstimado ??
    carteirasSelecionadas.reduce(
      (total, item) => total + (item.dashboard.patrimonioEstimado ?? 0), 0,
    )
  const valorAplicado = dashboardTotal?.valorAplicado ??
    carteirasSelecionadas.reduce(
      (total, item) => total + (item.dashboard.valorAplicado ?? 0), 0,
    )
  const valorDisponivel = dashboardTotal?.caixaDisponivel ??
    carteirasSelecionadas.reduce(
      (total, item) => total + (item.dashboard.caixaDisponivel ?? 0), 0,
    )
  const resultadoRealizadoAcoes = dashboardTotal?.resultadoRealizadoAcoes ??
    carteirasSelecionadas.reduce(
      (total, item) => total + (item.dashboard.resultadoRealizadoAcoes ?? 0), 0,
    )
  const totalProventos = dashboardTotal?.totalProventos ??
    carteirasSelecionadas.reduce(
      (total, item) => total + (item.dashboard.totalProventos ?? 0), 0,
    )
  const resultadoOpcoes =
    (dashboardTotal
      ? (dashboardTotal.premioLiquidoOpcoes ?? 0)
      : carteirasSelecionadas.reduce(
          (total, item) =>
            total + (item.dashboard.premioLiquidoOpcoes ?? 0),
          0,
        ))

  /*
   * O resultado econômico é calculado exclusivamente pelo backend.
   * A Carteira apenas apresenta a regra central do motor financeiro,
   * evitando manter uma segunda implementação da fórmula no React.
   */
  const resultadoCarteira =
    dashboardTotal?.resultadoCarteira ??
    carteirasSelecionadas.reduce(
      (total, item) =>
        total +
        (item.dashboard.resultadoCarteira ?? 0),
      0,
    )

  /*
   * RentabilidadeAno é um nome legado do contrato.
   * Conforme o CODEX, representa a rentabilidade econômica acumulada
   * da carteira e não uma rentabilidade limitada ao ano corrente.
   */
  const crescimentoCarteira =
    dashboardSelecionado?.rentabilidadeAno ?? null

  const rendaVariavel = posicoes
    .filter((posicao) => !ehOutroInvestimento(posicao.ticker, posicao.tipoAtivoCodigo))
    .sort((a, b) => b.valorAtual - a.valorAtual)

  const outrosInvestimentos = posicoes
    .filter((posicao) => ehOutroInvestimento(posicao.ticker, posicao.tipoAtivoCodigo))
    .sort((a, b) => b.valorAtual - a.valorAtual)

  const percentual = (valor: number) =>
    `${valor.toLocaleString('pt-BR', {
      minimumFractionDigits: 1,
      maximumFractionDigits: 1,
    })}%`

  const filtrosRenda = useColumnFilters(rendaVariavel, colunasCarteira, filtroInvestidor)
  const filtrosOutros = useColumnFilters(outrosInvestimentos, colunasOutros, filtroInvestidor)

  function selecionarInvestidor(nome: string) {
    setFiltroInvestidor(nome)
    if (nome !== 'TOTAL') onSelectInvestor(nome)
  }

  return (
    <section className="portfolio-view carteira-page">
      <PageHeader
        titulo="Carteira"
        investidores={investidoresComCarteira}
        selectedInvestor={filtroInvestidor}
        onSelectInvestor={selecionarInvestidor}
        incluirTodos={permitirTodos}
        rotuloTodos="Todos"
      />

      <div className="carteira-kpis">
        <article className="carteira-kpi kpi-violet"><span>Patrimônio Atual</span><strong>{formatarMoeda(patrimonio)}</strong><i>◆</i></article>
        <article className="carteira-kpi kpi-blue"><span>Valor Aplicado</span><strong>{formatarMoeda(valorAplicado)}</strong><i>▥</i></article>
        <article className="carteira-kpi kpi-green"><span>Disponível</span><strong>{formatarMoeda(valorDisponivel)}</strong><i>●</i></article>
        <article className="carteira-kpi kpi-gold"><span>Crescimento da Carteira</span><strong className={crescimentoCarteira == null ? '' : classeResultado(crescimentoCarteira)}>{crescimentoCarteira == null ? '-' : percentual(crescimentoCarteira)}</strong><i>↗</i></article>
      </div>

      <article className="panel portfolio-positions portfolio-variable-income carteira-panel">
        <SectionTitle title="Renda Variável" subtitle="Ações e FIIs com cotação em mercado" />
        {rendaVariavel.length > 0 ? (
          <>
          <TableFilterSummary shown={filtrosRenda.filteredRows.length} total={rendaVariavel.length} active={filtrosRenda.active} onClear={filtrosRenda.clear} />
          <div className="table-wrap compact">
            <table className="data-table carteira-positions-table">
              <thead><tr>
                {colunasCarteira.map(coluna => <ColumnFilterHeader key={coluna.id} label={coluna.label} values={filtrosRenda.values[coluna.id]} selected={filtrosRenda.filters[coluna.id]} onChange={values => filtrosRenda.setFilter(coluna.id, values)} />)}
              </tr></thead>
              <tbody>
                {filtrosRenda.filteredRows.map((posicao) => {
                  const rentabilidade =
                    posicao.rentabilidadeEconomica ?? 0
                  return (
                    <tr key={posicao.ticker}>
                      <td><span className="ticker">{posicao.ticker}</span></td>
                      <td>{posicao.tipoAtivoNome}</td>
                      <td className="align-right">{formatarNumeroInteiro(posicao.quantidade)}</td>
                      <td className="align-right">{formatarMoeda(posicao.precoMedio)}</td>
                      <td className="align-right">{posicao.precoAtual == null ? '-' : formatarMoeda(posicao.precoAtual)}</td>
                      <td className="align-right">{formatarMoeda(posicao.custoTotal)}</td>
                      <td className="align-right strong">{formatarMoeda(posicao.valorAtual)}</td>
                      <td className={`align-right strong ${classeResultado(posicao.valorizacao)}`}>{formatarMoeda(posicao.valorizacao)}</td>
                      <td className={`align-right strong ${classeResultado(rentabilidade)}`}>{percentual(rentabilidade)}</td>
                    </tr>
                  )
                })}
                {filtrosRenda.filteredRows.length === 0 && <tr><td colSpan={colunasCarteira.length} className="column-filter-no-results">Nenhuma posição corresponde aos filtros das colunas.</td></tr>}
              </tbody>
            </table>
          </div>
          </>
        ) : <div className="empty-state"><strong>Nenhuma posição de renda variável encontrada</strong></div>}
      </article>

      <article className="panel portfolio-other-investments carteira-panel">
        <SectionTitle title="Outros Investimentos" subtitle="FMP, renda fixa e previdência apresentados pelo valor patrimonial" />
        {outrosInvestimentos.length > 0 ? (
          <>
          <TableFilterSummary shown={filtrosOutros.filteredRows.length} total={outrosInvestimentos.length} active={filtrosOutros.active} onClear={filtrosOutros.clear} />
          <div className="table-wrap compact">
            <table className="data-table carteira-other-investments-table">
              <thead><tr>{colunasOutros.map(coluna => <ColumnFilterHeader key={coluna.id} label={coluna.label} values={filtrosOutros.values[coluna.id]} selected={filtrosOutros.filters[coluna.id]} onChange={values => filtrosOutros.setFilter(coluna.id, values)} />)}</tr></thead>
              <tbody>{filtrosOutros.filteredRows.map((posicao) => (
                <tr key={posicao.ticker}>
                  <td><span className="ticker">{posicao.ticker}</span></td>
                  <td><span className="investment-category">{obterCategoria(posicao.ticker, posicao.tipoAtivoCodigo) === 'Previdência' ? 'PREV' : obterCategoria(posicao.ticker, posicao.tipoAtivoCodigo)}</span></td>
                  <td className="align-right strong">{formatarMoeda(posicao.valorAtual)}</td>
                </tr>
              ))}
              {filtrosOutros.filteredRows.length === 0 && <tr><td colSpan={colunasOutros.length} className="column-filter-no-results">Nenhum investimento corresponde aos filtros das colunas.</td></tr>}
              </tbody>
            </table>
          </div>
          </>
        ) : <div className="empty-state"><strong>Nenhum outro investimento encontrado</strong></div>}
      </article>

      <div className="carteira-result-metrics">
        <article className={`carteira-result-card ${classeResultado(resultadoRealizadoAcoes)}`}><span>Resultado realizado em ações</span><strong>{formatarMoeda(resultadoRealizadoAcoes)}</strong></article>
        <article className={`carteira-result-card ${classeResultado(totalProventos)}`}><span>Proventos</span><strong>{formatarMoeda(totalProventos)}</strong></article>
        <article className={`carteira-result-card ${classeResultado(resultadoOpcoes)}`}><span>Resultado de Opções</span><strong>{formatarMoeda(resultadoOpcoes)}</strong></article>
        <article className={`carteira-result-card carteira-total-result ${classeResultado(resultadoCarteira)}`}><span>Resultado da Carteira</span><strong>{formatarMoeda(resultadoCarteira)}</strong></article>
      </div>
    </section>
  )
}
