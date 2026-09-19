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
      {
        cache: 'no-store',
      },
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

export async function excluirAtivo(
  id: string,
): Promise<void> {
  const response =
    await fetch(
      `${apiUrl}/api/admin/ativos/${id}`,
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

export interface SalvarDescontoFiscalAdministracao {
  dataPagamento: string
  valor: number
  descricao?: string | null
}

export async function atualizarDescontoFiscal(
  id: string,
  request: SalvarDescontoFiscalAdministracao,
): Promise<void> {
  const response =
    await fetch(
      `${apiUrl}/api/descontos-fiscais/${id}`,
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

export async function excluirDescontoFiscal(
  id: string,
): Promise<void> {
  const response =
    await fetch(
      `${apiUrl}/api/descontos-fiscais/${id}`,
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


export interface AtualizarParametroAdministracao {
  codigo: string
  nome: string
  ativo: boolean
}

export interface AtualizarTipoAtivoParametroAdministracao
  extends AtualizarParametroAdministracao {
  classeAtivoId: number
}

async function enviarParametro(
  url: string,
  method: 'PUT' | 'DELETE',
  body?: unknown,
): Promise<void> {
  const response =
    await fetch(
      `${apiUrl}${url}`,
      {
        method,
        headers:
          body === undefined
            ? undefined
            : {
                'Content-Type':
                  'application/json',
              },
        body:
          body === undefined
            ? undefined
            : JSON.stringify(body),
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }
}

export function atualizarClasseAtivo(
  id: number,
  request: AtualizarParametroAdministracao,
) {
  return enviarParametro(
    `/api/admin/classes-ativo/${id}`,
    'PUT',
    request,
  )
}

export function excluirClasseAtivo(
  id: number,
) {
  return enviarParametro(
    `/api/admin/classes-ativo/${id}`,
    'DELETE',
  )
}

export function atualizarTipoAtivoParametro(
  id: number,
  request: AtualizarTipoAtivoParametroAdministracao,
) {
  return enviarParametro(
    `/api/admin/tipos-ativo/${id}`,
    'PUT',
    request,
  )
}

export function excluirTipoAtivoParametro(
  id: number,
) {
  return enviarParametro(
    `/api/admin/tipos-ativo/${id}`,
    'DELETE',
  )
}

export function atualizarTipoOperacaoParametro(
  id: number,
  request: AtualizarParametroAdministracao,
) {
  return enviarParametro(
    `/api/admin/tipos-operacao/${id}`,
    'PUT',
    request,
  )
}

export function excluirTipoOperacaoParametro(
  id: number,
) {
  return enviarParametro(
    `/api/admin/tipos-operacao/${id}`,
    'DELETE',
  )
}
