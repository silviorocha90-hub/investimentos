import {
  formatarMoeda,
} from '../../../utils/formatters'

interface ProventosResumoProps {
  totalProventos: number
  totalMes: number
  mediaMensal: number
  maiorPagador?: {
    ticker: string
    valor: number
  }
}

export function ProventosResumo({
  totalProventos,
  totalMes,
  mediaMensal,
  maiorPagador,
}: ProventosResumoProps) {
  return (
    <div className="proventos-resumo">
      <article className="provento-kpi provento-kpi-destaque">
        <span>
          Proventos
        </span>

        <strong>
          {formatarMoeda(
            totalProventos,
          )}
        </strong>

        <small>
          Total recebido acumulado
        </small>
      </article>

      <article className="provento-kpi">
        <span>
          Proventos no mês
        </span>

        <strong>
          {formatarMoeda(
            totalMes,
          )}
        </strong>

        <small>
          Mês atual
        </small>
      </article>

      <article className="provento-kpi">
        <span>
          Média mensal
        </span>

        <strong>
          {formatarMoeda(
            mediaMensal,
          )}
        </strong>

        <small>
          Média do ano
        </small>
      </article>

      <article className="provento-kpi">
        <span>
          Maior pagador
        </span>

        <strong>
          {maiorPagador
            ?.ticker ??
            '—'}
        </strong>

        <small>
          {maiorPagador
            ? formatarMoeda(
                maiorPagador.valor,
              )
            : 'Sem proventos'}
        </small>
      </article>
    </div>
  )
}