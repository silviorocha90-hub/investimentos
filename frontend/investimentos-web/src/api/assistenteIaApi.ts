export interface RelatorioIa {
  id: string
  dataReferencia: string
  dataGeracao: string
  conteudo: string
  modelo: string
}

const API_URL =
  import.meta.env.VITE_API_URL ??
  'https://localhost:7237'

async function lerErro(
  response: Response,
) {
  const texto =
    await response.text()

  if (!texto) {
    return 'Falha ao acessar o Assistente IA.'
  }

  try {
    const json =
      JSON.parse(texto) as {
        detail?: string
      }

    return json.detail ?? texto
  } catch {
    return texto
  }
}

export async function listarRelatoriosIa():
  Promise<RelatorioIa[]> {
  const response =
    await fetch(
      `${API_URL}/api/assistente-ia/relatorios`,
      {
        credentials: 'include',
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }

  return response.json()
}

export async function gerarRelatorioIa():
  Promise<RelatorioIa> {
  const response =
    await fetch(
      `${API_URL}/api/assistente-ia/gerar`,
      {
        method: 'POST',
        credentials: 'include',
      },
    )

  if (!response.ok) {
    throw new Error(
      await lerErro(response),
    )
  }

  return response.json()
}
