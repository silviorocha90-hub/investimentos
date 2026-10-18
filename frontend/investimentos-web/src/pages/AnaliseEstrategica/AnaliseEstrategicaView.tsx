import { useEffect, useMemo, useState } from 'react'
import { obterOperacoes } from '../../api/dashboardApi'
import { obterAdministracao } from '../../api/administracaoApi'
import type { AtivoAdministracao } from '../../types/administracao'
import type { Dashboard, OperacaoCarteira, OperacaoOpcao, PosicaoAtivo } from '../../types/dashboard'

const moeda = (n: number) => n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
const percentual = (n: number) => n.toLocaleString('pt-BR', { maximumFractionDigits: 1 }) + '%'
const acao = (p: PosicaoAtivo) => /^(ACAO|AÇÃO|ACOES|AÇÕES)$/i.test(p.tipoAtivoCodigo) || /^[A-Z]{4}[3-6]$/.test(p.ticker)
const operacaoTipo = (op: OperacaoCarteira, tipo: string) => op.tipoOperacao.toUpperCase().includes(tipo)
const opcaoTipo = (op: OperacaoOpcao, tipo: string) => op.tipoOpcao.toUpperCase() === tipo
const encerrada = (op: OperacaoOpcao) => ['ENCERRADA', 'EXECUTADA', 'EXPIRADA'].includes(op.situacao.toUpperCase())
const ganhoPositivo = (op: OperacaoOpcao) => (op.resultadoFinal ?? op.resultadoLiquido ?? op.resultadoBruto ?? op.resultadoInformado ?? 0) > 0
const abaixo = (valor: number) => Math.max(0, Math.round((valor - .01) * 100) / 100)
const acima = (valor: number) => Math.round((valor + .01) * 100) / 100

