import type { Investidor } from '../types/investidor'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5001'

export async function listarInvestidores(): Promise<Investidor[]> {
  const response = await fetch(
    `${API_URL}/api/investidores`,
    { cache: 'no-store' },
  )

  if (!response.ok) {
    throw new Error(
      `Erro ao consultar investidores. Status: ${response.status}`,
    )
  }

  return response.json()
}
