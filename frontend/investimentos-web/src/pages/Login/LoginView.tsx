import {
  useState,
} from 'react'

import {
  useAuth,
} from '../../auth/AuthContext'

interface LoginViewProps {
  mensagem: string | null
  onCriarConta: () => void
}

export function LoginView({
  mensagem,
  onCriarConta,
}: LoginViewProps) {
  const {
    login,
  } = useAuth()

  const [
    identificador,
    setIdentificador,
  ] =
    useState('')

  const [
    senha,
    setSenha,
  ] =
    useState('')

  const [
    erro,
    setErro,
  ] =
    useState<string | null>(
      null,
    )

  const [
    entrando,
    setEntrando,
  ] =
    useState(false)

  async function enviar(
    event:
      React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    try {
      setEntrando(true)
      setErro(null)

      await login(
        identificador.trim(),
        senha,
      )
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível entrar.',
      )
    } finally {
      setEntrando(false)
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-card">
        <div className="auth-brand">
          <img
            src="/tio-patinhas.png"
            alt=""
          />

          <div>
            <h1>
              Investimentos
            </h1>

            <p>
              Gestão da carteira
            </p>
          </div>
        </div>

        <div className="auth-heading">
          <span>
            Acesso
          </span>

          <h2>
            Entrar no sistema
          </h2>

          <p>
            Informe suas credenciais
            para continuar.
          </p>
        </div>

        {mensagem ? (
          <div className="auth-success">
            {mensagem}
          </div>
        ) : null}

        {erro ? (
          <div className="auth-error">
            {erro}
          </div>
        ) : null}

        <form
          className="auth-form"
          onSubmit={enviar}
        >
          <label>
            <span>
              Usuário ou e-mail
            </span>

            <input
              type="text"
              value={identificador}
              autoComplete="username"
              autoFocus
              required
              placeholder="admin ou seu e-mail"
              onChange={(event) =>
                setIdentificador(
                  event.target.value,
                )
              }
            />
          </label>

          <label>
            <span>
              Senha
            </span>

            <input
              type="password"
              value={senha}
              autoComplete="current-password"
              required
              onChange={(event) =>
                setSenha(
                  event.target.value,
                )
              }
            />
          </label>

          <button
            className="auth-primary"
            type="submit"
            disabled={entrando}
          >
            {entrando
              ? 'Entrando...'
              : 'Entrar'}
          </button>
        </form>

        <div className="auth-links">
          <button
            type="button"
            onClick={onCriarConta}
          >
            Criar uma conta
          </button>

          <button
            type="button"
            disabled
            title="Será habilitado na etapa de recuperação de senha."
          >
            Esqueci minha senha
          </button>
        </div>
      </section>
    </main>
  )
}