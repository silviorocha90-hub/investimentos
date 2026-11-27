import { useEffect, useRef, useState } from 'react'
import { Modal } from '../../../components/Modal'
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
  return (
    <section className="estimativa-ir" aria-label="Estimativa de IR por investidor">
      <article className="panel admin-panel">
        <header className="admin-toolbar admin-assets-toolbar ir-toolbar">
          <div className="ir-heading">
            <strong>Estimativa de IR por investidor</strong>
            <span>Ano-calendário {ano} · Declaração {ano + 1}</span>
          </div>
          <button className="admin-primary-button" type="button" disabled={ocupado} onClick={() => void atualizar()}>
            {ocupado ? 'Processando…' : 'Atualizar e salvar histórico'}
          </button>
        </header>

        <div className="admin-asset-filters ir-filtros">
          <label>
            <span>Ano-calendário</span>
            <select value={ano} disabled={ocupado} onChange={e => setAno(Number(e.target.value))}>
              {[...new Set([ano, ...anos])].sort((a, b) => b - a).map(a => <option key={a}>{a}</option>)}
            </select>
          </label>
          <label>
            <span>Investidor</span>
            <select value={investidor} onChange={e => setInvestidor(e.target.value)}>
              <option value="">Todos</option>
              {relatorio?.investidores.map(x => <option key={x.investidorId} value={x.investidorId}>{x.investidor}</option>)}
            </select>
          </label>
          <label>
            <span>Histórico salvo</span>
            <select value={versao} disabled={ocupado || !historicos.length} onChange={e => void selecionarVersao(e.target.value)}>
              {!historicos.length && <option value="">Nenhuma versão salva</option>}
              {historicos.map(h => <option value={h.id} key={h.id}>{data(h.criadoEmUtc)}</option>)}
            </select>
          </label>
        </div>
        <div className="ir-contexto">
          <p>Controle interno de reembolso ao titular do CPF. Retenções definitivas já pagas aparecem separadas do imposto estimado.</p>
          {relatorio && <p>Base salva em {data(relatorio.calculadoEmUtc)}. O ano vigente é atualizado ao abrir esta aba. Versões anteriores são preservadas.</p>}
        </div>
      </article>

      {erro && !modal && <div role="alert" className="admin-error ir-message">{erro}</div>}
      {ocupado && <div role="status" className="ir-message">Processando estimativa…</div>}
      {!relatorio && !ocupado && <div className="panel ir-empty">Nenhuma estimativa salva para este ano. Use “Atualizar e salvar histórico” para calcular as operações cadastradas.</div>}

      {relatorio && <>
        <div className="ir-cards">
          <article className="panel ir-card">
            <span>Estimativa a provisionar</span>
            <strong>{moeda(resumos.reduce((s, x) => s + x.estimativa, 0))}</strong>
          </article>
          <article className="panel ir-card">
            <span>IR definitivo retido</span>
            <strong>{moeda(resumos.reduce((s, x) => s + x.retido, 0))}</strong>
          </article>
          <article className="panel ir-card ir-card-total">
            <span>Encargo total das carteiras</span>
            <strong>{moeda(resumos.reduce((s, x) => s + x.total, 0))}</strong>
          </article>
        </div>

        <article className="panel admin-panel">
          <div className="admin-toolbar admin-assets-toolbar">
            <strong>Consolidado por investidor</strong>
            <span className="ir-meta">{resumos.length} investidor(es)</span>
          </div>
          <div className="admin-table-wrap ir-tabela">
            <table className="admin-table ir-resumo-table" aria-label="Consolidado de IR por investidor">
              <thead><tr><th scope="col">Investidor</th><th scope="col" className="ir-number">Resultado realizado</th><th scope="col" className="ir-number">Estimado</th><th scope="col" className="ir-number">Retido definitivo</th><th scope="col" className="ir-number">Encargo total</th><th scope="col">Conferir</th></tr></thead>
              <tbody>{resumos.map(x => (
                <tr key={x.investidorId}>
                  <td><strong>{x.investidor}</strong></td>
                  <td className={`ir-number ${x.resultado < 0 ? 'negative' : 'positive'}`}>{moeda(x.resultado)}</td>
                  <td className="ir-number">{moeda(x.estimativa)}</td>
                  <td className="ir-number">{moeda(x.retido)}</td>
                  <td className="ir-number"><strong>{moeda(x.total)}</strong></td>
                  <td>{x.pendencias ? <span className="ir-pendente">{x.pendencias} pendência(s)</span> : '—'}</td>
                </tr>
              ))}</tbody>
            </table>
          </div>
          {!resumos.length && <p className="ir-empty">Nenhum investidor neste ano.</p>}
        </article>

        <details className="panel admin-panel ir-details">
          <summary><span>Analítico por operação</span><span className="ir-meta">{itens.length} registro(s)</span></summary>
          <p className="ir-note">Estimativas positivas são rateadas após compensar perdas mensais e anteriores da carteira. Compras isoladas não geram encargo. “Pendente” não equivale a isento.</p>
          <div className="admin-table-wrap ir-tabela">
            <table className="admin-table ir-analitico-table" aria-label="Analítico de IR por operação">
              <thead><tr><th scope="col">Data / investidor</th><th scope="col">Operação</th><th scope="col" className="ir-number">Resultado</th><th scope="col" className="ir-number">IR estimado</th><th scope="col" className="ir-number">Retido</th><th scope="col">Situação / memória</th></tr></thead>
              <tbody>{itens.map(x => (
                <tr key={x.operacaoId}>
                  <td><div className="ir-cell-stack"><span>{x.data.slice(0, 10).split('-').reverse().join('/')}</span><small>{x.investidor}</small></div></td>
                  <td><div className="ir-cell-stack"><strong>{x.ticker}</strong><small>{x.tipo} · {x.quantidade.toLocaleString('pt-BR')}</small><span>{moeda(x.valorOperacao)}</span></div></td>
                  <td className={`ir-number ${x.resultado !== null && x.resultado < 0 ? 'negative' : ''}`}>{x.resultado === null ? 'A conferir' : moeda(x.resultado)}</td>
                  <td className="ir-number">{moeda(x.estimativa)}</td>
                  <td className="ir-number">
                    <div className="ir-cell-stack ir-retencao">
                      <span>{moeda(x.retido)}</span>
                      {x.tipo === 'VENDA' && !['ACAO', 'ISENTO', 'FII', 'ETF_RV', 'BDR'].includes(x.categoria) && (
                        <button className="admin-action" type="button" disabled={ocupado || versao !== historicos[0]?.id} onClick={() => { setModal(x); setValor(String(x.retido)); setObservacao(x.situacao === 'RETIDO CONFIRMADO' ? x.observacao : '') }}>Informar retenção</button>
                      )}
                    </div>
                  </td>
                  <td><div className="ir-cell-stack"><span className={x.situacao === 'PENDENTE' ? 'ir-pendente' : ''}>{x.situacao}</span><small>{x.observacao}</small></div></td>
                </tr>
              ))}</tbody>
            </table>
          </div>
          {!itens.length && <p className="ir-empty">Sem vendas ou opções finalizadas neste ano.</p>}
        </details>

        <details className="panel admin-panel ir-details">
          <summary><span>Critérios e limites da estimativa</span></summary>
          <ul className="ir-criterios">{relatorio.avisos.map(x => <li key={x}>{x}</li>)}</ul>
        </details>
      </>}

      {modal && (
        <Modal title="IR definitivo retido" subtitle={`${modal.investidor} · ${modal.ticker} · ${modal.quantidade} unidades`} onClose={() => setModal(null)} closeDisabled={ocupado} className="ir-retencao-modal">
          <form onSubmit={e => void salvarRetencao(e)}>
            <p className="ir-note">Informe somente o imposto definitivo desta venda, conforme a nota. Não use o IRRF de antecipação nem o débito agregado de outras operações.</p>
            <div className="admin-modal-grid">
              <label className="ir-wide"><span>Valor em reais</span><input required autoFocus inputMode="decimal" value={valor} onChange={e => setValor(e.target.value)} /></label>
              <label className="ir-wide"><span>Fonte / observação</span><textarea required maxLength={1000} value={observacao} onChange={e => setObservacao(e.target.value)} /></label>
            </div>
            <p className="ir-note">A correção cria um novo registro e uma nova versão anual. O histórico anterior é preservado.</p>
            {erro && <p role="alert" className="admin-error ir-note">{erro}</p>}
            <div className="admin-modal-actions">
              <button className="admin-clear-button" type="button" disabled={ocupado} onClick={() => setModal(null)}>Cancelar</button>
              <button className="admin-primary-button" type="submit" disabled={ocupado}>Salvar retenção</button>
            </div>
          </form>
        </Modal>
      )}
    </section>
  )
}
