import {
  createContext,
  useContext,
  useEffect,
  useState,
} from 'react'

import type {
  ReactNode,
} from 'react'

import {
  entrar,
  obterSessao,
  sair,
} from './authApi'

import type {
  UsuarioAutenticado,
} from './authApi'

interface AuthContextValue {
  usuario:
    UsuarioAutenticado | null

  carregando: boolean

  login: (
    email: string,
    senha: string,
  ) => Promise<void>

  logout: () => Promise<void>

  possuiPermissao: (
    permissao: string,
  ) => boolean
}

const AuthContext =
  createContext<
    AuthContextValue | undefined
  >(undefined)

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({
  children,
}: AuthProviderProps) {
  const [
    usuario,
    setUsuario,
  ] =
    useState<UsuarioAutenticado | null>(
      null,
    )

  const [
    carregando,
    setCarregando,
  ] =
    useState(true)

  useEffect(() => {
    async function carregarSessao() {
      try {
        const sessao =
          await obterSessao()

        setUsuario(sessao)
      } catch (error) {
        console.error(error)

        setUsuario(null)
      } finally {
        setCarregando(false)
      }
    }

    carregarSessao()
  }, [])

  async function login(
    email: string,
    senha: string,
  ) {
    const resultado =
      await entrar(
        email,
        senha,
      )

    setUsuario(resultado)
  }

  async function logout() {
    try {
      await sair()
    } finally {
      setUsuario(null)
    }
  }

  function possuiPermissao(
    permissao: string,
  ) {
    if (!usuario) {
      return false
    }

    if (
      usuario.perfil === 'Admin'
    ) {
      return true
    }

    return usuario.permissoes
      .some(
        (item) =>
          item.toLowerCase() ===
          permissao.toLowerCase(),
      )
  }

  return (
    <AuthContext.Provider
      value={{
        usuario,
        carregando,
        login,
        logout,
        possuiPermissao,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context =
    useContext(AuthContext)

  if (!context) {
    throw new Error(
      'useAuth deve ser utilizado dentro de AuthProvider.',
    )
  }

  return context
}