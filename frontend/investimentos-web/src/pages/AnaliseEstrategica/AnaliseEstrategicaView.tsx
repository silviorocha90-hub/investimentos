import { useMemo, useState } from 'react'
import type { Dashboard, PosicaoAtivo } from '../../types/dashboard'

const brl = (n: number) => n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
const pct = (n: number) => n.toLocaleString('pt-BR', { maximumFractionDigits: 1 }) + '%'
const isAcao = (p: PosicaoAtivo) => /^(ACAO|AÇÃO|ACOES|AÇÕES)$/i.test(p.tipoAtivoCodigo) || /^[A-Z]{4}[3-6]$/.test(p.ticker)

export function AnaliseEstrategicaView({ dashboard }: { dashboard: Dashboard | null }) {
  const [desconto, setDesconto] = useState(10)
  const [busca, setBusca] = useState('')
  const [selecionado, setSelecionado] = useState<string | null>(null)
  const posicoes = useMemo(() => {
    if (!dashboard) return []
    const total = dashboard.posicoes.filter(p => p.quantidade > 0 && isAcao(p)).reduce((s, p) => s + p.valorAtual, 0)
    return dashboard.posicoes.filter(p => p.quantidade > 0 && isAcao(p)).map(p => ({
      ...p,
      peso: total > 0 ? p.valorAtual / total * 100 : 0,
      distanciaPm: p.precoAtual && p.precoMedio > 0 ? (p.precoAtual / p.precoMedio - 1) * 100 : null,
      puts: dashboard.opcoes.filter(o => o.tickerAtivo?.toUpperCase() === p.ticker.toUpperCase() && o.tipoOpcao?.toUpperCase() === 'PUT'),
    })).sort((a, b) => (a.distanciaPm ?? 999) - (b.distanciaPm ?? 999))
  }, [dashboard])
  const filtradas = posicoes.filter(p => (p.ticker + ' ' + p.nome).toLowerCase().includes(busca.toLowerCase()))
  const atual = posicoes.find(p => p.ticker === selecionado)
  return (
    <section style={{ padding: '24px', maxWidth: 1400, margin: '0 auto' }}>
      <div style={{ marginBottom: 24 }}>
        <small style={{ color: '#55d8bc', letterSpacing: 2 }}>INTELIGÊNCIA DE INVESTIMENTOS</small>
        <h1>Análise Estratégica da Carteira</h1>
        <p>Visão consolidada de todos os investidores. Apenas ativos existentes, sem separar titularidade.</p>
      </div>
      <div style={{ padding: 16, border: '1px solid #497a75', borderRadius: 12, marginBottom: 18 }}>
        <strong>Simulação educacional — não é recomendação automática de compra.</strong>
        <p>Os strikes abaixo são preços-alvo teóricos calculados por desconto sobre a última cotação cadastrada, não contratos negociáveis confirmados. Prêmios, delta, liquidez e vencimentos precisam ser consultados no book antes de enviar ordens. Cotações podem estar desatualizadas.</p>
      </div>
      <div style={{ display: 'flex', gap: 16, flexWrap: 'wrap', marginBottom: 20 }}>
        <label>Desconto-alvo para PUT (%)
          <select value={desconto} onChange={e => setDesconto(Number(e.target.value))} style={{ display: 'block', padding: 9 }}>
            {[5, 10, 15, 20].map(n => <option key={n} value={n}>{n}%</option>)}
          </select>
        </label>
        <label>Pesquisar ativo
          <input value={busca} onChange={e => setBusca(e.target.value)} placeholder="Ticker ou empresa" style={{ display: 'block', padding: 9 }} />
        </label>
      </div>
      {!dashboard ? <p>Carregando carteira consolidada ou dados indisponíveis.</p> : filtradas.length === 0 ? <p>Nenhuma ação encontrada na carteira consolidada.</p> :
      <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
          <thead><tr>{['Ativo', 'Quantidade', 'Preço médio', 'Cotação cadastrada', 'Variação vs. PM', 'Peso em ações', 'PUT: strike teórico', 'PUTs históricas'].map(t => <th key={t} style={{ padding: 12, borderBottom: '1px solid #49605c' }}>{t}</th>)}</tr></thead>
          <tbody>{filtradas.map(p => <tr key={p.ticker} onClick={() => setSelecionado(p.ticker === selecionado ? null : p.ticker)} style={{ cursor: 'pointer', background: selecionado === p.ticker ? 'rgba(80,210,170,.12)' : undefined }}>
            <td style={{ padding: 12 }}>{p.ticker}<div style={{ opacity: .7, fontSize: 12 }}>{p.nome}</div></td>
            <td>{p.quantidade.toLocaleString('pt-BR')}</td><td>{brl(p.precoMedio)}</td>
            <td>{p.precoAtual != null && p.precoAtual > 0 ? brl(p.precoAtual) : 'Sem cotação'}</td>
            <td>{p.distanciaPm == null ? '—' : pct(p.distanciaPm)}</td><td>{pct(p.peso)}</td>
            <td>{p.precoAtual != null && p.precoAtual > 0 ? brl(p.precoAtual * (1 - desconto / 100)) : 'Indisponível'}</td>
            <td>{p.puts.length}</td>
          </tr>)}</tbody>
        </table>
      </div>}
      {atual && <div style={{ marginTop: 24, padding: 20, border: '1px solid #497a75', borderRadius: 12 }}>
        <h2>{atual.ticker} — estudo de entrada</h2>
        <p><strong>Compra a mercado:</strong> {atual.precoAtual != null && atual.precoAtual > 0 ? brl(atual.precoAtual) : 'Cotação indisponível'}. Compare o preço com o PM consolidado ({brl(atual.precoMedio)}) e o peso atual ({pct(atual.peso)}). Estar abaixo do PM não significa necessariamente estar barato.</p>
        <p><strong>PUT (simulação):</strong> {atual.precoAtual != null && atual.precoAtual > 0 ? brl(atual.precoAtual * (1 - desconto / 100)) : 'Sem cotação'} com desconto de {desconto}%. Se exercida, o custo por ação será strike menos prêmio efetivamente recebido, antes de custos e impostos.</p>
        <p><strong>Histórico de PUTs:</strong> {atual.puts.length} operações registradas. {atual.puts.filter(o => o.estaAtiva).length} indicadas como ativas. Não representa ofertas disponíveis no mercado.</p>
        {atual.puts.length > 0 && <div style={{ overflowX: 'auto' }}><table style={{ width: '100%', textAlign: 'left' }}><thead><tr><th>Opção</th><th>Strike histórico</th><th>Prêmio</th><th>Vencimento</th><th>Situação</th></tr></thead><tbody>{[...atual.puts].sort((a,b) => b.dataOperacao.localeCompare(a.dataOperacao)).slice(0, 8).map(o => <tr key={o.id}><td>{o.tickerOpcao}</td><td>{brl(o.strike)}</td><td>{brl(o.premioUnitario)}</td><td>{new Date(o.vencimento).toLocaleDateString('pt-BR')}</td><td>{o.situacao}</td></tr>)}</tbody></table></div>}
        <p style={{ opacity: .75 }}>Antes de operar: conferir valuation, tendência de resultados, dividendos, concentração, garantias, liquidez, spread, delta e risco de exercício. A análise não envia ordens.</p>
      </div>}
    </section>
  )
}
