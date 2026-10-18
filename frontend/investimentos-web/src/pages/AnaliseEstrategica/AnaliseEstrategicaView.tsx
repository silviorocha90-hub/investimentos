import { useEffect, useMemo, useState } from 'react'
import { obterOperacoes } from '../../api/dashboardApi'
import type { Dashboard, OperacaoCarteira, OperacaoOpcao, PosicaoAtivo } from '../../types/dashboard'

const moeda = (n: number) => n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
const percentual = (n: number) => n.toLocaleString('pt-BR', { maximumFractionDigits: 1 }) + '%'
const acao = (p: PosicaoAtivo) => /^(ACAO|AÇÃO|ACOES|AÇÕES)$/i.test(p.tipoAtivoCodigo) || /^[A-Z]{4}[3-6]$/.test(p.ticker)
const quantil = (valores: number[], q: number): number | null => {
  const ordenados = valores.filter(n => Number.isFinite(n) && n > 0).sort((a,b) => a-b)
  if (!ordenados.length) return null
  const i = (ordenados.length - 1) * q
  const baixo = Math.floor(i)
  return ordenados[baixo] + (ordenados[Math.ceil(i)] - ordenados[baixo]) * (i - baixo)
}
const operacaoTipo = (op: OperacaoCarteira, tipo: string) => op.tipoOperacao.toUpperCase().includes(tipo)
const opcaoTipo = (op: OperacaoOpcao, tipo: string) => op.tipoOpcao.toUpperCase() === tipo
const alvoHistorico = (opcoes: OperacaoOpcao[], tipo: string, referencia: number | null, direcao: 'abaixo' | 'acima') => {
  if (referencia === null) return null
  const strikes = opcoes.filter(o => opcaoTipo(o, tipo) && o.strike > 0 && (direcao === 'abaixo' ? o.strike <= referencia : o.strike >= referencia)).map(o => o.strike)
  return strikes.length ? (direcao === 'abaixo' ? Math.max(...strikes) : Math.min(...strikes)) : null
}

