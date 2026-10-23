import { useMemo, useState } from 'react'
import type { Dashboard } from '../../types/dashboard'
import './CalendarioDividendos.css'

const MESES = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro']

export function CalendarioDividendos({ dashboard }: {
  dashboard: Dashboard | null
  tickers: { ticker: string; nome: string }[]
  investidoresIds: string[]
}) {
  const anos = useMemo(() => [...new Set((dashboard?.proventos ?? [])
    .map(p => Number(p.dataPagamento?.slice(0, 4)))
    .filter(a => Number.isInteger(a) && a >= 2000 && a <= 2100))]
    .sort((a, b) => b - a), [dashboard])
  const [anoEscolhido, setAnoEscolhido] = useState<number | null>(null)
  const ano = anoEscolhido !== null && anos.includes(anoEscolhido)
    ? anoEscolhido : (anos[0] ?? new Date().getFullYear())

  const recebimentos = useMemo(() => {
    const porMes = Array.from({ length: 12 }, () => new Map<string, number>())
    for (const p of dashboard?.proventos ?? []) {
      const ticker = p.ticker?.trim().toUpperCase()
      if (!ticker) continue
      const data = /^(\\d{4})-(\\d{2})-(\\d{2})/.exec(p.dataPagamento ?? '')
      if (!data || Number(data[1]) !== ano) continue
      const mes = Number(data[2]) - 1
      if (mes < 0 || mes > 11) continue
      const valor = Number(p.valorLiquido)
      if (!Number.isFinite(valor)) continue
      porMes[mes].set(ticker, (porMes[mes].get(ticker) ?? 0) + valor)
    }
    return porMes
  }, [dashboard, ano])

  const totalAno = recebimentos.reduce((total, mes) =>
    total + [...mes.values()].reduce((soma, valor) => soma + valor, 0), 0)
  const moeda = (valor: number) => valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })

  return (
    <section className="estrategia-panel calendario-dividendos" aria-label="Calendário de dividendos">
      <div className="calendario-cabecalho">
        <div>
          <h2>Calendário de dividendos</h2>
          <p>Somente pagamentos registrados: rendimentos, dividendos e juros sobre capital próprio (JCP), inclusive de empresas sem posição atual.</p>
        </div>
        <div className="calendario-controles">
          <label>Ano
            <select value={ano} onChange={e => setAnoEscolhido(Number(e.target.value))} disabled={!anos.length}>
              {anos.length ? anos.map(a => <option value={a} key={a}>{a}</option>)
                : <option value={ano}>{ano}</option>}
            </select>
          </label>
        </div>
      </div>
      <p>Total registrado no ano: <strong>{moeda(totalAno)}</strong></p>
      <div className="calendario-meses">
        {MESES.map((mes, i) => {
          const empresas = [...recebimentos[i].entries()].sort(([a], [b]) => a.localeCompare(b))
          return (
            <div className="calendario-mes" key={mes}>
              <h3>{mes}</h3>
              <div className="calendario-empresas">
                {empresas.map(([ticker, valor]) => (
                  <div key={ticker} className="calendario-empresa" title={ticker}>
                    <strong>{ticker}</strong>
                    <small>{moeda(valor)}</small>
                  </div>
                ))}
                {!empresas.length && <span className="calendario-vazio">Nenhum pagamento registrado</span>}
              </div>
            </div>
          )
        })}
      </div>
      <p>Valores líquidos agrupados por empresa e mês de pagamento, conforme os proventos registrados na carteira. Não são previsões ou anúncios futuros.</p>
    </section>
  )
}
