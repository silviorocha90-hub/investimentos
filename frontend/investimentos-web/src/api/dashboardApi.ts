import type { Dashboard, EvolucaoInvestidor } from '../types/dashboard'

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
