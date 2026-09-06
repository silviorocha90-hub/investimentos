import type { Dashboard, EvolucaoInvestidor, OperacaoCarteira } from '../types/dashboard'

const API_URL = import.meta.env.VITE_API_URL ?? 'https://localhost:7237'

async function obterMensagemErro(response: Response): Promise<string> {
    try {
        const texto = await response.text()

        if (texto) {
            return texto
        }
    } catch {
        // Mantém a mensagem padrão abaixo.
    }

    return `Status: ${response.status}`
}

export async function obterDashboard(
    investidorId: string,
): Promise<Dashboard> {
    const response = await fetch(
        `${API_URL}/api/dashboard/${investidorId}`,
        { cache: 'no-store' },
    )

    if (!response.ok) {
        throw new Error(
            `Erro ao consultar o dashboard. ${await obterMensagemErro(response)}`,
        )
    }

    return response.json()
}

export async function obterDashboardConsolidado(): Promise<Dashboard> {
    const response = await fetch(
        `${API_URL}/api/dashboard`,
        { cache: 'no-store' },
    )

    if (!response.ok) {
        throw new Error(
            `Erro ao consultar o dashboard consolidado. ${await obterMensagemErro(response)}`,
        )
    }

    return response.json()
}

export async function obterDashboardPorInvestidor(
    investidorId: string,
): Promise<Dashboard> {
    const response = await fetch(
        `${API_URL}/api/dashboard/${investidorId}`,
        { cache: 'no-store' },
    )

    if (!response.ok) {
        throw new Error(
            `Erro ao consultar a carteira do investidor. ${await obterMensagemErro(response)}`,
        )
    }

    return response.json()
}

export async function obterEvolucaoConsolidada(): Promise<EvolucaoInvestidor[]> {
    const response = await fetch(
        `${API_URL}/api/dashboard/evolucao`,
        { cache: 'no-store' },
    )

    if (!response.ok) {
        throw new Error(
            `Erro ao consultar a evolução da carteira. ${await obterMensagemErro(response)}`,
        )
    }

    return response.json()
}

export async function obterOperacoes(
    investidorId: string,
): Promise<OperacaoCarteira[]> {
    const response = await fetch(
        `${API_URL}/api/operacoes/${investidorId}`,
        { cache: 'no-store' },
    )

    if (!response.ok) {
        throw new Error(
            `Erro ao consultar as operações. ${await obterMensagemErro(response)}`,
        )
    }

    return response.json()
}

export async function atualizarOperacao(
    id: string,
    operacao: Pick<
        OperacaoCarteira,
        'data' | 'quantidade' | 'precoUnitario' | 'taxas'
    >,
): Promise<void> {
    const response = await fetch(
        `${API_URL}/api/operacoes/${id}`,
        {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                data: operacao.data,
                quantidade: operacao.quantidade,
                precoUnitario: operacao.precoUnitario,
                taxas: operacao.taxas,
            }),
        },
    )

    if (!response.ok) {
        throw new Error(
            `Erro ao atualizar a operação. ${await obterMensagemErro(response)}`,
        )
    }
}

export async function criarOperacao(
    investidorId: string,
    operacao: {
        data: string | Date
        ticker: string
        tipoOperacaoCodigo: string
        quantidade: number
        precoUnitario: number
        taxas: number
    },
): Promise<{ id: string }> {
    const data =
        typeof operacao.data === 'string'
            ? operacao.data
            : operacao.data.toISOString().split('T')[0]

    const response = await fetch(
        `${API_URL}/api/operacoes`,
        {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                data,
                investidorId,
                ticker: operacao.ticker,
                tipoOperacaoCodigo: operacao.tipoOperacaoCodigo,
                quantidade: operacao.quantidade,
                precoUnitario: operacao.precoUnitario,
                taxas: operacao.taxas,
            }),
        },
    )

    if (!response.ok) {
        throw new Error(
            `Erro ao criar a operação. ${await obterMensagemErro(response)}`,
        )
    }

    return response.json()
}

export async function excluirOperacao(id: string): Promise<void> {
    const response = await fetch(
        `${API_URL}/api/operacoes/${id}`,
        {
            method: 'DELETE',
        },
    )

    if (!response.ok) {
        throw new Error(
            `Erro ao excluir a operação. ${await obterMensagemErro(response)}`,
        )
    }
}