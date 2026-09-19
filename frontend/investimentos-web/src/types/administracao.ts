export interface PosicaoInvestidorAtivoAdministracao {
  investidorId: string
  investidorNome: string
  quantidade: number
  valorAtual: number
}

export interface AtivoAdministracao {
  id: string
  ticker: string
  nome: string
  tipoAtivoId: number
  tipoAtivoCodigo: string
  tipoAtivoNome: string
  classeAtivoId: number
  classeAtivoCodigo: string
  classeAtivoNome: string
  quantidade: number
  valorAtual: number
  posicoesInvestidores:
    PosicaoInvestidorAtivoAdministracao[]
  cotacaoAtual?: number | null
  dataCotacao?: string | null
}

export interface HistoricoPatrimonioAdministracao {
  id: string
  dataReferencia: string
  valorCarteira: number
}

export interface DescontoFiscalAdministracao {
  id: string
  tipo: string
  dataPagamento: string
  valor: number
  descricao?: string | null
}

export interface InvestidorAdministracao {
  id: string
  nome: string
  saldoDisponivel?: number | null
  dataSaldo?: string | null
  patrimonioAtual?: number | null
  dataPatrimonio?: string | null
  historicoPatrimonio:
    HistoricoPatrimonioAdministracao[]
}

export interface ClasseAtivoAdministracao {
  id: number
  codigo: string
  nome: string
  ativo: boolean
}

export interface TipoAtivoAdministracao {
  id: number
  codigo: string
  nome: string
  ativo: boolean
  classeAtivoId: number
  classeAtivoCodigo: string
  classeAtivoNome: string
}

export interface TipoOperacaoAdministracao {
  id: number
  codigo: string
  nome: string
  ativo: boolean
}

export interface Administracao {
  ativos: AtivoAdministracao[]
  investidores: InvestidorAdministracao[]
  classesAtivo: ClasseAtivoAdministracao[]
  tiposAtivo: TipoAtivoAdministracao[]
  tiposOperacao: TipoOperacaoAdministracao[]
  descontosFiscais: DescontoFiscalAdministracao[]
}

export interface CriarAtivoAdministracao {
  ticker: string
  nome: string
  tipoAtivoId: number
  cotacao?: number | null
  dataCotacao?: string | null
}

export interface AtualizarAtivoAdministracao {
  nome: string
  tipoAtivoId: number
  cotacao?: number | null
  dataCotacao?: string | null
}

export interface AtualizarInvestidorAdministracao {
  nome: string
  saldoDisponivel: number
}

export interface CriarHistoricoPatrimonioAdministracao {
  dataReferencia: string
  valorCarteira: number
}

export interface AtualizarHistoricoPatrimonioAdministracao {
  dataReferencia: string
  valorCarteira: number
}