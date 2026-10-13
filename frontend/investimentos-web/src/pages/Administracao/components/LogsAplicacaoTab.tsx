import { useEffect, useState } from 'react'

interface LogAplicacao {
  id: string
  dataUtc: string
  ambiente: string
  modulo: string
  operacao: string
  nivel: string
  mensagem: string
  detalhes: string | null
  correlacaoId: string | null
}

const apiUrl = import.meta.env.VITE_API_URL ?? 'https://localhost:7237'

export function LogsAplicacaoTab() {
  const [logs, setLogs] = useState<LogAplicacao[]>([])
  const [carregando, setCarregando] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const [modulo, setModulo] = useState('')
  const [nivel, setNivel] = useState('')
  const [correlacao, setCorrelacao] = useState('')
  const [selecionado, setSelecionado] = useState<string | null>(null)

  async function carregar() {
    setCarregando(true)
    setErro(null)
    try {
      const params = new URLSearchParams({ limite: '200' })
      if (modulo.trim()) params.set('modulo', modulo.trim())
      if (nivel) params.set('nivel', nivel)
      if (correlacao.trim()) params.set('correlacaoId', correlacao.trim())
      const response = await fetch(`${apiUrl}/api/admin/logs?${params}`, {
        credentials: 'include', cache: 'no-store',
      })
      if (!response.ok) throw new Error(`Falha ao consultar logs (HTTP ${response.status})`)
      setLogs(await response.json() as LogAplicacao[])
    } catch (e) {
      setErro(e instanceof Error ? e.message : 'Falha ao consultar logs')
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => { void carregar() }, [])

  return (
    <article className="panel admin-panel">
      <div className="admin-toolbar">
        <strong>Logs da aplicação</strong>
        <button type="button" className="admin-primary-button" disabled={carregando}
          onClick={() => void carregar()}>{carregando ? 'Consultando...' : 'Atualizar logs'}</button>
      </div>
      <div className="admin-toolbar-actions" style={{ padding: 12 }}>
        <input aria-label="Módulo" placeholder="Módulo (ex.: Assistente IA)" value={modulo}
          onChange={e => setModulo(e.target.value)} />
        <select aria-label="Nível" value={nivel} onChange={e => setNivel(e.target.value)}>
          <option value="">Todos os níveis</option>
          <option value="ERRO">Erro</option>
          <option value="AVISO">Aviso</option>
        </select>
        <input aria-label="ID de correlação" placeholder="Código da execução" value={correlacao}
          onChange={e => setCorrelacao(e.target.value)} />
        <button type="button" className="admin-clear-button" onClick={() => void carregar()}>Filtrar</button>
      </div>
      {erro && <div className="admin-inline-error">{erro}</div>}
      <div className="admin-table-wrap">
        <table className="admin-table">
          <thead><tr><th>Data/Hora</th><th>Ambiente</th><th>Módulo</th><th>Operação</th><th>Nível</th><th>Mensagem</th><th>Detalhes</th></tr></thead>
          <tbody>
            {logs.map(log => (
              <tr key={log.id}>
                <td>{new Date(log.dataUtc).toLocaleString('pt-BR')}</td>
                <td>{log.ambiente}</td><td>{log.modulo}</td><td>{log.operacao}</td>
                <td>{log.nivel}</td><td title={log.mensagem}>{log.mensagem}</td>
                <td><button type="button" className="admin-action"
                  onClick={() => setSelecionado(selecionado === log.id ? null : log.id)}>
                  {selecionado === log.id ? 'Fechar' : 'Ver'}
                </button></td>
              </tr>
            ))}
            {logs.length === 0 && <tr><td colSpan={7}>Nenhum log encontrado.</td></tr>}
          </tbody>
        </table>
      </div>
      {logs.filter(x => x.id === selecionado).map(log => (
        <div key={log.id} style={{ padding: 16, overflowWrap: 'anywhere' }}>
          <strong>Detalhes — {log.correlacaoId ?? log.id}</strong>
          <pre style={{ whiteSpace: 'pre-wrap', overflowWrap: 'anywhere' }}>
            {log.detalhes ?? log.mensagem}
          </pre>
        </div>
      ))}
    </article>
  )
}
