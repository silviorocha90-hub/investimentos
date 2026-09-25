const API_URL =
  import.meta.env.VITE_API_URL ??
  'https://localhost:7237'

export type TipoMovimentacaoFinanceira =
  | 'APORTE'
  | 'RETIRADA'

export interface MovimentacaoFinanceira {
  id: string
  investidorId: string
  investidor: string
  data: string
  tipo: TipoMovimentacaoFinanceira
  valor: number
  descricao?: string | null
}

export interface SalvarMovimentacaoFinanceira {
  investidorId: string
  data: string
  tipo: TipoMovimentacaoFinanceira
  valor: number
  descricao?: string | null
}

async function validar(response: Response) {
  if (response.ok) return

  try {
    const dados = await response.json()
    throw new Error(
      dados.detail ??
      dados.mensagem ??
      dados.title ??
      `Erro ${response.status}`,
    )
  } catch (error) {
    if (error instanceof Error &&
        !error.message.startsWith('Unexpected')) {
      throw error
    }
    throw new Error(`Erro ${response.status}`)
  }
}

export async function listarMovimentacoesFinanceiras(
  ano: number,
): Promise<MovimentacaoFinanceira[]> {
  const response = await fetch(
    `${API_URL}/api/movimentacoes-financeiras?ano=${ano}`,
  )
  await validar(response)
  return response.json()
}

export async function criarMovimentacaoFinanceira(
  request: SalvarMovimentacaoFinanceira,
) {
  const response = await fetch(
    `${API_URL}/api/movimentacoes-financeiras`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    },
  )
  await validar(response)
}

export async function atualizarMovimentacaoFinanceira(
  id: string,
  request: SalvarMovimentacaoFinanceira,
) {
  const response = await fetch(
    `${API_URL}/api/movimentacoes-financeiras/${id}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    },
  )
  await validar(response)
}

export async function excluirMovimentacaoFinanceira(
  id: string,
) {
  const response = await fetch(
    `${API_URL}/api/movimentacoes-financeiras/${id}`,
    { method: 'DELETE' },
  )
  await validar(response)
}
