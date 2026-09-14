import {
  useState,
} from 'react'

import {
  cadastrar,
} from '../../auth/authApi'

interface CadastroViewProps {
  onVoltar: () => void

  onCadastroRealizado: (
    mensagem: string,
  ) => void
}

export function CadastroView({
  onVoltar,
  onCadastroRealizado,
}: CadastroViewProps) {
  const [
    nome,
    setNome,
  ] =
    useState('')

  const [
    email,
    setEmail,
  ] =
    useState('')

  const [
    senha,
    setSenha,
  ] =
    useState('')

  const [
    confirmarSenha,
    setConfirmarSenha,
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
    salvando,
    setSalvando,
  ] =
    useState(false)

  async function enviar(
    event:
      React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    if (
      senha !==
      confirmarSenha
    ) {
      setErro(
        'As senhas informadas não são iguais.',
      )

      return
    }

    try {
      setSalvando(true)
      setErro(null)

      const mensagem =
        await cadastrar(
          nome.trim(),
          email.trim(),
          senha,
        )

      onCadastroRealizado(
        mensagem,
      )
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível realizar o cadastro.',
      )
    } finally {
      setSalvando(false)
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
            Cadastro
          </span>

          <h2>
            Criar conta
          </h2>

          <p>
            Após o cadastro,
            um administrador deverá
            liberar seu acesso.
          </p>
        </div>

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
              Nome
            </span>

            <input
              type="text"
              value={nome}
              autoComplete="name"
              required

              onChange={(event) =>
                setNome(
                  event.target.value,
                )
              }
            />
          </label>

          <label>
            <span>
              E-mail
            </span>

            <input
              type="email"
              value={email}
              autoComplete="email"
              required

              onChange={(event) =>
                setEmail(
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
              autoComplete="new-password"
              minLength={8}
              required

              onChange={(event) =>
                setSenha(
                  event.target.value,
                )
              }
            />

            <small>
              Mínimo de 8 caracteres,
              com maiúscula, minúscula
              e número.
            </small>
          </label>

          <label>
            <span>
              Confirmar senha
            </span>

            <input
              type="password"
              value={confirmarSenha}
              autoComplete="new-password"
              minLength={8}
              required

              onChange={(event) =>
                setConfirmarSenha(
                  event.target.value,
                )
              }
            />
          </label>

          <button
            className="auth-primary"
            type="submit"
            disabled={salvando}
          >
            {salvando
              ? 'Cadastrando...'
              : 'Criar conta'}
          </button>
        </form>

        <div className="auth-links auth-links-single">
          <button
            type="button"
            onClick={onVoltar}
          >
            Voltar para o login
          </button>
        </div>
      </section>
    </main>
  )
}