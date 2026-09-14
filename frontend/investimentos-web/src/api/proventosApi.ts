import type {
  Provento,
} from '../types/dashboard'

const API_URL =
  import.meta.env.VITE_API_URL ??
  'https://localhost:7237'

export interface SalvarProventoRequest {
  investidorId: string
  ticker: string
  tipo: string
  descricao?: string | null
  dataCom?: string | null
  dataPagamento: string
  quantidadeBase: number
  valorPorUnidade: number
  valorRecebido: number
}

async function obterMensagemErro(
  response: Response,
) {
  try {
    const dados =
      await response.json()

    return (
      dados.mensagem ??
      dados.title ??
      `Erro ${response.status}`
    )
  } catch {
    return `Erro ${response.status}`
  }
}

export async function listarProventos(
  investidorId: string,
): Promise<Provento[]> {
  const response =
    await fetch(
      `${API_URL}/api/proventos/${investidorId}`,
    )

  if (!response.ok) {
    throw new Error(
      await obterMensagemErro(
        response,
      ),
    )
  }

  return response.json()
}

export async function criarProvento(
  request: SalvarProventoRequest,
) {
  const response =
    await fetch(
      `${API_URL}/api/proventos`,
      {
        method: 'POST',

        headers: {
          'Content-Type':
            'application/json',
        },

        body: JSON.stringify(
          request,
        ),
      },
    )

  if (!response.ok) {
    throw new Error(
      await obterMensagemErro(
        response,
      ),
    )
  }
}

export async function atualizarProvento(
  id: string,
  request: Omit<
    SalvarProventoRequest,
    'investidorId'
  >,
) {
  const response =
    await fetch(
      `${API_URL}/api/proventos/${id}`,
      {
        method: 'PUT',

        headers: {
          'Content-Type':
            'application/json',
        },

        body: JSON.stringify(
          request,
        ),
      },
    )

  if (!response.ok) {
    throw new Error(
      await obterMensagemErro(
        response,
      ),
    )
  }
}

export async function excluirProvento(
  id: string,
) {
  const response =
    await fetch(
      `${API_URL}/api/proventos/${id}`,
      {
        method: 'DELETE',
      },
    )

  if (!response.ok) {
    throw new Error(
      await obterMensagemErro(
        response,
      ),
    )
  }
}