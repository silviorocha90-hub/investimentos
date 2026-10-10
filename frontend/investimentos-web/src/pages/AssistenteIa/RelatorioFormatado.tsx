import { Fragment, type ReactNode } from 'react'

function inline(text: string): ReactNode[] {
  const pattern = /(\[([^\]]+)\]\((https?:\/\/[^\s)]+)\)|\*\*([^*]+)\*\*|\`([^\`]+)\`)/g
  const parts: ReactNode[] = []
  let cursor = 0
  for (const match of text.matchAll(pattern)) {
    const index = match.index ?? 0
    if (index > cursor) parts.push(text.slice(cursor, index))
    const key = String(index)
    if (match[2] && match[3]) {
      parts.push(<a key={key} href={match[3]} target="_blank" rel="noopener noreferrer">{match[2]}</a>)
    } else if (match[4]) {
      parts.push(<strong key={key}>{match[4]}</strong>)
    } else if (match[5]) {
      parts.push(<code key={key}>{match[5]}</code>)
    }
    cursor = index + match[0].length
  }
  if (cursor < text.length) parts.push(text.slice(cursor))
  return parts
}

function tabela(linhas: string[], key: string) {
  const celulas = linhas.map(linha => linha.trim().replace(/^\|/, '').replace(/\|$/, '').split('|').map(c => c.trim()))
  if (celulas.length < 2 || !celulas[1].every(c => /^:?-{3,}:?$/.test(c))) return null
  const [cabecalho, , ...dados] = celulas
  return <div className="assistant-table-scroll" key={key}><table className="assistant-data-table">
    <thead><tr>{cabecalho.map((c, i) => <th key={i}>{inline(c)}</th>)}</tr></thead>
    <tbody>{dados.map((linha, i) => <tr key={i}>{cabecalho.map((_, j) => {
      const valor = linha[j] ?? ''
      const negativo = /(^|\s)(-\s?R\$|−\s?R\$|-\d|−\d)/.test(valor)
      const positivo = /(^|\s)\+\s?(R\$|\d)/.test(valor)
      return <td key={j} className={negativo ? 'assistant-negative' : positivo ? 'assistant-positive' : ''}>{inline(valor)}</td>
    })}</tr>)}</tbody>
  </table></div>
}

export function RelatorioFormatado({ conteudo }: { conteudo: string }) {
  const linhas = conteudo.split(/\r?\n/)
  const blocos: ReactNode[] = []
  let i = 0
  while (i < linhas.length) {
    const linha = linhas[i].trim()
    if (!linha) { i++; continue }
    if (linha.startsWith('|')) {
      const inicio = i
      const tab: string[] = []
      while (i < linhas.length && linhas[i].trim().startsWith('|')) tab.push(linhas[i++])
      blocos.push(tabela(tab, String(inicio)) ?? <p key={inicio}>{tab.join(' ')}</p>)
      continue
    }
    const titulo = /^(#{1,4})\s+(.+)$/.exec(linha)
    if (titulo) {
      const nivel = titulo[1].length
      const conteudoTitulo = inline(titulo[2])
      blocos.push(nivel === 1 ? <h2 key={i}>{conteudoTitulo}</h2> : nivel === 2 ? <h3 key={i}>{conteudoTitulo}</h3> : <h4 key={i}>{conteudoTitulo}</h4>)
      i++; continue
    }
    if (/^([-*]|\d+\.)\s+/.test(linha)) {
      const inicio = i
      const itens: ReactNode[] = []
      while (i < linhas.length && /^([-*]|\d+\.)\s+/.test(linhas[i].trim())) {
        itens.push(<li key={i}>{inline(linhas[i].trim().replace(/^([-*]|\d+\.)\s+/, ''))}</li>)
        i++
      }
      blocos.push(<ul key={inicio}>{itens}</ul>)
      continue
    }
    const inicio = i
    const paragrafos: string[] = []
    while (i < linhas.length && linhas[i].trim() && !/^(#|\||[-*]\s|\d+\.\s)/.test(linhas[i].trim())) paragrafos.push(linhas[i++].trim())
    if (paragrafos.length) blocos.push(<p key={inicio}>{inline(paragrafos.join(' '))}</p>)
    else { blocos.push(<p key={i}>{inline(linhas[i])}</p>); i++ }
  }
  return <Fragment>{blocos}</Fragment>
}
