import { useState, type FormEvent } from 'react'
import { alterarSenha } from '../../auth/authApi'

export function AlterarSenhaModal({ onClose }: { onClose: () => void }) {
  const [atual, setAtual] = useState('')
  const [nova, setNova] = useState('')
  const [confirmacao, setConfirmacao] = useState('')
  const [erro, setErro] = useState('')
  const [salvando, setSalvando] = useState(false)
  const [sucesso, setSucesso] = useState(false)
  const regras = [
    nova.length >= 8,
    /[A-ZÀ-Ý]/.test(nova),
    /[a-zà-ÿ]/.test(nova),
    /[0-9]/.test(nova),
    /[^\p{L}\p{N}]/u.test(nova),
  ]
  async function enviar(event: FormEvent) {
    event.preventDefault()
    setErro('')
    setSalvando(true)
    try {
      await alterarSenha(atual, nova, confirmacao)
      setSucesso(true)
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Erro ao alterar senha.')
    } finally {
      setSalvando(false)
    }
  }
  return <div className="alterar-senha-overlay">
    <section className="alterar-senha-modal" role="dialog" aria-modal="true" aria-label="Alterar senha">
      <button type="button" onClick={onClose} aria-label="Fechar">×</button>
      <h2>Alterar senha</h2>
      {sucesso ? <p role="status">Senha alterada com sucesso.</p> :
        <form onSubmit={event => void enviar(event)}>
          <label>Senha atual<input required type="password" autoComplete="current-password" value={atual} onChange={event => setAtual(event.target.value)} /></label>
          <label>Nova senha<input required type="password" autoComplete="new-password" value={nova} onChange={event => setNova(event.target.value)} /></label>
          <label>Confirmar nova senha<input required type="password" autoComplete="new-password" value={confirmacao} onChange={event => setConfirmacao(event.target.value)} /></label>
          <small>{['8 caracteres', 'Maiúscula', 'Minúscula', 'Número', 'Especial'].map((nome, i) => <span key={nome} style={{ color: regras[i] ? 'green' : 'inherit', display: 'block' }}>{regras[i] ? '✓' : '○'} {nome}</span>)}</small>
          {erro && <p role="alert">{erro}</p>}
          <button type="submit" disabled={salvando || !atual || !regras.every(Boolean) || nova !== confirmacao || nova === atual}>Alterar senha</button>
        </form>}
    </section>
  </div>
}
