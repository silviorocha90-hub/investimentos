import { useMemo, useState } from 'react'

import { PageHeader } from '../../components/PageHeader'
import type { Dashboard, PosicaoAtivo } from '../../types/dashboard'
import type { Investidor } from '../../types/investidor'
import { formatarMoeda, formatarNumeroInteiro } from '../../utils/formatters'

import './Ativos.css'

interface AtivosViewProps {
  investidores: readonly Investidor[]
  carteiras: ReadonlyArray<{ nome: string; dashboard: Dashboard }>
  dashboardConsolidado: Dashboard | null
}

interface AtivoAnalise extends PosicaoAtivo {
  proventos: number
  participacaoProventos: number
  rentabilidade: number
}

function percentual(valor: number) {
  return `${valor.toLocaleString('pt-BR', {
    minimumFractionDigits: 1,
    maximumFractionDigits: 1,
  })}%`
}

export function AtivosView({
  investidores,
  carteiras,
  dashboardConsolidado,
}: AtivosViewProps) {
  const [investidor, setInvestidor] = useState('TOTAL')

  const investidoresComAtivos = useMemo(
    () => {
      const nomesComAtivos = new Set(
        carteiras
          .filter((item) =>
            item.dashboard.posicoes.some(
              (posicao) =>
                posicao.quantidade > 0 &&
                posicao.valorAtual > 0,
            ),
          )
          .map((item) => item.nome),
      )

      return investidores.filter((item) =>
        nomesComAtivos.has(item.nome),
      )
    },
    [investidores, carteiras],
  )

  const dashboardSelecionado =
    investidor === 'TOTAL'
      ? dashboardConsolidado
      : carteiras.find(
          (item) =>
            item.nome === investidor,
        )?.dashboard ?? null

  const dados = useMemo(() => {
    if (!dashboardSelecionado) {
      return {
        ativos: [] as AtivoAnalise[],
        totalProventosHistorico: 0,
      }
    }

    const posicoesVisiveis =
      dashboardSelecionado.posicoes
        .filter(
          (posicao) =>
            posicao.quantidade > 0 &&
            posicao.valorAtual > 0,
        )

    const totalProventos =
      posicoesVisiveis.reduce(
        (total, posicao) =>
          total +
          (posicao.proventos ?? 0),
        0,
      )

    const ativos =
      posicoesVisiveis
        .map(
          (posicao): AtivoAnalise => ({
            ...posicao,
            proventos:
              posicao.proventos ?? 0,
            participacaoProventos:
              totalProventos > 0
                ? ((posicao.proventos ?? 0) /
                    totalProventos) *
                  100
                : 0,
            rentabilidade:
              posicao.rentabilidadeEconomica ?? 0,
          }),
        )
        .sort(
          (a, b) =>
            b.valorAtual -
            a.valorAtual,
        )

    return {
      ativos,
      totalProventosHistorico:
        dashboardSelecionado.totalProventos,
    }
  }, [dashboardSelecionado])

  const ativos = dados.ativos

  const totais = useMemo(
    () => ({
      ativos: ativos.length,
      valorAtual: ativos.reduce(
        (total, item) =>
          total +
          item.valorAtual,
        0,
      ),
      proventos:
        dados.totalProventosHistorico,
      rentabilidade:
        dashboardSelecionado?.rentabilidadeAno ?? 0,
    }),
    [
      ativos,
      dados.totalProventosHistorico,
      dashboardSelecionado,
    ],
  )

  const maiorValor = Math.max(...ativos.map((x) => x.valorAtual), 1)

  return (
    <section className="portfolio-view ativos-analytics-page">
      <PageHeader
        titulo="Ativos"
        investidores={investidoresComAtivos}
        selectedInvestor={investidor}
        onSelectInvestor={setInvestidor}
        incluirTodos
        rotuloTodos="Todos"
      />

      <div className="ativos-kpis">
        <article className="kpi-violet"><span>Ativos em carteira</span><strong>{totais.ativos}</strong><i>◆</i></article>
        <article className="kpi-blue"><span>Rentabilidade</span><strong className={totais.rentabilidade >= 0 ? 'positive' : 'negative'}>{percentual(totais.rentabilidade)}</strong><i>↗</i></article>
        <article className="kpi-green"><span>Valor atual</span><strong>{formatarMoeda(totais.valorAtual)}</strong><i>●</i></article>
        <article className="kpi-gold"><span>Proventos recebidos</span><strong>{formatarMoeda(totais.proventos)}</strong><i>★</i></article>
      </div>

      <div className="ativos-grid">
        <article className="panel ativos-chart">
          <header><strong>Distribuição por ativo</strong><span>Participação no valor atual</span></header>
          <div className="bar-list">
            {ativos.slice(0, 12).map((item) => (
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
            {ativos
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
          <div><strong>Posições atuais</strong><span>Quantidade, valor atual, proventos e yield por ativo</span></div>
        </header>

        <div className="ativos-cards">
          {ativos.map((item) => {
            const tipo = item.tipoAtivoCodigo.trim().toUpperCase()
            const ticker = item.ticker.trim().toUpperCase()
            const exibirDesempenho =
              tipo !== 'PREVIDENCIA' &&
              tipo !== 'CDB' &&
              !ticker.includes('CDB')

            const yieldProventos =
              item.custoTotal > 0
                ? (item.proventos / item.custoTotal) * 100
                : 0

            return (
              <article className="ativo-card" key={item.ticker}>
                <div className="ativo-card-head">
                  <div>
                    <b>{item.ticker}</b>
                    <small>
                      {item.tipoAtivoNome.trim().toUpperCase() === 'PREVIDENCIA'
                        ? 'PREV'
                        : item.tipoAtivoNome}
                    </small>
                  </div>
                  {exibirDesempenho ? (
                    <strong className={item.rentabilidade >= 0 ? 'positive' : 'negative'}>
                      {percentual(item.rentabilidade)}
                    </strong>
                  ) : null}
                </div>
                <div className={`ativo-numbers ${exibirDesempenho ? 'ativo-numbers-renda-variavel' : ''}`}>
                  <div><span>Quantidade</span><b>{formatarNumeroInteiro(item.quantidade)}</b></div>
                  <div><span>Valor atual</span><b>{formatarMoeda(item.valorAtual)}</b></div>
                  {exibirDesempenho ? (
                    <>
                      <div>
                        <span>Proventos</span>
                        <b>{formatarMoeda(item.proventos)}</b>
                      </div>
                      <div>
                        <span>Yield</span>
                        <b className={yieldProventos >= 0 ? 'positive' : 'negative'}>
                          {percentual(yieldProventos)}
                        </b>
                      </div>
                    </>
                  ) : null}
                </div>
              </article>
            )
          })}
        </div>
      </article>
    </section>
  )
}
