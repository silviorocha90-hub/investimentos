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
      if (a.quantidade !== b.quantidade) {
        return b.quantidade - a.quantidade
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

  return (
    <article className="panel options-call-clean">
      <header className="options-call-clean-header">
        <div>
          <strong>Disponível para CALL</strong>
          <span>Posição e cobertura por ativo</span>
        </div>

        <div className="options-call-clean-summary">
          <b>{quantidade.format(totalAcoes)}</b>
          <span>ações livres · {totalLotes} lotes</span>
        </div>
      </header>

      {disponiveis.length > 0 ? (
        <div className="options-call-clean-list">
          {disponiveis.map((item) => {
            const percentualLivre =
              item.quantidade > 0
                ? Math.max(
                    Math.min(
                      (item.livre / item.quantidade) * 100,
                      100,
                    ),
                    0,
                  )
                : 0

            return (
              <div
                className="options-call-clean-row"
                key={item.ticker}
              >
                <strong className="options-call-clean-ticker">
                  {item.ticker}
                </strong>

                <div className="options-call-clean-position">
                  <div className="options-call-clean-bar">
                    <i
                      style={{
                        width: `${percentualLivre}%`,
                      }}
                    />
                  </div>

                  <span>
                    {quantidade.format(item.quantidade)} ações
                    <small>
                      PM {item.precoMedio.toLocaleString(
                        'pt-BR',
                        {
                          style: 'currency',
                          currency: 'BRL',
                        },
                      )}
                    </small>
                  </span>
                </div>

                <div className="options-call-clean-metric">
                  <span>Comprometidas</span>
                  <b>{quantidade.format(item.comprometida)}</b>
                </div>

                <div
                  className={`options-call-clean-metric options-call-clean-free ${item.livre === 0 ? 'empty' : ''}`}
                >
                  <span>Disponíveis</span>
                  <b>{quantidade.format(item.livre)}</b>
                  <small>{item.lotes} lote{item.lotes !== 1 ? 's' : ''}</small>
                </div>
              </div>
            )
          })}
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
