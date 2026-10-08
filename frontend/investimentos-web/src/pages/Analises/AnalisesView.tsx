import { useMemo, useState } from 'react'
import { PageHeader } from '../../components/PageHeader'
import type { Dashboard } from '../../types/dashboard'
import type { Investidor } from '../../types/investidor'
import { formatarMoeda } from '../../utils/formatters'
import './Analises.css'
interface Props { investidores: readonly Investidor[]; carteiras: ReadonlyArray<{ nome:string; dashboard:Dashboard }> }
const pct=(v:number)=>`${v.toLocaleString('pt-BR',{minimumFractionDigits:1,maximumFractionDigits:1})}%`
export function AnalisesView({investidores,carteiras}:Props){
 const [investidor,setInvestidor]=useState(investidores[0]?.nome??'')
 const dashboard=carteiras.find(x=>x.nome===investidor)?.dashboard??null
 const dados=useMemo(()=>{if(!dashboard)return null;const pos=[...(dashboard.posicoes??[])].filter(x=>x.valorAtual>0).sort((a,b)=>b.valorAtual-a.valorAtual);return{pos,total:Math.max(pos.reduce((s,x)=>s+x.valorAtual,0),1),max:Math.max(...pos.map(x=>x.valorAtual),1),proventos:[...pos].filter(x=>(x.proventos??0)>0).sort((a,b)=>(b.proventos??0)-(a.proventos??0)).slice(0,8)}},[dashboard])
 return <section className="portfolio-view analises-page"><PageHeader titulo="Análises" investidores={investidores} selectedInvestor={investidor} onSelectInvestor={setInvestidor} incluirTodos={false}/>
 {!dashboard||!dados?<div className="analises-empty">Selecione um investidor com carteira disponível.</div>:<><div className="analises-kpis">
 <article><span>Patrimônio investido</span><strong>{formatarMoeda(dados.total)}</strong></article><article><span>Resultado da carteira</span><strong className={dashboard.resultadoCarteira>=0?'positive':'negative'}>{formatarMoeda(dashboard.resultadoCarteira)}</strong></article><article><span>Rentabilidade no ano</span><strong className={(dashboard.rentabilidadeAno??0)>=0?'positive':'negative'}>{pct(dashboard.rentabilidadeAno??0)}</strong></article><article><span>Proventos recebidos</span><strong>{formatarMoeda(dashboard.totalProventos)}</strong></article></div>
 <div className="analises-grid"><article className="analises-card"><header><strong>Concentração por ativo</strong><span>Peso das maiores posições</span></header><div className="analises-bars">{dados.pos.slice(0,10).map(x=><div className="analises-bar" key={x.ticker}><div><b>{x.ticker}</b><span>{pct(x.valorAtual/dados.total*100)}</span></div><i><em style={{width:`${x.valorAtual/dados.max*100}%`}}/></i><small>{formatarMoeda(x.valorAtual)}</small></div>)}</div></article>
 <article className="analises-card"><header><strong>Alocação por classe</strong><span>Diversificação da carteira</span></header><div className="analises-allocation">{(dashboard.distribuicaoPorTipo??[]).filter(x=>x.valor>0).sort((a,b)=>b.valor-a.valor).map(x=><div key={x.codigo}><span><b>{x.nome}</b><small>{formatarMoeda(x.valor)}</small></span><strong>{pct(x.percentual)}</strong></div>)}</div></article>
 <article className="analises-card"><header><strong>Renda por ativo</strong><span>Maiores geradores de proventos</span></header><div className="analises-ranking">{dados.proventos.map((x,i)=><div key={x.ticker}><span>{i+1}</span><b>{x.ticker}</b><strong>{formatarMoeda(x.proventos??0)}</strong></div>)}</div></article>
 <article className="analises-card"><header><strong>Resultado por posição</strong><span>Rentabilidade econômica dos ativos</span></header><div className="analises-ranking">{dados.pos.slice(0,10).map((x,i)=><div key={x.ticker}><span>{i+1}</span><b>{x.ticker}</b><strong className={(x.rentabilidadeEconomica??0)>=0?'positive':'negative'}>{pct(x.rentabilidadeEconomica??0)}</strong></div>)}</div></article></div></>}</section>
}