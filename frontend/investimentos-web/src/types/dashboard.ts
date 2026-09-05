export interface PosicaoAtivo {
	ticker: string
	nome: string
	quantidade: number
	precoMedio: number
	custoTotal: number
	resultadoRealizado: number
	dataPrimeiraCompra?: string
}

export interface Provento {
	id: string
	ticker: string
	tipo: string
	dataCom: string
	dataPagamento: string
	quantidadeBase: number
	valorPorUnidade: number
	valorTotal: number
}

export interface OperacaoOpcao {
	id: string
	tickerAtivo: string
	tickerOpcao: string
	tipoOpcao: string
	natureza: string
	dataOperacao: string
	vencimento: string
	strike: number
	contratos: number
	quantidade: number
	premioUnitario: number
		premioTotal: number
		 taxas: number
	resultadoInformado?: number
		 situacao: string
}

export interface Dashboard {
	valorAplicado: number
	resultadoRealizado: number
	totalProventos: number
	premioLiquidoOpcoes: number
	quantidadeAtivos: number
	quantidadeOpcoesAbertas: number
	patrimonioEstimado?: number
	caixaDisponivel?: number
	descontosFiscais?: number
	saldosDisponiveis?: SaldoInvestidor[]
	posicoes: PosicaoAtivo[]
	proventos: Provento[]
	opcoes: OperacaoOpcao[]
}

export interface EvolucaoPonto {
	data: string
	carteira: number
}

export interface EvolucaoInvestidor {
	investidor: string
	pontos: EvolucaoPonto[]
}

export interface SaldoInvestidor {
	investidor: string
	valor: number
}
