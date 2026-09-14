const apiUrl =
  import.meta.env.VITE_API_URL ??
  'https://localhost:7237'

export interface UsuarioAutenticado {
  id: string
  nome: string
  email: string
  perfil: 'Admin' | 'Usuario'
  status:
    | 'Pendente'
    | 'Ativo'
    | 'Rejeitado'
    | 'Bloqueado'
  permissoes: string[]
  investidoresIds: string[]
}

interface ErroApi {
  detail?: string
  title?: string
  mensagem?: string
}

async function lerErro(
  response: Response,
): Promise<string> {
  const texto =
    await response.text()

  if (!texto.trim()) {
    if (response.status === 401) {
      return 'E-mail ou senha inválidos.'
    }

    return `HTTP ${response.status}`
  }

  try {
    const json =
      JSON.parse(texto) as ErroApi

    return (
      json.detail ??
      json.mensagem ??
      json.title ??
      texto
    )
  } catch {
    return texto
  }
}

export async function obterSessao():
  Promise<UsuarioAutenticado | null> {
  const response =
    await fetch(
      `${apiUrl}/api/auth/me`,
      {
        credentials: 'include',
      },
    )

  if (response.status === 401) {
    return null
  }

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }

  return response.json()
}

export async function entrar(
  email: string,
  senha: string,
): Promise<UsuarioAutenticado> {
  const response =
    await fetch(
      `${apiUrl}/api/auth/login`,
      {
        method: 'POST',

        credentials:
          'include',

        headers: {
          'Content-Type':
            'application/json',
        },

        body: JSON.stringify({
          email,
          senha,
        }),
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }

  return response.json()
}

export async function sair():
  Promise<void> {
  const response =
    await fetch(
      `${apiUrl}/api/auth/logout`,
      {
        method: 'POST',

        credentials:
          'include',
      },
    )

  if (
    !response.ok &&
    response.status !== 401
  ) {
    throw new Error(
      await lerErro(response),
    )
  }
}

export async function cadastrar(
  nome: string,
  email: string,
  senha: string,
): Promise<string> {
  const response =
    await fetch(
      `${apiUrl}/api/auth/cadastro`,
      {
        method: 'POST',

        credentials:
          'include',

        headers: {
          'Content-Type':
            'application/json',
        },

        body: JSON.stringify({
          nome,
          email,
          senha,
        }),
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }

  const resultado =
    await response.json() as {
      mensagem?: string
    }

  return (
    resultado.mensagem ??
    'Cadastro realizado com sucesso.'
  )
}