import type {
  AlterarAcessosUsuario,
  UsuarioAdministracao,
} from '../types/usuario'

const apiUrl =
  import.meta.env.VITE_API_URL ??
  'https://localhost:7237'

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
      return 'Sua sessão expirou. Entre novamente.'
    }

    if (response.status === 403) {
      return 'Você não possui permissão para realizar esta operação.'
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

async function executar(
  caminho: string,
  init?: RequestInit,
): Promise<Response> {
  const response =
    await fetch(
      `${apiUrl}${caminho}`,
      {
        ...init,

        credentials:
          'include',

        headers: {
          ...(
            init?.body
              ? {
                  'Content-Type':
                    'application/json',
                }
              : {}
          ),

          ...init?.headers,
        },
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }

  return response
}

export async function listarUsuarios():
  Promise<UsuarioAdministracao[]> {
  const response =
    await executar(
      '/api/usuarios',
    )

  return response.json()
}

export async function aprovarUsuario(
  id: string,
): Promise<void> {
  await executar(
    `/api/usuarios/${id}/aprovar`,
    {
      method: 'PUT',
    },
  )
}

export async function rejeitarUsuario(
  id: string,
): Promise<void> {
  await executar(
    `/api/usuarios/${id}/rejeitar`,
    {
      method: 'PUT',
    },
  )
}

export async function bloquearUsuario(
  id: string,
): Promise<void> {
  await executar(
    `/api/usuarios/${id}/bloquear`,
    {
      method: 'PUT',
    },
  )
}

export async function desbloquearUsuario(
  id: string,
): Promise<void> {
  await executar(
    `/api/usuarios/${id}/desbloquear`,
    {
      method: 'PUT',
    },
  )
}

export async function alterarAcessosUsuario(
  id: string,
  dados: AlterarAcessosUsuario,
): Promise<void> {
  await executar(
    `/api/usuarios/${id}/acessos`,
    {
      method: 'PUT',

      body:
        JSON.stringify(dados),
    },
  )
}