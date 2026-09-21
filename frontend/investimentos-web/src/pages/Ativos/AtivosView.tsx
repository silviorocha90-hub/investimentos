import { useMemo, useState } from 'react'

import { PageHeader } from '../../components/PageHeader'
import type { Dashboard, PosicaoAtivo } from '../../types/dashboard'
import type { Investidor } from '../../types/investidor'
import { formatarMoeda, formatarNumeroInteiro } from '../../utils/formatters'

import './Ativos.css'

interface AtivosViewProps {
  investidores: readonly Investidor[]
  carteiras: ReadonlyArray<{ nome: string; dashboard: Dashboard }>
}

interface AtivoAnalise extends PosicaoAtivo {
  proventos: number
  participacaoProventos: number
  rentabilidade: number
}

const META_PADRAO = 1000

function percentual(valor: number) {
  return `${valor.toLocaleString('pt-BR', {
    minimumFractionDigits: 1,
    maximumFractionDigits: 1,
  })}%`
}

export function AtivosView({
  investidores,
  carteiras,
}: AtivosViewProps) {
  const [investidor, setInvestidor] = useState('TOTAL')

  const dados = useMemo(() => {
    const selecionadas =
      investidor === 'TOTAL'
        ? carteiras
        : carteiras.filter((item) => item.nome === investidor)

    const mapa = new Map<string, AtivoAnalise>()

    selecionadas.forEach(({ dashboard }) => {
      dashboard.posicoes.forEach((posicao) => {
        if (posicao.quantidade <= 0) return

        const atual = mapa.get(posicao.ticker)
        const proventos = dashboard.proventos
          .filter((item) => item.ticker === posicao.ticker)
          .reduce((total, item) => total + item.valorRecebido, 0)

        if (!atual) {
          mapa.set(posicao.ticker, {
            ...posicao,
            proventos,
            participacaoProventos: 0,
            rentabilidade: 0,
          })
          return
        }

        atual.quantidade += posicao.quantidade
        atual.custoTotal += posicao.custoTotal
        atual.valorAtual += posicao.valorAtual
        atual.valorizacao += posicao.valorizacao
        atual.resultadoRealizado += posicao.resultadoRealizado
        atual.proventos += proventos
        atual.precoMedio =
          atual.quantidade > 0
            ? atual.custoTotal / atual.quantidade
            : 0
      })
    })

    const lista = [...mapa.values()]
    const totalProventos = lista.reduce((total, item) => total + item.proventos, 0)

    lista.forEach((item) => {
      item.rentabilidade =
        item.custoTotal > 0
          ? ((item.valorAtual - item.custoTotal) / item.custoTotal) * 100
          : 0
      item.participacaoProventos =
        totalProventos > 0 ? (item.proventos / totalProventos) * 100 : 0
    })

    return lista
      .filter(
        (item) =>
          item.quantidade > 0 &&
          item.valorAtual > 0,
      )
      .sort(
        (a, b) =>
          b.valorAtual -
          a.valorAtual,
      )
  }, [carteiras, investidor])

  const totais = useMemo(() => ({
    ativos: dados.length,
    valorAtual: dados.reduce((t, x) => t + x.valorAtual, 0),
    proventos: dados.reduce((t, x) => t + x.proventos, 0),
    quantidade: dados.reduce((t, x) => t + x.quantidade, 0),
  }), [dados])

  const maiorValor = Math.max(...dados.map((x) => x.valorAtual), 1)

  return (
    <section className="portfolio-view ativos-analytics-page">
      <PageHeader
        titulo="Ativos"
        investidores={investidores}
        selectedInvestor={investidor}
        onSelectInvestor={setInvestidor}
        incluirTodos
        rotuloTodos="Total"
      />

      <div className="ativos-kpis">
        <article className="kpi-violet"><span>Ativos em carteira</span><strong>{totais.ativos}</strong><i>◆</i></article>
        <article className="kpi-blue"><span>Quantidade total</span><strong>{formatarNumeroInteiro(totais.quantidade)}</strong><i>▥</i></article>
        <article className="kpi-green"><span>Valor atual</span><strong>{formatarMoeda(totais.valorAtual)}</strong><i>●</i></article>
        <article className="kpi-gold"><span>Proventos recebidos</span><strong>{formatarMoeda(totais.proventos)}</strong><i>★</i></article>
      </div>

      <div className="ativos-grid">
        <article className="panel ativos-chart">
          <header><strong>Distribuição por ativo</strong><span>Participação no valor atual</span></header>
          <div className="bar-list">
            {dados.slice(0, 12).map((item) => (
              <div className="bar-row" key={item.ticker} style={{ '--bar-size': `${(item.valorAtual / maiorValor) * 100}%` } as React.CSSProperties}>
                <b>{item.ticker}</b>
                <div className="bar-track"><i /></div>
                <span>{formatarMoeda(item.valorAtual)}</span>
              </div>
            ))}
          </div>
        </article>

        <article className="panel ativos-chart">
          <header><strong>Proventos por ativo</strong><span>% do total recebido</span></header>
          <div className="dividend-list">
            {dados
              .filter(
                (item) =>
                  item.proventos > 0,
              )
              .slice()
              .sort((a, b) => b.proventos - a.proventos)
              .slice(0, 10)
              .map((item) => (
                <div className="dividend-row" key={item.ticker}>
                  <div><b>{item.ticker}</b><small>{formatarMoeda(item.proventos)}</small></div>
                  <strong>{percentual(item.participacaoProventos)}</strong>
                </div>
              ))}
          </div>
        </article>
      </div>

      <article className="panel ativos-progress-panel">
        <header>
          <div><strong>Evolução das posições</strong><span>Quantidade atual em relação à meta padrão de {META_PADRAO.toLocaleString('pt-BR')} ativos</span></div>
        </header>

        <div className="ativos-cards">
          {dados.map((item) => {
            const progresso = Math.min((item.quantidade / META_PADRAO) * 100, 100)
            return (
              <article className="ativo-card" key={item.ticker} style={{ '--goal-size': `${progresso}%` } as React.CSSProperties}>
                <div className="ativo-card-head">
                  <div><b>{item.ticker}</b><small>{item.tipoAtivoNome}</small></div>
                  <strong className={item.rentabilidade >= 0 ? 'positive' : 'negative'}>
                    {percentual(item.rentabilidade)}
                  </strong>
                </div>
                <div className="ativo-numbers">
                  <div><span>Atual</span><b>{formatarNumeroInteiro(item.quantidade)}</b></div>
                  <div><span>Faltam</span><b>{formatarNumeroInteiro(Math.max(META_PADRAO - item.quantidade, 0))}</b></div>
                  <div><span>% Proventos</span><b>{percentual(item.participacaoProventos)}</b></div>
                </div>
                <div className="goal-track"><i /></div>
                <footer><span>{percentual(progresso)} da meta</span><strong>{formatarMoeda(item.valorAtual)}</strong></footer>
              </article>
            )
          })}
        </div>
      </article>
    </section>
  )
}