export function AnaliseEstrategicaView({ dashboard, investidoresIds }: { dashboard: Dashboard | null; investidoresIds: string[] }) {
  const [historico, setHistorico] = useState<OperacaoCarteira[]>([])
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [busca, setBusca] = useState('')
  const [selecionado, setSelecionado] = useState<string | null>(null)
  const chave = [...investidoresIds].sort().join(',')

  useEffect(() => {
    let ativo = true
    setCarregando(true)
    setErro(null)
    Promise.all(chave.split(',').filter(Boolean).map(id => obterOperacoes(id)))
      .then(grupos => {
        if (ativo) setHistorico([...new Map(grupos.flat().map(o => [o.id, o])).values()].sort((a,b) => a.data.localeCompare(b.data) || a.sequencia - b.sequencia))
      })
      .catch(e => { if (ativo) { setHistorico([]); setErro(e instanceof Error ? e.message : 'Erro ao carregar operações') } })
      .finally(() => { if (ativo) setCarregando(false) })
    return () => { ativo = false }
  }, [chave])

  const analises = useMemo(() => {
    if (!dashboard || carregando || erro) return []
    const posicoes = dashboard.posicoes.filter(p => p.quantidade > 0 && acao(p))
    const total = posicoes.reduce((s,p) => s + p.valorAtual, 0)
    return posicoes.map(p => {
      const ops = historico.filter(o => o.ticker.toUpperCase() === p.ticker.toUpperCase())
      const compras = ops.filter(o => operacaoTipo(o, 'COMPRA'))
      const vendas = ops.filter(o => operacaoTipo(o, 'VENDA'))
      const opcoes = dashboard.opcoes.filter(o => o.tickerAtivo?.toUpperCase() === p.ticker.toUpperCase())
      // Faixas extraídas de preços realmente executados; não são estimativas de valor justo.
      const compra = quantil(compras.map(o => o.precoUnitario), .25)
      const venda = quantil(vendas.map(o => o.precoUnitario), .75)
      const put = alvoHistorico(opcoes, 'PUT', compra, 'abaixo') ?? compra
      const call = alvoHistorico(opcoes, 'CALL', venda, 'acima') ?? venda
      const callsAbertas = opcoes.filter(o => opcaoTipo(o,'CALL') && o.estaAtiva)
      const putsAbertas = opcoes.filter(o => opcaoTipo(o,'PUT') && o.estaAtiva)
      const comprometidas = callsAbertas.reduce((s,o) => s + o.acoesComprometidasCall, 0)
      const livres = Math.max(0,p.quantidade - comprometidas)
      const peso = total > 0 ? p.valorAtual / total * 100 : 0
      const preco = p.precoAtual && p.precoAtual > 0 ? p.precoAtual : null
      const classificacao = compra !== null && preco !== null && preco <= compra ? 'Faixa histórica de compra' : venda !== null && preco !== null && preco >= venda ? 'Faixa histórica de venda' : 'Aguardar / avaliar'
      return { ...p, ops, compras, vendas, opcoes, compra, venda, put, call, preco, peso, livres, putsAbertas, callsAbertas, classificacao }
    }).sort((a,b) => a.ticker.localeCompare(b.ticker))
  }, [dashboard, historico, carregando, erro])
  const filtradas = analises.filter(p => (p.ticker + ' ' + p.nome).toLowerCase().includes(busca.toLowerCase()))
  const atual = analises.find(p => p.ticker === selecionado)
  const mostrar = (valor: number | null) => valor === null ? 'Sem referência' : moeda(valor)
  const delta = (alvo: number | null, atual: number | null) => alvo === null || atual === null ? '—' : percentual((alvo / atual - 1) * 100)
  return <section style={{ padding: 24, maxWidth: 1400, margin: '0 auto' }}>
    <header style={{ marginBottom: 22 }}>
      <small style={{ color: '#55d8bc', letterSpacing: 2 }}>INTELIGÊNCIA DE INVESTIMENTOS</small>
      <h1>Análise Estratégica da Carteira</h1>
      <p>Preços de referência para compra, venda e operações cobertas, usando o histórico consolidado de todos os investidores.</p>
    </header>
    <label>Pesquisar ativo <input value={busca} onChange={e => setBusca(e.target.value)} placeholder="Ticker ou empresa" style={{ display: 'block', padding: 10, margin: '8px 0 18px', minWidth: 230 }} /></label>
    {carregando && <p>Calculando preços a partir do histórico completo de operações...</p>}
    {erro && <p role="alert">Não foi possível calcular as referências históricas: {erro}</p>}
    {!carregando && !erro && !dashboard && <p>Carteira consolidada indisponível.</p>}
    {!carregando && !erro && dashboard && <div style={{ overflowX: 'auto' }}>
      <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
        <thead><tr>{['Ativo', 'Cotação', 'Comprar a mercado até', 'PUT: strike de referência', 'Vender a mercado a partir de', 'CALL: strike de referência', 'Leitura histórica'].map(t => <th key={t} style={{ padding: 12, borderBottom: '1px solid #497a75' }}>{t}</th>)}</tr></thead>
        <tbody>{filtradas.map(p => <tr key={p.ticker} onClick={() => setSelecionado(selecionado === p.ticker ? null : p.ticker)} style={{ cursor: 'pointer', background: selecionado === p.ticker ? 'rgba(80,210,170,.12)' : undefined }}>
          <td style={{ padding: 12 }}><strong>{p.ticker}</strong><div style={{ fontSize: 12, opacity: .7 }}>{p.nome}</div></td>
          <td>{mostrar(p.preco)}</td><td>{mostrar(p.compra)}</td><td>{mostrar(p.put)}</td><td>{mostrar(p.venda)}</td><td>{mostrar(p.call)}</td><td>{p.classificacao}</td>
        </tr>)}</tbody>
      </table>
      {!filtradas.length && <p>Nenhum ativo encontrado.</p>}
    </div>}
    {atual && <article style={{ border: '1px solid #497a75', borderRadius: 12, padding: 20, marginTop: 24 }}>
      <h2>{atual.ticker} — valores de entrada e saída</h2>
      <p>Cotação cadastrada: <strong>{mostrar(atual.preco)}</strong> | Preço médio: <strong>{moeda(atual.precoMedio)}</strong> | Posição: <strong>{atual.quantidade.toLocaleString('pt-BR')}</strong> ações | Peso na carteira de ações: <strong>{percentual(atual.peso)}</strong></p>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(215px, 1fr))', gap: 12 }}>
        {[
          { titulo: 'COMPRA A MERCADO', valor: atual.compra, nota: '25º percentil dos preços de compras executadas' },
          { titulo: 'VENDA DE PUT', valor: atual.put, nota: 'Strike histórico compatível com a entrada, ou alvo teórico se não houver' },
          { titulo: 'VENDA A MERCADO', valor: atual.venda, nota: '75º percentil dos preços de vendas executadas' },
          { titulo: 'CALL COBERTA', valor: atual.call, nota: 'Strike histórico compatível com a saída, ou alvo teórico se não houver' },
        ].map(item => <div key={item.titulo} style={{ border: '1px solid #355955', borderRadius: 10, padding: 14 }}>
          <small style={{ opacity: .8 }}>{item.titulo}</small><h2 style={{ margin: '8px 0' }}>{mostrar(item.valor)}</h2>
          <div>Distância da cotação: {delta(item.valor, atual.preco)}</div><p style={{ fontSize: 12, opacity: .8 }}>{item.nota}</p>
        </div>)}
      </div>
      <p><strong>Base utilizada:</strong> {atual.compras.length} compras, {atual.vendas.length} vendas e {atual.opcoes.length} operações de opções registradas. Os preços de compra e venda são faixas históricas executadas, não valuation ou previsão de mercado.</p>
      <p><strong>Garantias:</strong> {atual.livres.toLocaleString('pt-BR')} ações potencialmente livres para CALL, antes de validar contratos, bloqueios e demais garantias; {atual.putsAbertas.length} PUTs e {atual.callsAbertas.length} CALLs ativas cadastradas.</p>
      <p><strong>Antes de operar opções:</strong> validar existência do strike na B3, vencimento, prêmio, spread, liquidez, delta, cobertura e custo efetivo. O sistema não dispõe de book confirmado nesta tela; não apresenta prêmios inventados nem envia ordens.</p>
      {atual.compra === null && <p>Sem compras históricas suficientes para definir preço de entrada.</p>}
      {atual.venda === null && <p>Sem vendas históricas para fundamentar preço de realização. Não foi arbitrado um alvo de venda.</p>}
    </article>}
  </section>
}
