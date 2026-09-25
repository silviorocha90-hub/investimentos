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
        posicao.ticker.trim().toUpperCase() !== 'LFTB11',
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

  return (
    <article className="options-call-panel">
      <div className="options-call-heading">
        <div>
          <strong>Disponível para CALL</strong>
          <span>
            Ações livres para venda coberta, em lotes de 100
          </span>
        </div>

        <div className="options-call-totals">
          <span>
            <b>{disponiveis.length}</b>
            ativos em carteira
          </span>
          <span>
            <b>{quantidade.format(totalAcoes)}</b>
            ações livres
          </span>
          <span>
            <b>{totalLotes}</b>
            lotes
          </span>
        </div>
      </div>

      {disponiveis.length > 0 ? (
        <div className="options-call-grid">
          {disponiveis.map((item) => (
            <div
              className="options-call-card"
              key={item.ticker}
            >
              <div className="options-call-card-head">
                <strong>{item.ticker}</strong>
                <span>
                  {item.lotes > 0
                    ? `${item.lotes} lote${item.lotes !== 1 ? 's' : ''}`
                    : 'sem lote livre'}
                </span>
              </div>

              <div className="options-call-card-value">
                {quantidade.format(item.livre)}
                <small>ações disponíveis</small>
              </div>

              <div className="options-call-card-detail">
                <span>
                  Posição
                  <b>{quantidade.format(item.quantidade)}</b>
                </span>
                <span>
                  Em CALL comprometida
                  <b>{quantidade.format(item.comprometida)}</b>
                </span>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="options-call-empty">
          Nenhum ativo possui lote completo disponível para CALL.
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
