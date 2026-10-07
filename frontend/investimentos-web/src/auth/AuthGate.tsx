 import App from '../App'

import {
  LoginView,
} from '../pages/Login/LoginView'

 import {
  useAuth,
} from './AuthContext'

import './auth.css'

 export function AuthGate() {
  const {
    usuario,
    carregando,
  } = useAuth()

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
      mensagem={null}
    />
  )
}