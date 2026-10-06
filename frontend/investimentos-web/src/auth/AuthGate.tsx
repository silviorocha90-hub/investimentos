import {
  useState,
} from 'react'

import App from '../App'

import {
  LoginView,
} from '../pages/Login/LoginView'

import {
  CadastroView,
} from '../pages/Cadastro/CadastroView'

import {
  useAuth,
} from './AuthContext'

import './auth.css'

type TelaAutenticacao =
  | 'login'
  | 'cadastro'

export function AuthGate() {
  const {
    usuario,
    carregando,
  } = useAuth()

  const [
    tela,
    setTela,
  ] =
    useState<TelaAutenticacao>(
      'login',
    )

  const [
    mensagem,
    setMensagem,
  ] =
    useState<string | null>(
      null,
    )

  if (carregando) {
    return (
      <main className="auth-loading">
        <div className="loader" />

        <span>
          Verificando sessão...
        </span>
      </main>
    )
  }

  if (usuario) {
    return <App />
  }

  if (tela === 'cadastro') {
    return (
      <CadastroView
        onVoltar={() => {
          setMensagem(null)
          setTela('login')
        }}

        onCadastroRealizado={(
          mensagemCadastro,
        ) => {
          setMensagem(
            mensagemCadastro,
          )

          setTela('login')
        }}
      />
    )
  }

  return (
    <LoginView
      mensagem={mensagem}

      onCriarConta={() => {
        setMensagem(null)
        setTela('cadastro')
      }}
    />
  )
}