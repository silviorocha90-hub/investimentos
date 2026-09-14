import { PageHeader } from '../../components/PageHeader'
import { SectionTitle } from '../../components/SectionTitle'

import type { Dashboard } from '../../types/dashboard'
import type { Investidor } from '../../types/investidor'

import './Carteira.css'

interface CarteiraViewProps {
  investidores: readonly Investidor[]
  carteiras: ReadonlyArray<{
    nome: string
    dashboard: Dashboard
  }>
  selectedInvestor: string
  onSelectInvestor: (nome: string) => void
  snapshotSeries: Record<
    string,
    {
      data: string
      carteira: number
    }[]
  >
  saldosDisponiveis?: ReadonlyArray<{
    investidor: string
    valor: number
  }>
}

function formatarMoeda(
  valor: number | null | undefined,
) {
  if (
    valor == null ||
    Number.isNaN(valor)
  ) {
    return '--'
  }

  return valor.toLocaleString(
    'pt-BR',
    {
      style: 'currency',
      currency: 'BRL',
    },
  )
}

function formatarNumeroInteiro(
  valor: number,
) {
  return valor.toLocaleString(
    'pt-BR',
    {
      maximumFractionDigits: 2,
    },
  )
}

function classeResultado(
  valor: number,
) {
  return valor >= 0
    ? 'metric-positive'
    : 'metric-negative'
}

/*
 * POSIÇÕES PATRIMONIAIS SEM MARCAÇÃO
 * POR COTAÇÃO.
 *
 * Para esses ativos:
 *
 * Preço Atual = -
 * Valor Atual = Custo Total
 * Valorização = -
 *
 * O backend continua sendo responsável
 * pelo cálculo patrimonial.
 *
 * Esta função controla somente a
 * apresentação na tabela da Carteira.
 */
function ehAtivoSemMarcacaoPorCotacao(
  ticker: string,
  tipoAtivoCodigo: string,
) {
  const tickerNormalizado =
    ticker
      .trim()
      .toUpperCase()

  const tipoNormalizado =
    tipoAtivoCodigo
      .trim()
      .toUpperCase()

  if (
    tipoNormalizado === 'PREVIDENCIA'
  ) {
    return true
  }

  if (
    tickerNormalizado === 'CDB NEON' ||
    tickerNormalizado === 'CDB BTG'  ||
    tickerNormalizado === 'FMP ELETROBRAS'  
  ) {
    return true
  }

  if (
    tickerNormalizado.includes(
      'FMP ELETROBRAS',
    ) ||
    tickerNormalizado.includes(
      'ELETROBRAS',
    ) ||
    tipoNormalizado === 'FMP'
  ) {
    return true
  }

  return false
}
 
