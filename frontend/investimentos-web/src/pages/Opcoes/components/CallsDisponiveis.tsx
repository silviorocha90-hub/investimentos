import type {
  Dashboard,
  OperacaoOpcao,
  PosicaoAtivo,
} from '../../../types/dashboard'

interface Props {
  posicoes: PosicaoAtivo[]
  opcoes: OperacaoOpcao[]
}

const quantidade = new Intl.NumberFormat('pt-BR', {
  maximumFractionDigits: 2,
})

export function CallsDisponiveis({
  posicoes,
  opcoes,
}: Props) {
  const callsComprometidas = new Map<string, number>()

  opcoes
    .filter(
      (opcao) =>
        opcao.tipoOpcao.trim().toUpperCase() === 'CALL' &&
        opcao.natureza.trim().toUpperCase() === 'VENDA' &&
        (
          opcao.situacao.trim().toUpperCase() === 'ABERTA' ||
          opcao.situacao.trim().toUpperCase() === 'EXECUTADA'
        ),
    )
    .forEach((opcao) => {
      const tickerAtivo =
        opcao.tickerAtivo.trim().toUpperCase()

      callsComprometidas.set(
        tickerAtivo,
        (callsComprometidas.get(tickerAtivo) ?? 0) +
          opcao.quantidade,
      )
    })

  const disponiveis = posicoes
    .filter(
      (posicao) =>
        posicao.quantidade > 0 &&
        posicao.tipoAtivoCodigo === 'ACAO' &&
        posicao.ticker.trim().toUpperCase() !== 'LFTB11' &&
        posicao.ticker.trim().toUpperCase() !== 'FMP ELETROBRAS',
    )
    .map((posicao) => {
      const ticker =
        posicao.ticker.trim().toUpperCase()

      const comprometida =
        callsComprometidas.get(ticker) ?? 0
      const livre = Math.max(
        posicao.quantidade - comprometida,
        0,
      )
      const lotes = Math.floor(livre / 100)

      return {
        ...posicao,
        comprometida,
        livre,
        lotes,
      }
    })
    .sort((a, b) => {
      if (a.livre !== b.livre) {
        return b.livre - a.livre
      }

      return a.ticker.localeCompare(
        b.ticker,
        'pt-BR',
      )
    })

  const totalAcoes = disponiveis.reduce(
    (total, item) => total + item.livre,
    0,
  )
  const totalLotes = disponiveis.reduce(
    (total, item) => total + item.lotes,
    0,
  )

  const maiorPosicao =
    Math.max(
      ...disponiveis.map((item) => item.quantidade),
      1,
    )

  return (
    <article className="panel options-chart-card options-call-ranking-card">
      <header className="options-chart-header">
        <strong>
          Disponível para CALL
        </strong>
      </header>

      {disponiveis.length > 0 ? (
        <div className="options-ranking options-call-ranking">
          {disponiveis.map(
            (item, index) => (
              <div
                className="options-ranking-row"
                key={item.ticker}
                title={`Posição: ${quantidade.format(item.quantidade)} | CALL comprometida: ${quantidade.format(item.comprometida)}`}
              >
                <span className="options-ranking-position">
                  {index + 1}
                </span>

                <strong>
                  {item.ticker}
                </strong>

                <div className="options-ranking-track">
                  <i
                    className={
                      item.livre === 0
                        ? 'negative'
                        : ''
                    }
                    style={{
                      width: `${item.livre > 0
                        ? Math.max(
                            (item.livre / maiorPosicao) * 100,
                            3,
                          )
                        : 0}%`,
                    }}
                  />
                </div>

                <span
                  className={`options-ranking-value ${item.livre === 0 ? 'negative' : ''}`}
                >
                  <span className="options-call-ranking-numbers">
                    <b>{quantidade.format(item.livre)} livres</b>
                    <small>
                      {quantidade.format(item.quantidade)} total
                      <span aria-hidden="true"> · </span>
                      {quantidade.format(item.comprometida)} comprometidas
                      <span aria-hidden="true"> · </span>
                      PM {item.precoMedio.toLocaleString(
                        'pt-BR',
                        {
                          style: 'currency',
                          currency: 'BRL',
                        },
                      )}
                    </small>
                  </span>
                </span>
              </div>
            ),
          )}
        </div>
      ) : (
        <div className="options-chart-empty">
          Nenhum ativo elegível para CALL.
        </div>
      )}
    </article>
  )
}

export function obterPosicoesConsulta(
  investidorAtivo: string,
  carteira: Dashboard | undefined,
  carteiras: ReadonlyArray<{
    nome: string
    dashboard: Dashboard
  }>,
): PosicaoAtivo[] {
  if (investidorAtivo !== 'TOTAL') {
    return carteira?.posicoes ?? []
  }

  const consolidadas = new Map<string, PosicaoAtivo>()

  carteiras.forEach(({ dashboard }) => {
    dashboard.posicoes.forEach((posicao) => {
      const atual = consolidadas.get(posicao.ticker)

      if (!atual) {
        consolidadas.set(posicao.ticker, { ...posicao })
        return
      }

      atual.quantidade += posicao.quantidade
      atual.valorAtual += posicao.valorAtual
      atual.custoTotal += posicao.custoTotal
    })
  })

  return Array.from(consolidadas.values())
}
