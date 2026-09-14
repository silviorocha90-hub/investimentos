import type {
  Administracao,
  AtivoAdministracao,
  AtualizarAtivoAdministracao,
  AtualizarHistoricoPatrimonioAdministracao,
  AtualizarInvestidorAdministracao,
  CriarAtivoAdministracao,
  CriarHistoricoPatrimonioAdministracao,
} from '../types/administracao'

const apiUrl =
  import.meta.env.VITE_API_URL ??
  'https://localhost:7237'

async function lerErro(
  response: Response,
) {
  const texto =
    await response.text()

  if (!texto.trim()) {
    return `HTTP ${response.status}`
  }

  try {
    const json =
      JSON.parse(texto) as {
        detail?: string
        title?: string
      }

    return (
      json.detail ??
      json.title ??
      texto
    )
  } catch {
    return texto
  }
}

export async function obterAdministracao():
  Promise<Administracao> {
  const response =
    await fetch(
      `${apiUrl}/api/admin`,
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }

  return response.json()
}

export async function criarAtivo(
  request: CriarAtivoAdministracao,
): Promise<AtivoAdministracao> {
  const response =
    await fetch(
      `${apiUrl}/api/admin/ativos`,
      {
        method: 'POST',
        headers: {
          'Content-Type':
            'application/json',
        },
        body:
          JSON.stringify(request),
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }

  return response.json()
}

export async function atualizarAtivo(
  id: string,
  request: AtualizarAtivoAdministracao,
): Promise<AtivoAdministracao> {
  const response =
    await fetch(
      `${apiUrl}/api/admin/ativos/${id}`,
      {
        method: 'PUT',
        headers: {
          'Content-Type':
            'application/json',
        },
        body:
          JSON.stringify(request),
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }

  return response.json()
}

export async function atualizarInvestidor(
  id: string,
  request: AtualizarInvestidorAdministracao,
): Promise<void> {
  const response =
    await fetch(
      `${apiUrl}/api/admin/investidores/${id}`,
      {
        method: 'PUT',
        headers: {
          'Content-Type':
            'application/json',
        },
        body:
          JSON.stringify(request),
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }
}

export async function criarHistoricoPatrimonio(
  investidorId: string,
  request: CriarHistoricoPatrimonioAdministracao,
): Promise<void> {
  const response =
    await fetch(
      `${apiUrl}/api/admin/investidores/${investidorId}/historico-patrimonial`,
      {
        method: 'POST',
        headers: {
          'Content-Type':
            'application/json',
        },
        body:
          JSON.stringify(request),
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }
}

export async function atualizarHistoricoPatrimonio(
  id: string,
  request: AtualizarHistoricoPatrimonioAdministracao,
): Promise<void> {
  const response =
    await fetch(
      `${apiUrl}/api/admin/historico-patrimonial/${id}`,
      {
        method: 'PUT',
        headers: {
          'Content-Type':
            'application/json',
        },
        body:
          JSON.stringify(request),
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }
}

export async function excluirHistoricoPatrimonio(
  id: string,
): Promise<void> {
  const response =
    await fetch(
      `${apiUrl}/api/admin/historico-patrimonial/${id}`,
      {
        method: 'DELETE',
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }
}