export interface PosicaoAtivo {
  ticker: string
  nome: string
  tipoAtivoCodigo: string
  tipoAtivoNome: string
  quantidade: number
  precoMedio: number
  custoTotal: number
  resultadoRealizado: number
  dataPrimeiraCompra?: string | null

  precoAtual?: number | null
  valorAtual: number
  valorizacao: number
}

export interface Provento {
  id: string
  ticker: string
  tipo: string
  descricao?: string | null
  dataCom?: string | null
  dataPagamento: string
  quantidadeBase: number
  valorPorUnidade: number
  valorBruto: number
  valorRecebido: number
  impostoRetido: number

  valorLiquido: number
  irEfetivo: number
  aliquotaEfetiva: number
}

export interface OperacaoOpcao {
  id: string
  tickerAtivo: string
  tickerOpcao: string
  tipoOpcao: string
  natureza: string
  dataOperacao: string
  vencimento: string
  dataFinalizacao?: string | null
  strike: number
  contratos: number
  quantidade: number
  premioUnitario: number
  premioTotal: number
  taxas: number
  precoRecompraUnitario?: number | null
  valorRecompraTotal?: number | null
  valorExecucao?: number | null
  resultadoInformado?: number | null
  resultadoFinal?: number | null
  situacao: string

  valorAcaoAtual?: number | null
  resultadoBruto?: number | null
  ehDayTrade: boolean
  regimeTributario: string
  aliquotaIr: number
  irEstimado: number
  resultadoLiquido?: number | null
  percentualGanho?: number | null
  percentualGanhoPremio?: number | null
}

export interface DistribuicaoTipoAtivo {
  codigo: string
  nome: string
  valor: number
  percentual: number
}

export interface SaldoInvestidor {
  investidor: string
  valor: number
}

export interface Dashboard {
  valorAplicado: number
  resultadoRealizado: number
  resultadoRealizadoAcoes?: number
  totalProventos: number
  premioLiquidoOpcoes: number
  quantidadeAtivos: number
  quantidadeOpcoesAbertas: number

  patrimonioEstimado?: number
  caixaDisponivel?: number
  descontosFiscais?: number

  valorizacaoAtivos?: number

  proventosBrutos?: number
  irProventos?: number
  opcoesBrutas?: number
  irEstimadoOpcoes?: number

  rentabilidadeAno?: number
  entradasAno?: number
  saidasAno?: number

  saldosDisponiveis?: SaldoInvestidor[]
  distribuicaoPorTipo: DistribuicaoTipoAtivo[]

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

export interface OperacaoCarteira {
  id: string
  ativoId: string
  ticker: string
  nome: string
  tipoOperacao: string
  quantidade: number
  precoUnitario: number
  taxas: number
  data: string
  sequencia: number
}