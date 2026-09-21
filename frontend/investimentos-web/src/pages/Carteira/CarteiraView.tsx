import { useEffect, useMemo, useState } from 'react'

import { PageHeader } from '../../components/PageHeader'
import { SectionTitle } from '../../components/SectionTitle'

import type { Dashboard } from '../../types/dashboard'
import type { Investidor } from '../../types/investidor'

import {
  formatarMoeda,
  formatarNumeroInteiro,
} from '../../utils/formatters'

import './Carteira.css'

interface CarteiraViewProps {
  investidores: readonly Investidor[]
  carteiras: ReadonlyArray<{
    nome: string
    dashboard: Dashboard
  }>
  selectedInvestor: string
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

export function CarteiraView({
  investidores,
  carteiras,
  selectedInvestor,
  onSelectInvestor,
}: CarteiraViewProps) {
  const [filtroInvestidor, setFiltroInvestidor] = useState('TOTAL')

  useEffect(() => {
    setFiltroInvestidor('TOTAL')
  }, [])

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

  const posicoes = useMemo(() => {
    const mapa = new Map<string, Dashboard['posicoes'][number]>()

    carteirasSelecionadas.forEach(({ dashboard }) => {
      dashboard.posicoes.forEach((posicao) => {
        if (posicao.quantidade <= 0 || posicao.valorAtual <= 0) return

        const atual = mapa.get(posicao.ticker)
        if (!atual) {
          mapa.set(posicao.ticker, { ...posicao })
          return
        }

        const quantidade = atual.quantidade + posicao.quantidade
        const custoTotal = atual.custoTotal + posicao.custoTotal
        const valorAtual = atual.valorAtual + posicao.valorAtual

        mapa.set(posicao.ticker, {
          ...atual,
          quantidade,
          custoTotal,
          valorAtual,
          precoMedio: quantidade > 0 ? custoTotal / quantidade : 0,
          precoAtual: quantidade > 0 ? valorAtual / quantidade : null,
          valorizacao: atual.valorizacao + posicao.valorizacao,
          resultadoRealizado:
            atual.resultadoRealizado + posicao.resultadoRealizado,
        })
      })
    })

    return [...mapa.values()]
  }, [carteirasSelecionadas])

  const patrimonio = carteirasSelecionadas.reduce(
    (total, item) => total + (item.dashboard.patrimonioEstimado ?? 0),
    0,
  )
  const valorAplicado = carteirasSelecionadas.reduce(
    (total, item) => total + (item.dashboard.valorAplicado ?? 0),
    0,
  )
  const valorDisponivel = carteirasSelecionadas.reduce(
    (total, item) => total + (item.dashboard.caixaDisponivel ?? 0),
    0,
  )
  const resultadoRealizadoAcoes = carteirasSelecionadas.reduce(
    (total, item) => total + (item.dashboard.resultadoRealizadoAcoes ?? 0),
    0,
  )
  const totalProventos = carteirasSelecionadas.reduce(
    (total, item) => total + (item.dashboard.totalProventos ?? 0),
    0,
  )
  const resultadoOpcoes = carteirasSelecionadas.reduce(
    (total, item) =>
      total + (item.dashboard.opcoesBrutas ?? item.dashboard.premioLiquidoOpcoes ?? 0),
    0,
  )
  const resultadoCarteira = carteirasSelecionadas.reduce(
    (total, item) => total + (item.dashboard.resultadoRealizado ?? 0),
    0,
  )

  const custoTotal = posicoes.reduce((total, item) => total + item.custoTotal, 0)
  const valorAtualTotal = posicoes.reduce((total, item) => total + item.valorAtual, 0)
  const rentabilidadeTotal =
    custoTotal > 0
      ? ((valorAtualTotal - custoTotal + totalProventos) / custoTotal) * 100
      : 0

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
        incluirTodos
        rotuloTodos="Todos"
      />

      <div className="carteira-kpis">
        <article className="carteira-kpi kpi-violet"><span>Patrimônio Atual</span><strong>{formatarMoeda(patrimonio)}</strong><i>◆</i></article>
        <article className="carteira-kpi kpi-blue"><span>Valor Aplicado</span><strong>{formatarMoeda(valorAplicado)}</strong><i>▥</i></article>
        <article className="carteira-kpi kpi-green"><span>Disponível</span><strong>{formatarMoeda(valorDisponivel)}</strong><i>●</i></article>
        <article className="carteira-kpi kpi-gold"><span>Rentabilidade Total</span><strong>{percentual(rentabilidadeTotal)}</strong><i>↗</i></article>
      </div>

      <article className="panel portfolio-positions portfolio-variable-income carteira-panel">
        <SectionTitle title="Renda Variável" subtitle="Ações e FIIs com cotação em mercado" />
        {rendaVariavel.length > 0 ? (
          <div className="table-wrap compact">
            <table className="data-table carteira-positions-table">
              <thead><tr>
                <th>Ativo</th><th>Tipo</th><th>Quantidade</th><th>Preço Médio</th>
                <th>Preço Atual</th><th>Custo Total</th><th>Valor Atual</th>
                <th>Valorização</th><th>Rentab.</th>
              </tr></thead>
              <tbody>
                {rendaVariavel.map((posicao) => {
                  const rentabilidade =
                    posicao.custoTotal > 0
                      ? (posicao.valorizacao / posicao.custoTotal) * 100
                      : 0
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
              </tbody>
            </table>
          </div>
        ) : <div className="empty-state"><strong>Nenhuma posição de renda variável encontrada</strong></div>}
      </article>

      <article className="panel portfolio-other-investments carteira-panel">
        <SectionTitle title="Outros Investimentos" subtitle="FMP, renda fixa e previdência apresentados pelo valor patrimonial" />
        {outrosInvestimentos.length > 0 ? (
          <div className="table-wrap compact">
            <table className="data-table carteira-other-investments-table">
              <thead><tr><th>Investimento</th><th>Categoria</th><th>Valor Atual</th></tr></thead>
              <tbody>{outrosInvestimentos.map((posicao) => (
                <tr key={posicao.ticker}>
                  <td><span className="ticker">{posicao.ticker}</span></td>
                  <td><span className="investment-category">{obterCategoria(posicao.ticker, posicao.tipoAtivoCodigo) === 'Previdência' ? 'PREV' : obterCategoria(posicao.ticker, posicao.tipoAtivoCodigo)}</span></td>
                  <td className="align-right strong">{formatarMoeda(posicao.valorAtual)}</td>
                </tr>
              ))}</tbody>
            </table>
          </div>
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
