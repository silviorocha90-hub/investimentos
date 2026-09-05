export interface TrendPoint {
  data: string
  carteira: number
}

export interface SimpleValueRow {
  label: string
  value: number
}

export interface QuoteRow {
  ativo: string
  tipo: string
  precoAtual: number
}

export interface AnalysisRow {
  nome: string
  investido: number
  rendimento: number
  percentual: number
  periodo: string
}

export interface MonthlyResultRow {
  mes: string
  ano: number
  lucroOperacoes: number
  prejuizoOperacoes: number
  darfsSprad: number
  resultado: string
  saldoFinalMes: number
  statusPagamento?: string
}

export interface ProventoSnapshotRow {
  data: string
  ativo: string
  descricao: string
  pessoa: string
  cotas: number
  valorPorCota: number
  valorRecebido: number
  tipo: string
}

export interface OpcaoSnapshotRow {
  data: string
  finalizada: string | null
  pessoa: string
  ativo: string
  tipo: string
  direcao: string
  qtde: number
  strike: number
  valorAcao: number
  vencimento: string
  venda: number
  recompraAtual: number
  myprofit: number
  status: string
  recebido: number
  resultadoFinal: number
  percentual: number
}

export const dashboardSnapshot = {
  referencia: '05/09/2026',
  investidores: ['Anna', 'Lucas', 'Mãe', 'Silene', 'Silvio', 'Simone'],
  rentabilidade: [
    { label: 'Carteira', value: 0.4387 },
    { label: 'CDI', value: 0.1407 },
    { label: 'IBOV', value: 0.2344 },
    { label: 'IFIX', value: 0.0645 },
    { label: 'IPCA', value: 0.0602 },
  ],
  valoresDisponiveis: [
    { label: 'Simone', value: 122361 },
    { label: 'Silvio', value: 59739.81 },
    { label: 'Anna', value: 9135 },
    { label: 'Lucas', value: 3223.56 },
    { label: 'Silene', value: 2.56 },
    { label: 'Mãe', value: 0 },
  ] satisfies SimpleValueRow[],
  timelinePorPessoa: {
    Silvio: [
      { data: '2025-03-30', carteira: 0 },
      { data: '2025-04-30', carteira: 45561.16 },
      { data: '2025-05-31', carteira: 73789.73 },
      { data: '2025-06-30', carteira: 103805.36 },
      { data: '2025-07-31', carteira: 107886.55 },
      { data: '2025-08-31', carteira: 107897.52 },
      { data: '2025-09-30', carteira: 121513.42 },
      { data: '2025-10-31', carteira: 123547.43 },
      { data: '2025-11-30', carteira: 131756.32 },
      { data: '2025-12-31', carteira: 148368.69 },
      { data: '2026-01-31', carteira: 148320.91 },
      { data: '2026-02-28', carteira: 158098.06 },
      { data: '2026-03-31', carteira: 167648.37 },
      { data: '2026-04-30', carteira: 172839.68 },
      { data: '2026-05-31', carteira: 230453.65 },
      { data: '2026-06-30', carteira: 269409.01 },
      { data: '2026-07-31', carteira: 281158.28 },
      { data: '2026-08-31', carteira: 300037.16 },
      { data: '2026-09-30', carteira: 302322.22 },
    ] satisfies TrendPoint[],
    Silene: [
      { data: '2026-03-31', carteira: 0 },
      { data: '2026-04-30', carteira: 43500 },
      { data: '2026-05-31', carteira: 57621.76 },
      { data: '2026-06-30', carteira: 63252.84 },
      { data: '2026-07-31', carteira: 63602.36 },
      { data: '2026-08-31', carteira: 69332.33 },
      { data: '2026-09-30', carteira: 69428.56 },
    ] satisfies TrendPoint[],
    Mãe: [
      { data: '2026-04-30', carteira: 0 },
      { data: '2026-05-31', carteira: 10395.84 },
      { data: '2026-06-30', carteira: 13531.05 },
      { data: '2026-07-31', carteira: 13593.06 },
      { data: '2026-08-31', carteira: 15120.02 },
      { data: '2026-09-30', carteira: 15786.5 },
    ] satisfies TrendPoint[],
    Lucas: [
      { data: '2026-06-30', carteira: 4000 },
      { data: '2026-07-31', carteira: 4130 },
      { data: '2026-08-30', carteira: 4348.64 },
      { data: '2026-09-30', carteira: 6585.62 },
    ] satisfies TrendPoint[],
    Simone: [
      { data: '2026-08-30', carteira: 120678 },
      { data: '2026-09-30', carteira: 121578 },
    ] satisfies TrendPoint[],
    Anna: [
      { data: '2026-08-30', carteira: 9050 },
      { data: '2026-09-30', carteira: 9230 },
    ] satisfies TrendPoint[],
  } as Record<string, TrendPoint[]>,
  quotes: [
    { ativo: 'BBAS3', tipo: 'Ação', precoAtual: 22.45 },
    { ativo: 'BBSE3', tipo: 'Ação', precoAtual: 41.69 },
    { ativo: 'CPLE3', tipo: 'Ação', precoAtual: 15.78 },
    { ativo: 'CXSE3', tipo: 'Ação', precoAtual: 19.77 },
    { ativo: 'EGIE3', tipo: 'Ação', precoAtual: 30.36 },
    { ativo: 'ITUB4', tipo: 'Ação', precoAtual: 41.92 },
    { ativo: 'LFTB11', tipo: 'ETF', precoAtual: 126.09 },
    { ativo: 'SAPR11', tipo: 'Ação', precoAtual: 35.51 },
    { ativo: 'TAEE11', tipo: 'Ação', precoAtual: 41.55 },
    { ativo: 'TIMS3', tipo: 'Ação', precoAtual: 18.97 },
    { ativo: 'VIVT3', tipo: 'Ação', precoAtual: 30.31 },
    { ativo: 'BTLG11', tipo: 'FII', precoAtual: 99 },
    { ativo: 'HGRU11', tipo: 'FII', precoAtual: 114.96 },
    { ativo: 'KNCR11', tipo: 'FII', precoAtual: 106.37 },
    { ativo: 'XPML11', tipo: 'FII', precoAtual: 103.39 },
    { ativo: 'PREV', tipo: 'Previdência', precoAtual: 132829.25 },
    { ativo: 'NEON', tipo: 'Renda Fixa', precoAtual: 299.55 },
    { ativo: 'ELETROBAS', tipo: 'Ação', precoAtual: 185.41 },
  ] satisfies QuoteRow[],
  analise: [
    {
      nome: 'Privilege DI',
      investido: 51782,
      rendimento: 2890.32,
      percentual: 0.0568,
      periodo: '23/03/26 a 20/08/26',
    },
    {
      nome: 'Diferenciado Crédito Privado Renda Fixa',
      investido: 8858.97,
      rendimento: 585.55,
      percentual: 0.087,
      periodo: '02/01/26 a 20/08/26',
    },
    {
      nome: 'Itaú Crédito Bancário Renda Fixa Crédito Privado',
      investido: 33196.9,
      rendimento: 2630.93,
      percentual: 0.0892,
      periodo: '02/01/26 a 20/08/26',
    },
    {
      nome: 'Tesouro Selic 2031',
      investido: 42243.99,
      rendimento: 6106.8,
      percentual: 0.1444,
      periodo: '02/03/26 a 20/08/26',
    },
    {
      nome: 'Ações de Dividendos',
      investido: 64000,
      rendimento: 4800,
      percentual: 0.075,
      periodo: 'ano',
    },
  ] satisfies AnalysisRow[],
  proventos: {
    total: 5032.01,
    porPessoa: [
      { label: 'Silvio', value: 4952.68 },
      { label: 'Lucas', value: 79.33 },
    ] satisfies SimpleValueRow[],
    porTipo: [
      { label: 'Rendimento', value: 2998.45 },
      { label: 'Dividendo', value: 1333.65 },
      { label: 'Juros', value: 699.91 },
    ] satisfies SimpleValueRow[],
    recentes: [
      {
        data: '2026-09-03',
        ativo: 'BBSE3',
        descricao: 'Ação',
        pessoa: 'Silvio',
        cotas: 364,
        valorPorCota: 1.9833,
        valorRecebido: 721.91,
        tipo: 'Dividendo',
      },
      {
        data: '2026-09-03',
        ativo: 'BBSE3',
        descricao: 'Ação',
        pessoa: 'Lucas',
        cotas: 40,
        valorPorCota: 1.9833,
        valorRecebido: 79.33,
        tipo: 'Dividendo',
      },
      {
        data: '2026-09-02',
        ativo: 'VALE3',
        descricao: 'Ação',
        pessoa: 'Silvio',
        cotas: 200,
        valorPorCota: 1.57,
        valorRecebido: 258.84,
        tipo: 'Juros',
      },
      {
        data: '2026-09-02',
        ativo: 'VALE3',
        descricao: 'Ação',
        pessoa: 'Silvio',
        cotas: 200,
        valorPorCota: 0.46,
        valorRecebido: 92.4,
        tipo: 'Juros',
      },
      {
        data: '2026-09-01',
        ativo: 'ITUB4',
        descricao: 'Ação',
        pessoa: 'Silvio',
        cotas: 100,
        valorPorCota: 0.02,
        valorRecebido: 1.5,
        tipo: 'Juros',
      },
      {
        data: '2026-08-21',
        ativo: 'CPTS11',
        descricao: 'FII',
        pessoa: 'Silvio',
        cotas: 100,
        valorPorCota: 0.09,
        valorRecebido: 9,
        tipo: 'Rendimento',
      },
    ] satisfies ProventoSnapshotRow[],
  },
  opcoes: {
    total: 61,
    encerradas: 46,
    exercidas: 15,
    porTipo: [
      { label: 'PUT', value: 33 },
      { label: 'CALL', value: 13 },
    ] satisfies SimpleValueRow[],
    porPessoa: [
      { label: 'Silvio', value: 43 },
      { label: 'Simone', value: 2 },
      { label: 'Anna', value: 1 },
    ] satisfies SimpleValueRow[],
    recentes: [
      {
        data: '2026-08-31',
        finalizada: '2026-09-03',
        pessoa: 'Simone',
        ativo: 'ALOSV253',
        tipo: 'PUT',
        direcao: 'Venda',
        qtde: 400,
        strike: 24.79,
        valorAcao: 29.08,
        vencimento: '2026-10-16',
        venda: 0.39,
        recompraAtual: 0.13,
        myprofit: 0,
        status: 'Encerrada',
        recebido: 156,
        resultadoFinal: 104,
        percentual: 0.6667,
      },
      {
        data: '2026-08-31',
        finalizada: '2026-09-03',
        pessoa: 'Anna',
        ativo: 'TIMSV179',
        tipo: 'PUT',
        direcao: 'Venda',
        qtde: 500,
        strike: 17.85,
        valorAcao: 19.32,
        vencimento: '2026-10-16',
        venda: 0.36,
        recompraAtual: 0.19,
        myprofit: 0,
        status: 'Encerrada',
        recebido: 180,
        resultadoFinal: 85,
        percentual: 0.4722,
      },
      {
        data: '2026-09-02',
        finalizada: '2026-09-03',
        pessoa: 'Silvio',
        ativo: 'BBASI224W1',
        tipo: 'CALL',
        direcao: 'Venda',
        qtde: 300,
        strike: 22.29,
        valorAcao: 22.17,
        vencimento: '2026-09-04',
        venda: 0.33,
        recompraAtual: 0.2,
        myprofit: 0,
        status: 'Encerrada',
        recebido: 99,
        resultadoFinal: 39,
        percentual: 0.3939,
      },
      {
        data: '2026-09-02',
        finalizada: '2026-09-03',
        pessoa: 'Silvio',
        ativo: 'PETRV460',
        tipo: 'PUT',
        direcao: 'Venda',
        qtde: 300,
        strike: 43.86,
        valorAcao: 47.68,
        vencimento: '2026-11-19',
        venda: 0.95,
        recompraAtual: 0.94,
        myprofit: 0,
        status: 'Encerrada',
        recebido: 285,
        resultadoFinal: 3,
        percentual: 0.0105,
      },
      {
        data: '2026-08-05',
        finalizada: '2026-08-14',
        pessoa: 'Silvio',
        ativo: 'PETRV384',
        tipo: 'PUT',
        direcao: 'Venda',
        qtde: 2000,
        strike: 37.3,
        valorAcao: 42,
        vencimento: '2026-10-16',
        venda: 0.64,
        recompraAtual: 0.55,
        myprofit: 176.98,
        status: 'Encerrada',
        recebido: 1280,
        resultadoFinal: 180,
        percentual: 0.1406,
      },
    ] satisfies OpcaoSnapshotRow[],
    top5: [
      {
        data: '2026-06-23',
        finalizada: '2026-07-20',
        pessoa: 'Silvio',
        ativo: 'VALEJ924',
        tipo: 'CALL',
        direcao: 'Venda',
        qtde: 5000,
        strike: 92.4,
        valorAcao: 74.07,
        vencimento: '2026-10-16',
        venda: 1.5,
        recompraAtual: 0.35,
        myprofit: 5737.63,
        status: 'Encerrada',
        recebido: 7500,
        resultadoFinal: 5750,
        percentual: 0.7667,
      },
      {
        data: '2026-07-21',
        finalizada: '2026-08-24',
        pessoa: 'Silvio',
        ativo: 'VALEM631',
        tipo: 'PUT',
        direcao: 'Venda',
        qtde: 5000,
        strike: 63.15,
        valorAcao: 77.49,
        vencimento: '2027-01-15',
        venda: 1.1,
        recompraAtual: 0.58,
        myprofit: 2589.59,
        status: 'Encerrada',
        recebido: 5500,
        resultadoFinal: 2600,
        percentual: 0.4727,
      },
      {
        data: '2026-05-22',
        finalizada: '2026-06-05',
        pessoa: 'Silvio',
        ativo: 'VALEI801',
        tipo: 'CALL',
        direcao: 'Venda',
        qtde: 1000,
        strike: 80.15,
        valorAcao: 82.45,
        vencimento: '2026-09-18',
        venda: 8.4,
        recompraAtual: 6.16,
        myprofit: 2173.23,
        status: 'Encerrada',
        recebido: 8400,
        resultadoFinal: 2240,
        percentual: 0.2667,
      },
      {
        data: '2026-07-22',
        finalizada: '2026-08-21',
        pessoa: 'Silvio',
        ativo: 'VIVTT358',
        tipo: 'PUT',
        direcao: 'Venda',
        qtde: 300,
        strike: 34.19,
        valorAcao: 29.74,
        vencimento: '2026-08-21',
        venda: 4.25,
        recompraAtual: 0,
        myprofit: 1275,
        status: 'Encerrada',
        recebido: 1275,
        resultadoFinal: 1275,
        percentual: 1,
      },
      {
        data: '2026-07-30',
        finalizada: '2026-08-21',
        pessoa: 'Silvio',
        ativo: 'CXSET23',
        tipo: 'PUT',
        direcao: 'Venda',
        qtde: 400,
        strike: 21.02,
        valorAcao: 18.59,
        vencimento: '2026-08-21',
        venda: 1.16,
        recompraAtual: 0,
        myprofit: 464,
        status: 'Encerrada',
        recebido: 464,
        resultadoFinal: 464,
        percentual: 1,
      },
    ] satisfies OpcaoSnapshotRow[],
    resultadoCapital: 11967.16,
  },
  resultadoMensal: [
    {
      mes: 'MAIO',
      ano: 2026,
      lucroOperacoes: 195.9,
      prejuizoOperacoes: 0,
      darfsSprad: 37.75,
      resultado: 'POSITIVO',
      saldoFinalMes: 158.15,
      statusPagamento: 'PAGO',
    },
    {
      mes: 'JUNHO',
      ano: 2026,
      lucroOperacoes: 3232.4,
      prejuizoOperacoes: 0,
      darfsSprad: 490.78,
      resultado: 'POSITIVO',
      saldoFinalMes: 2741.62,
      statusPagamento: 'PAGO',
    },
    {
      mes: 'JULHO',
      ano: 2026,
      lucroOperacoes: 6257.26,
      prejuizoOperacoes: 0,
      darfsSprad: 943.41,
      resultado: 'POSITIVO',
      saldoFinalMes: 5313.85,
      statusPagamento: 'PAGO',
    },
    {
      mes: 'AGOSTO',
      ano: 2026,
      lucroOperacoes: 4415.93,
      prejuizoOperacoes: 0,
      darfsSprad: 662.39,
      resultado: 'POSITIVO',
      saldoFinalMes: 3753.54,
    },
  ] satisfies MonthlyResultRow[],
  irpf: {
    totalVendas: 22117.87,
    lucroLiquido: 607.92,
    aliquota: 0.15,
    irDevido: 91.19,
    codigo: '6015',
    periodo: '09/2025',
  },
} as const
