export interface FiscalMes {
  ano: number
  mes: number
  investidorId?: string | null
  investidor: string
  resultadoComumOpcoes: number
  resultadoDayTradeOpcoes: number
  irEstimadoOpcoes: number
  darfPago: number
  diferencaEstimadoPago: number
  operacoesConsideradas: number
  pendencias: number
}

export interface FiscalResumo {
  resultadoComumOpcoes: number
  resultadoDayTradeOpcoes: number
  irEstimadoOpcoes: number
  darfPago: number
  diferencaEstimadoPago: number
  operacoesConsideradas: number
  pendencias: number
  meses: FiscalMes[]
  avisos: string[]
}

const apiUrl =
  import.meta.env.VITE_API_URL ??
  'https://localhost:7237'

export async function obterFiscal(
  investidorId?: string,
  ano?: number,
): Promise<FiscalResumo> {
  const params = new URLSearchParams()
  if (investidorId) params.set('investidorId', investidorId)
  if (ano) params.set('ano', String(ano))

  const response = await fetch(
    `${apiUrl}/api/fiscal?${params.toString()}`,
    { cache: 'no-store' },
  )

  if (!response.ok) {
    throw new Error(await response.text())
  }

  return response.json()
}
