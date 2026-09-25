const apiUrl =
  import.meta.env.VITE_API_URL ??
  'https://localhost:7237'

export interface MetaAtivo {
  id: string
  investidorId: string
  investidor: string
  ativoId: string
  ticker: string
  nomeAtivo: string
  quantidadeAtual: number
  quantidadeDesejada: number
  quantidadeFaltante: number
  percentualAtingido: number
}

async function erro(response: Response) {
  const texto = await response.text()
  return texto || `HTTP ${response.status}`
}

export async function listarMetasAtivos():
  Promise<MetaAtivo[]> {
  const response = await fetch(
    `${apiUrl}/api/metas-ativos`,
    { cache: 'no-store' },
  )

  if (!response.ok) {
    throw new Error(await erro(response))
  }

  return response.json()
}

export async function salvarMetaAtivo(
  investidorId: string,
  ativoId: string,
  quantidadeDesejada: number,
): Promise<MetaAtivo> {
  const response = await fetch(
    `${apiUrl}/api/metas-ativos`,
    {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        investidorId,
        ativoId,
        quantidadeDesejada,
      }),
    },
  )

  if (!response.ok) {
    throw new Error(await erro(response))
  }

  return response.json()
}

export async function excluirMetaAtivo(
  id: string,
): Promise<void> {
  const response = await fetch(
    `${apiUrl}/api/metas-ativos/${id}`,
    { method: 'DELETE' },
  )

  if (!response.ok) {
    throw new Error(await erro(response))
  }
}
