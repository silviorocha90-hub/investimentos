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
  snapshotSeries,
  saldosDisponiveis = [],
}: CarteiraViewProps) {
  const carteira = carteiras.find(
    (item) => item.nome === selectedInvestor,
  )?.dashboard

  const pontos = snapshotSeries[selectedInvestor] ?? []

  const patrimonioBaseSnapshot =
    pontos
      .slice()
      .sort((a, b) => a.data.localeCompare(b.data))
      .at(-1)?.carteira ?? 0

  const patrimonioApi = carteira?.patrimonioEstimado ?? 0

  const patrimonio =
    patrimonioApi > 0 ? patrimonioApi : patrimonioBaseSnapshot

  const valorAplicado = carteira?.valorAplicado ?? 0

  const saldoConsolidado =
    saldosDisponiveis.find(
      (item) => item.investidor === selectedInvestor,
    )?.valor ?? 0

  const valorDisponivel = carteira
    ? carteira.caixaDisponivel ?? saldoConsolidado
    : saldoConsolidado

  const resultadoRealizadoAcoes =
    carteira?.resultadoRealizadoAcoes ?? 0

  const totalProventos = carteira?.totalProventos ?? 0

  const resultadoOpcoes =
    carteira?.opcoesBrutas ??
    carteira?.premioLiquidoOpcoes ??
    0

  const resultadoCarteira =
    carteira?.resultadoRealizado ?? 0

  const posicoes = carteira?.posicoes ?? []

  const rendaVariavel = posicoes
    .filter(
      (posicao) =>
        !ehOutroInvestimento(
          posicao.ticker,
          posicao.tipoAtivoCodigo,
        ),
    )
    .slice()
    .sort((a, b) => b.valorAtual - a.valorAtual)

  const outrosInvestimentos = posicoes
    .filter((posicao) =>
      ehOutroInvestimento(
        posicao.ticker,
        posicao.tipoAtivoCodigo,
      ),
    )
    .slice()
    .sort((a, b) => b.valorAtual - a.valorAtual)

  return (
    <section className="portfolio-view carteira-page">
      <PageHeader
        titulo="Carteira"
        investidores={investidores}
        selectedInvestor={selectedInvestor}
        onSelectInvestor={onSelectInvestor}
      />

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

      <article className="panel portfolio-positions portfolio-variable-income">
        <SectionTitle
          title="Renda Variável"
          subtitle="Ações e FIIs com cotação em mercado"
        />

        {rendaVariavel.length > 0 ? (
          <div className="table-wrap compact">
            <table className="data-table carteira-positions-table">
              <thead>
                <tr>
                  <th>Ativo</th>
                  <th>Tipo</th>
                  <th>Quantidade</th>
                  <th>Preço Médio</th>
                  <th>Preço Atual</th>
                  <th>Custo Total</th>
                  <th>Valor Atual</th>
                  <th>Valorização</th>
                </tr>
              </thead>

              <tbody>
                {rendaVariavel.map((posicao) => (
                  <tr key={posicao.ticker}>
                    <td>
                      <span className="ticker">
                        {posicao.ticker}
                      </span>
                    </td>

                    <td>{posicao.tipoAtivoNome}</td>

                    <td className="align-right">
                      {formatarNumeroInteiro(posicao.quantidade)}
                    </td>

                    <td className="align-right">
                      {formatarMoeda(posicao.precoMedio)}
                    </td>

                    <td className="align-right">
                      {posicao.precoAtual == null
                        ? '-'
                        : formatarMoeda(posicao.precoAtual)}
                    </td>

                    <td className="align-right">
                      {formatarMoeda(posicao.custoTotal)}
                    </td>

                    <td className="align-right strong">
                      {formatarMoeda(posicao.valorAtual)}
                    </td>

                    <td
                      className={`align-right strong ${
                        posicao.precoAtual == null
                          ? ''
                          : posicao.valorizacao >= 0
                            ? 'positive'
                            : 'negative'
                      }`}
                    >
                      {posicao.precoAtual == null
                        ? '-'
                        : formatarMoeda(posicao.valorizacao)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="empty-state">
            <strong>
              Nenhuma posição de renda variável encontrada
            </strong>
          </div>
        )}
      </article>

      <article className="panel portfolio-other-investments">
        <SectionTitle
          title="Outros Investimentos"
          subtitle="FMP, renda fixa e previdência apresentados pelo valor patrimonial"
        />

        {outrosInvestimentos.length > 0 ? (
          <div className="table-wrap compact">
            <table className="data-table carteira-other-investments-table">
              <thead>
                <tr>
                  <th>Investimento</th>
                  <th>Categoria</th>
                  <th>Valor Atual</th>
                </tr>
              </thead>

              <tbody>
                {outrosInvestimentos.map((posicao) => (
                  <tr key={posicao.ticker}>
                    <td>
                      <span className="ticker">
                        {posicao.ticker}
                      </span>
                    </td>

                    <td>
                      <span className="investment-category">
                        {obterCategoria(
                          posicao.ticker,
                          posicao.tipoAtivoCodigo,
                        )}
                      </span>
                    </td>

                    <td className="align-right strong">
                      {formatarMoeda(posicao.valorAtual)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="empty-state">
            <strong>
              Nenhum outro investimento encontrado
            </strong>
          </div>
        )}
      </article>

      <div className="portfolio-secondary-metrics">
        <article
          className={`portfolio-metric portfolio-result-card ${classeResultado(
            resultadoRealizadoAcoes,
          )}`}
        >
          <span>Resultado realizado em ações</span>
          <strong>
            {formatarMoeda(resultadoRealizadoAcoes)}
          </strong>
        </article>

        <article
          className={`portfolio-metric portfolio-result-card ${classeResultado(
            totalProventos,
          )}`}
        >
          <span>Proventos</span>
          <strong>{formatarMoeda(totalProventos)}</strong>
        </article>

        <article
          className={`portfolio-metric portfolio-result-card ${classeResultado(
            resultadoOpcoes,
          )}`}
        >
          <span>Resultado de Opções</span>
          <strong>{formatarMoeda(resultadoOpcoes)}</strong>
        </article>

        <article
          className={`portfolio-metric portfolio-result-card portfolio-total-result ${classeResultado(
            resultadoCarteira,
          )}`}
        >
          <span>Resultado da Carteira</span>
          <strong>
            {formatarMoeda(resultadoCarteira)}
          </strong>
        </article>
      </div>
    </section>
  )
}