export function CarteiraView({
  investidores,
  carteiras,
  selectedInvestor,
  onSelectInvestor,
  snapshotSeries,
  saldosDisponiveis = [],
}: CarteiraViewProps) {
  const carteira =
    carteiras.find(
      (item) =>
        item.nome ===
        selectedInvestor,
    )?.dashboard

  /*
   * O snapshot permanece apenas como
   * fallback histórico.
   *
   * Os números atuais da carteira vêm
   * prioritariamente da API.
   */
  const pontos =
    snapshotSeries[
      selectedInvestor
    ] ?? []

  const patrimonioBaseSnapshot =
    pontos
      .slice()
      .sort(
        (a, b) =>
          a.data.localeCompare(
            b.data,
          ),
      )
      .at(-1)
      ?.carteira ?? 0

  const patrimonioApi =
    carteira?.patrimonioEstimado ??
    0

  const patrimonio =
    patrimonioApi > 0
      ? patrimonioApi
      : patrimonioBaseSnapshot

  const valorAplicado =
    carteira?.valorAplicado ?? 0

  const saldoConsolidado =
    saldosDisponiveis.find(
      (item) =>
        item.investidor ===
        selectedInvestor,
    )?.valor ?? 0

  const valorDisponivel =
    carteira
      ? carteira
          .caixaDisponivel ??
        saldoConsolidado
      : saldoConsolidado

  /*
   * COMPONENTES DO RESULTADO
   *
   * Valorização:
   * diferença entre valor atual
   * das posições e custo total.
   *
   * Proventos:
   * dividendos, JCP etc.
   *
   * Opções:
   * resultado das opções
   * encerradas/executadas.
   *
   * Resultado Total:
   * Valorização
   * + Proventos
   * + Opções.
   */
  const valorizacaoAtivos =
    carteira?.valorizacaoAtivos ??
    0

  const totalProventos =
    carteira?.totalProventos ??
    0

  const premioOpcoes =
    carteira
      ?.premioLiquidoOpcoes ??
    0

  const resultadoTotal =
    carteira
      ?.resultadoRealizado ??
    0

  const posicoes =
    carteira?.posicoes ?? []

  const posicoesOrdenadas =
    posicoes
      .slice()
      .sort(
        (a, b) =>
          b.valorAtual -
          a.valorAtual,
      )

  return (
    <section className="portfolio-view carteira-page">
      <PageHeader
        titulo="Carteira"
        investidores={
          investidores
        }
        selectedInvestor={
          selectedInvestor
        }
        onSelectInvestor={
          onSelectInvestor
        }
      />

      <div className="portfolio-metrics">
        <article className="portfolio-metric accent">
          <span>
            Patrimônio Atual
          </span>

          <strong>
            {formatarMoeda(
              patrimonio,
            )}
          </strong>
        </article>

        <article className="portfolio-metric">
          <span>
            Valor Aplicado
          </span>

          <strong>
            {formatarMoeda(
              valorAplicado,
            )}
          </strong>
        </article>

        <article className="portfolio-metric">
          <span>
            Disponível
          </span>

          <strong>
            {formatarMoeda(
              valorDisponivel,
            )}
          </strong>
        </article>
      </div>

      <article className="panel portfolio-positions">
        <SectionTitle
          title={`Posições de ${selectedInvestor}`}
          subtitle="Posições atuais da carteira"
          badge={`${posicoesOrdenadas.length} ativos`}
        />

        {posicoesOrdenadas.length >
        0 ? (
          <div className="table-wrap compact">
            <table className="data-table carteira-positions-table">
              <thead>
                <tr>
                  <th>
                    Ativo
                  </th>

                  <th>
                    Quantidade
                  </th>

                  <th>
                    Preço Médio
                  </th>

                  <th>
                    Preço Atual
                  </th>

                  <th>
                    Custo Total
                  </th>

                  <th>
                    Valor Atual
                  </th>

                  <th>
                    Valorização
                  </th>
                </tr>
              </thead>

              <tbody>
                {posicoesOrdenadas.map(
                  (posicao) => {
                    const semMarcacaoPorCotacao =
                      ehAtivoSemMarcacaoPorCotacao(
                        posicao.ticker,
                        posicao.tipoAtivoCodigo,
                      )

                    return (
                      <tr
                        key={
                          posicao.ticker
                        }
                      >
                        <td>
                          <span className="ticker">
                            {
                              posicao.ticker
                            }
                          </span>

                          {posicao.nome
                            .trim()
                            .toUpperCase() !==
                          posicao.ticker
                            .trim()
                            .toUpperCase() ? (
                            <span className="subtle-inline">
                              {' '}
                              {
                                posicao.nome
                              }
                            </span>
                          ) : null}
                        </td>

                        <td className="align-right">
                          {formatarNumeroInteiro(
                            posicao.quantidade,
                          )}
                        </td>

                        <td className="align-right">
                          {formatarMoeda(
                            posicao.precoMedio,
                          )}
                        </td>

                        <td className="align-right">
                          {semMarcacaoPorCotacao
                            ? '-'
                            : formatarMoeda(
                                posicao.precoAtual,
                              )}
                        </td>

                        <td className="align-right">
                          {formatarMoeda(
                            posicao.custoTotal,
                          )}
                        </td>

                        <td className="align-right strong">
                          {formatarMoeda(
                            posicao.valorAtual,
                          )}
                        </td>

                        <td
                          className={`align-right strong ${
                            semMarcacaoPorCotacao
                              ? ''
                              : posicao.valorizacao >=
                                  0
                                ? 'positive'
                                : 'negative'
                          }`}
                        >
                          {semMarcacaoPorCotacao
                            ? '-'
                            : formatarMoeda(
                                posicao.valorizacao,
                              )}
                        </td>
                      </tr>
                    )
                  },
                )}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="empty-state">
            <strong>
              Nenhuma posição
              encontrada
            </strong>
          </div>
        )}
      </article>

      <div
        className="portfolio-secondary-metrics"
        style={{
          gridTemplateColumns:
            'repeat(4, minmax(0, 1fr))',
        }}
      >
        <article
          className={`portfolio-metric portfolio-result-card ${classeResultado(
            valorizacaoAtivos,
          )}`}
        >
          <span>
            Valorização dos Ativos
          </span>

          <strong>
            {formatarMoeda(
              valorizacaoAtivos,
            )}
          </strong>
        </article>

        <article
          className={`portfolio-metric portfolio-result-card ${classeResultado(
            totalProventos,
          )}`}
        >
          <span>
            Proventos
          </span>

          <strong>
            {formatarMoeda(
              totalProventos,
            )}
          </strong>
        </article>

        <article
          className={`portfolio-metric portfolio-result-card ${classeResultado(
            premioOpcoes,
          )}`}
        >
          <span>
            Opções
          </span>

          <strong>
            {formatarMoeda(
              premioOpcoes,
            )}
          </strong>
        </article>

        <article
          className={`portfolio-metric portfolio-result-card portfolio-total-result ${classeResultado(
            resultadoTotal,
          )}`}
        >
          <span>
            Resultado Total
          </span>

          <strong>
            {formatarMoeda(
              resultadoTotal,
            )}
          </strong>
        </article>
      </div>
    </section>
  )
}