export function AnaliseEstrategicaView({ dashboard, investidoresIds }: { dashboard: Dashboard | null; investidoresIds: string[] }) {
  const [historico, setHistorico] = useState<OperacaoCarteira[]>([])
  const [ativos, setAtivos] = useState<AtivoAdministracao[]>([])
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [busca, setBusca] = useState('')
  const [selecionado, setSelecionado] = useState<string | null>(null)
  const chave = [...investidoresIds].sort().join(',')

  useEffect(() => {
    let ativo = true
    setCarregando(true)
    setErro(null)
    Promise.all([
      Promise.all(chave.split(',').filter(Boolean).map(id => obterOperacoes(id))),
      obterAdministracao(),
    ])
      .then(([grupos, administracao]) => {
        if (ativo) {
          setHistorico([...new Map(grupos.flat().map(o => [o.id, o])).values()].sort((a,b) => a.data.localeCompare(b.data) || a.sequencia - b.sequencia))
          setAtivos(administracao.ativos)
        }
      })
      .catch(e => { if (ativo) { setHistorico([]); setAtivos([]); setErro(e instanceof Error ? e.message : 'Erro ao carregar operações') } })
      .finally(() => { if (ativo) setCarregando(false) })
    return () => { ativo = false }
  }, [chave])

  const analises = useMemo(() => {
    if (!dashboard || carregando || erro) return []
    const posicoes = dashboard.posicoes.filter(p => p.quantidade > 0 && acao(p) && p.ticker.toUpperCase() !== 'FMP ELETROBRAS' && !p.ticker.toUpperCase().startsWith('FMP '))
    const total = posicoes.reduce((s,p) => s + p.valorAtual, 0)
    return posicoes.map(p => {
      const ops = historico.filter(o => o.ticker.toUpperCase() === p.ticker.toUpperCase())
      const compras = ops.filter(o => operacaoTipo(o, 'COMPRA'))
      const vendas = ops.filter(o => operacaoTipo(o, 'VENDA'))
      const opcoes = dashboard.opcoes.filter(o => o.tickerAtivo?.toUpperCase() === p.ticker.toUpperCase())
      const menorCompra = compras.length ? Math.min(...compras.filter(o => o.precoUnitario > 0).map(o => o.precoUnitario)) : null
      const compra = menorCompra === null || !Number.isFinite(menorCompra) ? null : abaixo(menorCompra)
      const putsLucrativas = opcoes.filter(o => opcaoTipo(o, 'PUT') && encerrada(o) && ganhoPositivo(o) && o.strike > 0)
      const menorStrikeLucro = putsLucrativas.length ? Math.min(...putsLucrativas.map(o => o.strike)) : null
      const put = menorStrikeLucro === null ? null : abaixo(menorStrikeLucro)
      const maiorHistorico = [...ops.map(o => o.precoUnitario), ...opcoes.map(o => o.strike)].filter(n => Number.isFinite(n) && n > 0)
      const maiorRegistrado = maiorHistorico.length ? Math.max(...maiorHistorico) : null
      // Lucro mínimo de 15% sobre o preço médio consolidado, além do maior preço registrado.
      const venda = maiorRegistrado === null || p.precoMedio <= 0 ? null : acima(Math.max(maiorRegistrado, p.precoMedio * 1.15))
      const call = venda
      const callsAbertas = opcoes.filter(o => opcaoTipo(o,'CALL') && o.estaAtiva)
      const putsAbertas = opcoes.filter(o => opcaoTipo(o,'PUT') && o.estaAtiva)
      const comprometidas = callsAbertas.reduce((s,o) => s + o.acoesComprometidasCall, 0)
      const livres = Math.max(0,p.quantidade - comprometidas)
      const peso = total > 0 ? p.valorAtual / total * 100 : 0
      const ativo = ativos.find(a => a.ticker.toUpperCase() === p.ticker.toUpperCase())
      const preco = ativo?.cotacaoAtual && ativo.cotacaoAtual > 0 ? ativo.cotacaoAtual : null
      const pm = p.precoMedio
      const resultadoEconomico = (p.resultadoRealizado ?? 0) + (preco !== null ? (preco - pm) * p.quantidade : 0) + (p.proventos ?? 0) + (p.resultadoOpcoes ?? 0)
      const rentavel = resultadoEconomico > 0
      const classificacao = !preco ? 'Sem cotação atualizada' :
        !rentavel ? 'Resultado econômico não positivo: aguardar / avaliar venda' :
        venda !== null && preco >= venda ? 'Rentável: avaliar venda ou CALL' :
        compra !== null && preco <= compra ? 'Rentável: avaliar compra a mercado' :
        put !== null && preco > put ? 'Rentável: avaliar PUT com garantia' :
        'Rentável: manter / aguardar preço'
      return { ...p, ops, compras, vendas, opcoes, compra, venda, put, call, preco, pm, peso, livres, putsAbertas, callsAbertas, classificacao, resultadoEconomico, rentavel, menorCompra, menorStrikeLucro, maiorRegistrado, dataCotacao: ativo?.dataCotacao ?? null, putsLucrativas }
    }).sort((a,b) => a.ticker.localeCompare(b.ticker))
  }, [dashboard, historico, ativos, carregando, erro])
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
        <thead><tr>{['Ativo', 'Cotação (Ativo)', 'PM consolidado', 'Compra abaixo de', 'PUT abaixo de', 'Venda acima de', 'CALL acima de', 'Leitura histórica'].map(t => <th key={t} style={{ padding: 12, borderBottom: '1px solid #497a75' }}>{t}</th>)}</tr></thead>
        <tbody>{filtradas.map(p => <tr key={p.ticker} onClick={() => setSelecionado(selecionado === p.ticker ? null : p.ticker)} style={{ cursor: 'pointer', background: selecionado === p.ticker ? 'rgba(80,210,170,.12)' : undefined }}>
          <td style={{ padding: 12 }}><strong>{p.ticker}</strong><div style={{ fontSize: 12, opacity: .7 }}>{p.nome}</div></td>
          <td>{mostrar(p.preco)}</td><td>{moeda(p.pm)}</td><td>{mostrar(p.compra)}</td><td>{mostrar(p.put)}</td><td>{mostrar(p.venda)}</td><td>{mostrar(p.call)}</td><td>{p.classificacao}</td>
        </tr>)}</tbody>
      </table>
      {!filtradas.length && <p>Nenhum ativo encontrado.</p>}
    </div>}
    {atual && <article style={{ border: '1px solid #497a75', borderRadius: 12, padding: 20, marginTop: 24 }}>
      <h2>{atual.ticker} — valores de entrada e saída</h2>
      <p>Cotação do cadastro de ativos: <strong>{mostrar(atual.preco)}</strong> ({atual.dataCotacao ? new Date(atual.dataCotacao).toLocaleString('pt-BR') : 'sem data'}) | PM consolidado: <strong>{moeda(atual.pm)}</strong> | Posição: <strong>{atual.quantidade.toLocaleString('pt-BR')}</strong> ações | Peso na carteira de ações: <strong>{percentual(atual.peso)}</strong></p>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(215px, 1fr))', gap: 12 }}>
        {[
          { titulo: 'COMPRA A MERCADO', valor: atual.compra, nota: 'Abaixo da menor compra registrada no histórico consolidado' },
          { titulo: 'VENDA DE PUT', valor: atual.put, nota: 'Abaixo do menor strike de PUT encerrada com resultado positivo' },
          { titulo: 'VENDA A MERCADO', valor: atual.venda, nota: 'Acima do maior preço histórico registrado e com pelo menos 15% sobre o PM' },
          { titulo: 'CALL COBERTA', valor: atual.call, nota: 'Strike-alvo acima do maior preço histórico e do PM acrescido de 15%; validar cobertura' },
        ].map(item => <div key={item.titulo} style={{ border: '1px solid #355955', borderRadius: 10, padding: 14 }}>
          <small style={{ opacity: .8 }}>{item.titulo}</small><h2 style={{ margin: '8px 0' }}>{mostrar(item.valor)}</h2>
          <div>Distância da cotação: {delta(item.valor, atual.preco)}</div><p style={{ fontSize: 12, opacity: .8 }}>{item.nota}</p>
        </div>)}
      </div>
      <p><strong>Base utilizada:</strong> {atual.compras.length} compras, {atual.vendas.length} vendas e {atual.opcoes.length} operações de opções registradas. A compra usa o menor preço executado; a PUT usa apenas opções encerradas com resultado positivo; a venda exige superar o maior preço registrado e o PM em pelo menos 15%.</p>
      <p><strong>Leitura histórica:</strong> resultado econômico estimado {moeda(atual.resultadoEconomico)}. {atual.rentavel ? 'Ativo com resultado positivo na carteira; avaliar oportunidades somente nos preços-alvo.' : 'Resultado não positivo; evitar reforço automático e reavaliar fundamentos e saída.'} Esta avaliação mede rentabilidade na carteira, não qualidade fundamentalista da empresa.</p>
      <p><strong>Garantias:</strong> {atual.livres.toLocaleString('pt-BR')} ações potencialmente livres para CALL, antes de validar contratos, bloqueios e demais garantias; {atual.putsAbertas.length} PUTs e {atual.callsAbertas.length} CALLs ativas cadastradas.</p>
      <p><strong>Antes de operar opções:</strong> validar existência do strike na B3, vencimento, prêmio, spread, liquidez, delta, cobertura e custo efetivo. O sistema não dispõe de book confirmado nesta tela; não apresenta prêmios inventados nem envia ordens.</p>
      {atual.compra === null && <p>Sem compras registradas para definir preço inferior ao mínimo histórico.</p>}
      {atual.put === null && <p>Sem PUT encerrada com resultado positivo; não há strike histórico lucrativo para referência.</p>}
      {atual.venda === null && <p>Sem PM ou preços históricos suficientes para estabelecer saída com 15% de ganho.</p>}
    </article>}
  </section>
}
