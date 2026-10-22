import { useEffect, useMemo, useState } from 'react'
import type { Dashboard } from '../../types/dashboard'
import './CalendarioDividendos.css'

const MESES = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro']
type MesesPlanejados = Record<string, number[]>

function lerPlanejamento(chave: string): MesesPlanejados {
  try {
    const valor = window.localStorage.getItem(chave)
    const objeto: unknown = valor ? JSON.parse(valor) : {}
    if (!objeto || typeof objeto !== 'object' || Array.isArray(objeto)) return {}
    return Object.fromEntries(Object.entries(objeto).filter(([, meses]) =>
      Array.isArray(meses)).map(([ticker, meses]) => [ticker,
      (meses as unknown[]).filter((m): m is number =>
        typeof m === 'number' && Number.isInteger(m) && m >= 0 && m < 12)]))
  } catch { return {} }
}

export function CalendarioDividendos({ dashboard, tickers, investidoresIds }: {
  dashboard: Dashboard | null
  tickers: { ticker: string; nome: string }[]
  investidoresIds: string[]
}) {
  const chave = 'aportiva:calendario-dividendos:v1:' + [...investidoresIds].sort().join(',')
  const [planejamento, setPlanejamento] = useState<MesesPlanejados>(() => lerPlanejamento(chave))
  const [selecionado, setSelecionado] = useState<string>('')
  const [salvando, setSalvando] = useState(false)
  const [erroSalvar, setErroSalvar] = useState<string | null>(null)
  const [carregandoServidor, setCarregandoServidor] = useState(true)
  const [ano, setAno] = useState(new Date().getFullYear())
  const [modo, setModo] = useState<'planejado' | 'recebido'>('planejado')

  const api = import.meta.env.VITE_API_URL ?? 'https://localhost:7237'
  const ids = useMemo(() => [...new Set(investidoresIds)].filter(Boolean).sort(), [investidoresIds.join(',')])
  useEffect(() => {
    let ativo = true
    setSelecionado('')
    setCarregandoServidor(true)
    setErroSalvar(null)
    if (!ids.length) { setPlanejamento({}); setCarregandoServidor(false); return }
    void Promise.all(ids.map(async id => {
      const response = await fetch(`${api}/api/analise-estrategica/calendario-dividendos/${encodeURIComponent(id)}`, { credentials: 'include' })
      if (!response.ok) throw new Error(`Falha ao carregar calendário: HTTP ${response.status}`)
      return await response.json() as MesesPlanejados
    })).then(resultados => {
      if (!ativo) return
      const combinado: MesesPlanejados = {}
      for (const resultado of resultados) for (const [ticker, meses] of Object.entries(resultado)) {
        combinado[ticker] = [...new Set([...(combinado[ticker] ?? []), ...meses])].sort((a, b) => a - b)
      }
      setPlanejamento(combinado)
      setCarregandoServidor(false)
    }).catch(e => {
      if (!ativo) return
      setPlanejamento(lerPlanejamento(chave))
      setErroSalvar('Não foi possível consultar o calendário no servidor. Alterações desabilitadas até a conexão voltar. ' + (e instanceof Error ? e.message : ''))
      setCarregandoServidor(false)
    })
    return () => { ativo = false }
  }, [chave, api])

  const empresas = useMemo(() => [...tickers].sort((a, b) =>
    a.ticker.localeCompare(b.ticker)), [tickers])
  const elegiveis = useMemo(() => new Set(empresas.map(e => e.ticker.toUpperCase())), [empresas])
  const recebimentos = useMemo(() => {
    const porMes = Array.from({ length: 12 }, () => new Map<string, number>())
    for (const p of dashboard?.proventos ?? []) {
      const ticker = p.ticker.toUpperCase()
      if (!elegiveis.has(ticker)) continue
      const data = /^\d{4}-(\d{2})-(\d{2})/.exec(p.dataPagamento)
      if (!data || Number(p.dataPagamento.slice(0, 4)) !== ano) continue
      const mes = Number(data[1]) - 1
      if (mes < 0 || mes > 11) continue
      porMes[mes].set(ticker, (porMes[mes].get(ticker) ?? 0) + p.valorLiquido)
    }
    return porMes
  }, [dashboard, elegiveis, ano])

  const mesesHistoricos = useMemo(() => {
    const mapa = new Map<string, Set<number>>()
    for (const p of dashboard?.proventos ?? []) {
      const ticker = p.ticker.toUpperCase()
      if (!elegiveis.has(ticker)) continue
      const data = /^(\d{4})-(\d{2})-(\d{2})/.exec(p.dataPagamento)
      if (!data) continue
      const mes = Number(data[1]) - 1
      if (mes < 0 || mes > 11) continue
      if (!mapa.has(ticker)) mapa.set(ticker, new Set())
      mapa.get(ticker)?.add(mes)
    }
    return mapa
  }, [dashboard, elegiveis])

  async function salvar(ticker: string, meses: number[]) {
    if (salvando || carregandoServidor || erroSalvar || !ids.length) return
    setSalvando(true)
    setErroSalvar(null)
    try {
      await Promise.all(ids.map(async id => {
        const response = await fetch(`${api}/api/analise-estrategica/calendario-dividendos/${encodeURIComponent(id)}/${encodeURIComponent(ticker)}`, {
          method: 'PUT', credentials: 'include',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ meses }),
        })
        if (!response.ok) throw new Error(`Erro HTTP ${response.status}`)
      }))
      const proximo = { ...planejamento, [ticker]: meses }
      setPlanejamento(proximo)
      try { window.localStorage.setItem(chave, JSON.stringify(proximo)) } catch { /* backup opcional */ }
    } catch (e) {
      setErroSalvar('Falha ao salvar no servidor. Recarregue para sincronizar antes de editar novamente. ' + (e instanceof Error ? e.message : ''))
    } finally { setSalvando(false) }
  }

  function usarHistorico(ticker: string) {
    const meses = [...(mesesHistoricos.get(ticker) ?? new Set<number>())].sort((a, b) => a - b)
    if (meses.length) void salvar(ticker, meses)
  }

  const porMes = MESES.map((_, mes) => modo === 'planejado'
    ? empresas.filter(e => planejamento[e.ticker]?.includes(mes)).map(e => e.ticker)
    : [...recebimentos[mes].keys()].sort())

  function alternarMes(ticker: string, mes: number) {
    const atuais = planejamento[ticker] ?? []
    const novos = atuais.includes(mes) ? atuais.filter(m => m !== mes) : [...atuais, mes].sort((a, b) => a - b)
    void salvar(ticker, novos)
  }

  return (
    <section className="estrategia-panel calendario-dividendos" aria-label="Calendário de dividendos">
      <div className="calendario-cabecalho">
        <div>
          <h2>Calendário de dividendos</h2>
          <p>Organize os meses de pagamento das empresas com posição na carteira.</p>
        </div>
        <div className="calendario-controles">
          <label>Visualização
            <select value={modo} onChange={e => setModo(e.target.value as 'planejado' | 'recebido')}>
              <option value="planejado">Meu planejamento</option>
              <option value="recebido">Pagamentos registrados</option>
            </select>
          </label>
          {modo === 'recebido' && <label>Ano
            <select value={ano} onChange={e => setAno(Number(e.target.value))}>
              {Array.from({ length: 8 }, (_, i) => new Date().getFullYear() - i).map(a =>
                <option value={a} key={a}>{a}</option>)}
            </select>
          </label>}
        </div>
      </div>
      {carregandoServidor && <p role="status">Carregando calendário salvo no servidor...</p>}
      {salvando && <p role="status">Salvando no Azure SQL...</p>}
      {erroSalvar && <p role="alert" className="estrategia-error">{erroSalvar}</p>}
      <div className="calendario-meses">
        {MESES.map((mes, i) => (
          <div className="calendario-mes" key={mes}>
            <h3>{mes}</h3>
            <div className="calendario-empresas">
              {porMes[i].map(ticker => (
                <button key={ticker} type="button" className="calendario-empresa"
                  title={empresas.find(e => e.ticker === ticker)?.nome ?? ticker}
                  onClick={() => { setSelecionado(ticker); setModo('planejado') }}>
                  <strong>{ticker}</strong>
                  {modo === 'recebido' && <small>{recebimentos[i].get(ticker)?.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</small>}
                </button>
              ))}
              {!porMes[i].length && <span className="calendario-vazio">Nenhuma empresa</span>}
            </div>
          </div>
        ))}
      </div>
      <div className="calendario-editor">
        <label>Preencher meses da empresa
          <select value={selecionado} onChange={e => setSelecionado(e.target.value)}>
            <option value="">Selecione uma empresa</option>
            {empresas.map(e => <option value={e.ticker} key={e.ticker}>{e.ticker} — {e.nome}</option>)}
          </select>
        </label>
        {selecionado && (mesesHistoricos.get(selecionado)?.size ?? 0) > 0 && <button type="button" className="calendario-importar" disabled={salvando || carregandoServidor || !!erroSalvar} onClick={() => usarHistorico(selecionado)}>Preencher com meses observados no histórico ({mesesHistoricos.get(selecionado)?.size})</button>}
        {selecionado && <div className="calendario-checkboxes">
          {MESES.map((mes, i) => <label key={mes}>
            <input type="checkbox" disabled={salvando || carregandoServidor || !!erroSalvar} checked={planejamento[selecionado]?.includes(i) ?? false}
              onChange={() => alternarMes(selecionado, i)} />
            {mes.slice(0, 3).toUpperCase()}
          </label>)}
        </div>}
        <p>Os meses planejados são preenchidos por você ou copiados do histórico, mediante confirmação; não representam anúncio de dividendos. Os pagamentos registrados usam a data de pagamento e os proventos disponíveis na carteira. O planejamento é salvo no Azure SQL, separado por investidor, e sincronizado entre dispositivos. Uma cópia local serve apenas como backup.</p>
      </div>
    </section>
  )
}
