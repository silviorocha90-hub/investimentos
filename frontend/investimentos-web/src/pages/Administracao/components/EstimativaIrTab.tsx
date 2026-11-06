import { useEffect, useRef, useState } from 'react'
import './EstimativaIr.css'

interface Item {
  operacaoId: string; investidorId: string; investidor: string; ticker: string
  data: string; tipo: string; categoria: string; quantidade: number; valorOperacao: number
  resultado: number | null; estimativa: number; retido: number; situacao: string; observacao: string
}
interface Resumo {
  investidorId: string; investidor: string; resultado: number; estimativa: number
  retido: number; total: number; pendencias: number
}
interface Relatorio { ano: number; calculadoEmUtc: string; investidores: Resumo[]; operacoes: Item[]; avisos: string[] }
interface Historico { id: string; ano: number; criadoEmUtc: string }
const api = `${import.meta.env.VITE_API_URL ?? 'https://localhost:7237'}/api/admin/estimativa-ir`
const moeda = (v: number) => v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
const data = (v: string) => new Date(v).toLocaleString('pt-BR', { timeZone: 'America/Sao_Paulo' })
async function requisitar<T>(caminho: string, body?: object): Promise<T> {
  const r = await fetch(api + caminho, { credentials: 'include', cache: 'no-store',
    ...(body === undefined ? {} : { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) }) })
  if (!r.ok) {
    const erro = await r.json().catch(() => null) as { detail?: string } | null
    throw new Error(erro?.detail ?? `Não foi possível acessar a estimativa (HTTP ${r.status}).`)
  }
  return r.status === 204 ? undefined as T : r.json()
}
export function EstimativaIrTab() {
  const [ano, setAno] = useState(Number(new Intl.DateTimeFormat('en', { year: 'numeric', timeZone: 'America/Sao_Paulo' }).format(new Date())))
  const [anos, setAnos] = useState<number[]>([])
  const [relatorio, setRelatorio] = useState<Relatorio | null>(null)
  const [historicos, setHistoricos] = useState<Historico[]>([])
  const [versao, setVersao] = useState('')
  const [investidor, setInvestidor] = useState('')
  const [erro, setErro] = useState('')
  const [ocupado, setOcupado] = useState(false)
  const [modal, setModal] = useState<Item | null>(null)
  const [valor, setValor] = useState('')
  const [observacao, setObservacao] = useState('')
  const sequencia = useRef(0)

  useEffect(() => {
    let ativo = true
    void requisitar<number[]>('/anos').then(v => { if (ativo) setAnos(v) }).catch(e => { if (ativo) setErro(String(e.message)) })
    return () => { ativo = false }
  }, [])
  useEffect(() => {
    const id = ++sequencia.current
    setOcupado(true); setErro(''); setRelatorio(null); setVersao('')
    void requisitar<Historico[]>(`/${ano}/historico`).then(async h => {
      if (id !== sequencia.current) return
      setHistoricos(h)
      // Consultas históricas nunca recalculam ou sobrescrevem uma versão salva.
      const vigente = Number(new Intl.DateTimeFormat('en', { year: 'numeric', timeZone: 'America/Sao_Paulo' }).format(new Date()))
      if (ano === vigente || !h.length) {
        const novo = await requisitar<{ id: string; relatorio: Relatorio }>(`/${ano}/atualizar`, {})
        if (id === sequencia.current) {
          setRelatorio(novo.relatorio); setVersao(novo.id)
          setHistoricos(await requisitar<Historico[]>(`/${ano}/historico`))
        }
      } else if (h.length) {
        const r = await requisitar<Relatorio>(`/historico/${h[0].id}`)
        if (id === sequencia.current) { setRelatorio(r); setVersao(h[0].id) }
      }
    }).catch(e => { if (id === sequencia.current) setErro(String(e.message)) })
      .finally(() => { if (id === sequencia.current) setOcupado(false) })
    return () => { sequencia.current++ }
  }, [ano])
  async function atualizar() {
    setOcupado(true); setErro('')
    try {
      const r = await requisitar<{ id: string; relatorio: Relatorio }>(`/${ano}/atualizar`, {})
      setRelatorio(r.relatorio); setVersao(r.id)
      setHistoricos(await requisitar<Historico[]>(`/${ano}/historico`))
    } catch (e) { setErro(e instanceof Error ? e.message : 'Falha ao atualizar.') }
    finally { setOcupado(false) }
  }
  async function selecionarVersao(id: string) {
    setOcupado(true); setErro('')
    try { setRelatorio(await requisitar<Relatorio>(`/historico/${id}`)); setVersao(id) }
    catch (e) { setErro(e instanceof Error ? e.message : 'Falha ao consultar histórico.') }
    finally { setOcupado(false) }
  }
  async function salvarRetencao(event: React.FormEvent) {
    event.preventDefault()
    if (!modal) return
    const numero = Number(valor.replace(',', '.'))
    if (!Number.isFinite(numero) || numero < 0 || !valor.trim()) { setErro('Informe um valor válido.'); return }
    setOcupado(true); setErro('')
    try {
      await requisitar(`/retencao/${modal.operacaoId}`, { valor: numero, observacao })
      setModal(null)
      await atualizar()
    } catch (e) { setErro(e instanceof Error ? e.message : 'Falha ao salvar retenção.') }
    finally { setOcupado(false) }
  }
  const resumos = relatorio?.investidores.filter(x => !investidor || x.investidorId === investidor) ?? []
  const itens = relatorio?.operacoes.filter(x => !investidor || x.investidorId === investidor) ?? []
  return <section className="estimativa-ir">
    <header><div><h2>Estimativa de IR por investidor</h2><p>Acerto entre carteiras • ano-calendário {ano} • declaração {ano + 1}</p></div>
      <button type="button" disabled={ocupado} onClick={() => void atualizar()}>Atualizar e salvar histórico</button></header>
    <p>O valor por investidor é um controle interno para reembolso ao titular do CPF. Retenções definitivas já pagas aparecem separadas do imposto estimado.</p>
    <div className="ir-filtros">
      <label>Ano-calendário<select value={ano} disabled={ocupado} onChange={e => setAno(Number(e.target.value))}>
        {[...new Set([ano, ...anos])].sort((a, b) => b - a).map(a => <option key={a}>{a}</option>)}
      </select></label>
      <label>Investidor<select value={investidor} onChange={e => setInvestidor(e.target.value)}><option value="">Todos</option>
        {relatorio?.investidores.map(x => <option key={x.investidorId} value={x.investidorId}>{x.investidor}</option>)}
      </select></label>
      <label>Histórico salvo<select value={versao} disabled={ocupado || !historicos.length} onChange={e => void selecionarVersao(e.target.value)}>
        {!historicos.length && <option value="">Nenhuma versão salva</option>}
        {historicos.map(h => <option value={h.id} key={h.id}>{data(h.criadoEmUtc)}</option>)}
      </select></label>
    </div>
    {erro && <p role="alert" className="ir-erro">{erro}</p>}
    {ocupado && <p role="status">Processando…</p>}
    {!relatorio && !ocupado && <p>Nenhuma estimativa salva para este ano. Use “Atualizar e salvar histórico” para calcular todas as operações cadastradas. O ano vigente também é atualizado e salvo ao abrir esta aba.</p>}
    {relatorio && <>
      <p>Base salva em {data(relatorio.calculadoEmUtc)}. O ano vigente é atualizado ao abrir esta aba; use atualizar após corrigir operações. Versões anteriores são preservadas.</p>
      <div className="ir-cards">
        <div><span>Estimativa a provisionar</span><strong>{moeda(resumos.reduce((s, x) => s + x.estimativa, 0))}</strong></div>
        <div><span>IR definitivo retido</span><strong>{moeda(resumos.reduce((s, x) => s + x.retido, 0))}</strong></div>
        <div><span>Encargo total das carteiras</span><strong>{moeda(resumos.reduce((s, x) => s + x.total, 0))}</strong></div>
      </div>
      <div className="ir-tabela"><table><thead><tr><th>Investidor</th><th>Resultado realizado</th><th>Estimado</th><th>Retido definitivo</th><th>Encargo total</th><th>Conferir</th></tr></thead>
        <tbody>{resumos.map(x => <tr key={x.investidorId}><td>{x.investidor}</td><td>{moeda(x.resultado)}</td><td>{moeda(x.estimativa)}</td><td>{moeda(x.retido)}</td><td><strong>{moeda(x.total)}</strong></td><td>{x.pendencias ? `${x.pendencias} pendência(s)` : '—'}</td></tr>)}</tbody></table></div>
      <details><summary>Ver analítico por operação ({itens.length})</summary>
        <p>Estimativas positivas são rateadas após compensar as perdas mensais e anteriores da carteira. Compras isoladas não geram encargo. “Pendente” não equivale a isento.</p>
        <div className="ir-tabela"><table><thead><tr><th>Data / investidor</th><th>Operação</th><th>Resultado</th><th>IR estimado</th><th>Retido</th><th>Situação / memória</th></tr></thead>
          <tbody>{itens.map(x => <tr key={x.operacaoId}><td>{x.data.slice(0, 10).split('-').reverse().join('/')}<br />{x.investidor}</td><td><strong>{x.ticker}</strong><br />{x.tipo} • {x.quantidade.toLocaleString('pt-BR')}<br />{moeda(x.valorOperacao)}</td><td>{x.resultado === null ? 'A conferir' : moeda(x.resultado)}</td><td>{moeda(x.estimativa)}</td><td>{moeda(x.retido)}{x.tipo === 'VENDA' && !['ACAO', 'ISENTO', 'FII', 'ETF_RV', 'BDR'].includes(x.categoria) && <button type="button" disabled={ocupado || versao !== historicos[0]?.id} onClick={() => { setModal(x); setValor(String(x.retido)); setObservacao(x.situacao === 'RETIDO CONFIRMADO' ? x.observacao : '') }}>Informar retenção</button>}</td><td><span className={x.situacao === 'PENDENTE' ? 'ir-pendente' : ''}>{x.situacao}</span><br /><small>{x.observacao}</small></td></tr>)}</tbody></table></div>
        {!itens.length && <p>Sem vendas ou opções finalizadas neste ano.</p>}
      </details>
      <details><summary>Critérios e limites da estimativa</summary><ul>{relatorio.avisos.map(x => <li key={x}>{x}</li>)}</ul></details>
    </>}
    {modal && <div className="ir-modal" role="dialog" aria-modal="true" aria-labelledby="ir-modal-titulo"><form onSubmit={e => void salvarRetencao(e)}>
      <h3 id="ir-modal-titulo">IR definitivo retido</h3><p>{modal.investidor} • {modal.ticker} • {modal.quantidade} unidades</p>
      <p>Informe somente o imposto definitivo desta venda, conforme a nota. Não use o IRRF de antecipação nem o débito agregado de outras operações.</p>
      <label>Valor em reais<input required autoFocus inputMode="decimal" value={valor} onChange={e => setValor(e.target.value)} /></label>
      <label>Fonte / observação<textarea required maxLength={1000} value={observacao} onChange={e => setObservacao(e.target.value)} /></label>
      <p>A correção cria um novo registro e uma nova versão anual. O histórico anterior é preservado.</p>
      <button type="button" disabled={ocupado} onClick={() => setModal(null)}>Cancelar</button> <button disabled={ocupado}>Salvar retenção</button>
    </form></div>}
  </section>
}
