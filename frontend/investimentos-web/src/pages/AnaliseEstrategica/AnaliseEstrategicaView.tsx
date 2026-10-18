import { useEffect, useMemo, useState } from 'react'
import { SectionTitle } from '../../components/SectionTitle'
import './AnaliseEstrategica.css'
import { obterOperacoes } from '../../api/dashboardApi'
import { obterAdministracao } from '../../api/administracaoApi'
import type { AtivoAdministracao } from '../../types/administracao'
import type { Dashboard, OperacaoCarteira, OperacaoOpcao, PosicaoAtivo } from '../../types/dashboard'

const moeda = (n: number) => n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
const percentual = (n: number) => n.toLocaleString('pt-BR', { maximumFractionDigits: 1 }) + '%'
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
    // Incluir ativos já negociados, mesmo sem posição atual, e subjacentes
    // de opções, mesmo quando nunca houve compra direta da ação.
    const elegivel = (ticker: string) => !!ticker.trim() &&
      ticker.toUpperCase() !== 'FMP ELETROBRAS' && !ticker.toUpperCase().startsWith('FMP ')
    const porTicker = new Map<string, PosicaoAtivo>()
    for (const p of dashboard.posicoes) {
      if (elegivel(p.ticker)) porTicker.set(p.ticker.toUpperCase(), p)
    }
    for (const op of historico) {
      const ticker = op.ticker.trim().toUpperCase()
      if (!elegivel(ticker) || porTicker.has(ticker)) continue
      porTicker.set(ticker, {
        ticker, nome: op.nome || ticker, tipoAtivoCodigo: '', tipoAtivoNome: '',
        quantidade: 0, precoMedio: 0, custoTotal: 0, resultadoRealizado: 0,
        valorAtual: 0, valorizacao: 0,
      })
    }
    for (const op of dashboard.opcoes) {
      const ticker = (op.tickerAtivo ?? '').trim().toUpperCase()
      if (!elegivel(ticker) || porTicker.has(ticker)) continue
      porTicker.set(ticker, {
        ticker, nome: ativos.find(a => a.ticker.toUpperCase() === ticker)?.nome ?? ticker,
        tipoAtivoCodigo: '', tipoAtivoNome: '', quantidade: 0, precoMedio: 0,
        custoTotal: 0, resultadoRealizado: 0, valorAtual: 0, valorizacao: 0,
      })
    }
    const posicoes = [...porTicker.values()]
    const total = posicoes.filter(p => p.quantidade > 0).reduce((s,p) => s + p.valorAtual, 0)
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
      const venda = p.quantidade <= 0 || maiorRegistrado === null || p.precoMedio <= 0 ? null : acima(Math.max(maiorRegistrado, p.precoMedio * 1.15))
      const call = p.quantidade > 0 ? venda : null
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
      const classificacao = p.quantidade <= 0 ? 'Sem posição atual: referência histórica, sem recomendação automática' :
        !preco ? 'Sem cotação atualizada' :
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
  return (
    <section className="estrategia-page">
      <div className="estrategia-heading">
        <SectionTitle title="Análise Estratégica" subtitle="Preços de referência para compra, venda e opções com base no histórico consolidado da carteira." />
      </div>
      <div className="estrategia-panel">
        <div className="estrategia-toolbar">
          <label className="estrategia-search">
            <span>Pesquisar ativo</span>
            <input value={busca} onChange={e => setBusca(e.target.value)} placeholder="Ticker ou empresa" />
          </label>
        </div>
        {carregando && <div className="estrategia-state">Calculando preços a partir do histórico completo de operações...</div>}
        {erro && <div className="estrategia-state estrategia-error" role="alert">Não foi possível calcular as referências históricas: {erro}</div>}
        {!carregando && !erro && !dashboard && <div className="estrategia-state">Carteira consolidada indisponível.</div>}
        {!carregando && !erro && dashboard && <div className="estrategia-table-wrap">
          <table className="estrategia-table">
            <thead><tr>{['Ativo', 'Cotação', 'PM consolidado', 'Compra abaixo de', 'PUT abaixo de', 'Venda acima de', 'CALL acima de', 'Leitura histórica'].map(t => <th key={t}>{t}</th>)}</tr></thead>
            <tbody>{filtradas.map(p => <tr key={p.ticker} className={selecionado === p.ticker ? 'selected' : ''} onClick={() => setSelecionado(selecionado === p.ticker ? null : p.ticker)} aria-selected={selecionado === p.ticker}>
              <td><strong>{p.ticker}</strong><small>{p.nome}</small></td>
              <td>{mostrar(p.preco)}</td>
              <td>{moeda(p.pm)}</td>
              <td className="estrategia-positive">{mostrar(p.compra)}</td>
              <td className="estrategia-positive">{mostrar(p.put)}</td>
              <td>{mostrar(p.venda)}</td>
              <td>{mostrar(p.call)}</td>
              <td><span className={p.rentavel ? 'estrategia-status positive' : 'estrategia-status'}>{p.classificacao}</span></td>
            </tr>)}</tbody>
          </table>
          {!filtradas.length && <div className="estrategia-state">Nenhum ativo encontrado.</div>}
        </div>}
      </div>
      {atual && (
        <article className="estrategia-panel estrategia-details">
          <div className="estrategia-details-header">
            <div><h2>{atual.ticker} — valores de entrada e saída</h2><p>{atual.nome}</p></div>
            <button className="admin-clear-button" type="button" onClick={() => setSelecionado(null)}>Fechar detalhes</button>
          </div>
          <div className="estrategia-overview">
            <div><span>Cotação atual</span><strong>{mostrar(atual.preco)}</strong><small>{atual.dataCotacao ? new Date(atual.dataCotacao).toLocaleString('pt-BR') : 'Sem data de atualização'}</small></div>
            <div><span>Preço médio consolidado</span><strong>{moeda(atual.pm)}</strong></div>
            <div><span>Quantidade</span><strong>{atual.quantidade.toLocaleString('pt-BR')}</strong></div>
            <div><span>Peso em ações</span><strong>{percentual(atual.peso)}</strong></div>
          </div>
          <div className="estrategia-opportunities">
            {[
              { titulo: 'Compra a mercado', valor: atual.compra, nota: 'Abaixo da menor compra registrada no histórico consolidado' },
              { titulo: 'Venda de PUT', valor: atual.put, nota: 'Abaixo do menor strike de PUT encerrada com resultado positivo' },
              { titulo: 'Venda a mercado', valor: atual.venda, nota: 'Acima do maior preço histórico registrado e com pelo menos 15% sobre o PM' },
              { titulo: 'CALL coberta', valor: atual.call, nota: 'Strike-alvo acima do maior preço histórico e do PM acrescido de 15%; validar cobertura' },
            ].map(item => <div className="estrategia-opportunity" key={item.titulo}>
              <span>{item.titulo}</span><strong>{mostrar(item.valor)}</strong>
              <small>Distância da cotação: {delta(item.valor, atual.preco)}</small>
              <p>{item.nota}</p>
            </div>)}
          </div>
          <div className="estrategia-notes">
            <p><strong>Base utilizada:</strong> {atual.compras.length} compras, {atual.vendas.length} vendas e {atual.opcoes.length} operações de opções registradas. A compra usa o menor preço executado; a PUT usa apenas opções encerradas com resultado positivo; a venda exige superar o maior preço registrado e o PM em pelo menos 15%.</p>
            {atual.quantidade <= 0 && <p><strong>Sem posição atual:</strong> este ativo aparece por operações históricas de carteira ou opções. Preço médio e resultado econômico não representam uma posição em aberto; referências de entrada não constituem recomendação de compra.</p>}
            {atual.quantidade > 0 && <p><strong>Leitura histórica:</strong> resultado econômico estimado {moeda(atual.resultadoEconomico)}. {atual.rentavel ? 'Ativo com resultado positivo na carteira; avaliar oportunidades somente nos preços-alvo.' : 'Resultado não positivo; evitar reforço automático e reavaliar fundamentos e saída.'} Esta avaliação mede rentabilidade na carteira, não qualidade fundamentalista da empresa.</p>}
            <p><strong>Garantias:</strong> {atual.livres.toLocaleString('pt-BR')} ações potencialmente livres para CALL, antes de validar contratos, bloqueios e demais garantias; {atual.putsAbertas.length} PUTs e {atual.callsAbertas.length} CALLs ativas cadastradas.</p>
            <p><strong>Antes de operar opções:</strong> validar existência do strike na B3, vencimento, prêmio, spread, liquidez, delta, cobertura e custo efetivo. O sistema não dispõe de book confirmado nesta tela; não apresenta prêmios inventados nem envia ordens.</p>
            {atual.compra === null && <p>Sem compras registradas para definir preço inferior ao mínimo histórico.</p>}
            {atual.put === null && <p>Sem PUT encerrada com resultado positivo; não há strike histórico lucrativo para referência.</p>}
            {atual.venda === null && <p>Sem PM ou preços históricos suficientes para estabelecer saída com 15% de ganho.</p>}
          </div>
        </article>
      )}
    </section>
  )
}
