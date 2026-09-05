import type { Dashboard, EvolucaoInvestidor, OperacaoCarteira } from '../types/dashboard'

const API_URL = 'https://localhost:7237'

export async function obterDashboard(
    investidorId: string,
): Promise<Dashboard> {
    const response = await fetch(
        `${API_URL}/api/dashboard/${investidorId}`,
    )

    if (!response.ok) {
        throw new Error(
            `Erro ao consultar o dashboard. Status: ${response.status}`,
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
            `Erro ao consultar o dashboard consolidado. Status: ${response.status}`,
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
            `Erro ao consultar a carteira do investidor. Status: ${response.status}`,
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
            `Erro ao consultar a evolução da carteira. Status: ${response.status}`,
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
            `Erro ao consultar as operações. Status: ${response.status}`,
        )
    }

    return response.json()
}

export async function atualizarOperacao(
    id: string,
    operacao: Pick<OperacaoCarteira, 'data' | 'quantidade' | 'precoUnitario' | 'taxas'>,
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
            `Erro ao atualizar a operação. Status: ${response.status}`,
        )
    }
}
