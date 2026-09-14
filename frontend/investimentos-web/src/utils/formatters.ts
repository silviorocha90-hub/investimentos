let ocultarValoresMonetarios = false

export function definirOcultacaoValores(
  ocultar: boolean,
) {
  ocultarValoresMonetarios = ocultar
}

export function formatarMoeda(
  valor: number | null | undefined,
) {
  if (ocultarValoresMonetarios) {
    return '••••••'
  }

  if (
    valor == null ||
    Number.isNaN(valor)
  ) {
    return '--'
  }

  return valor.toLocaleString(
    'pt-BR',
    {
      style: 'currency',
      currency: 'BRL',
    },
  )
}

export function formatarDataCurta(
  data: string,
) {
  return new Date(
    `${data.slice(0, 10)}T00:00:00`,
  ).toLocaleDateString(
    'pt-BR',
    {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    },
  )
}

export function formatarNumeroInteiro(
  valor: number,
) {
  return valor.toLocaleString(
    'pt-BR',
    {
      maximumFractionDigits: 2,
    },
  )
}

export function formatarPercentual(
  valor: number | null | undefined,
) {
  if (
    valor == null ||
    Number.isNaN(valor)
  ) {
    return '--'
  }

  return valor.toLocaleString(
    'pt-BR',
    {
      style: 'percent',
      maximumFractionDigits: 1,
    },
  )
}

export function formatarMilhares(
  valor: number,
) {
  if (Math.abs(valor) < 1000) {
    return valor.toLocaleString(
      'pt-BR',
      {
        maximumFractionDigits: 1,
      },
    )
  }

  return `${(valor / 1000).toLocaleString(
    'pt-BR',
    {
      maximumFractionDigits: 1,
    },
  )}K`
}

export function formatarMilharesInteiros(
  valor: number,
) {
  if (Math.abs(valor) < 1000) {
    return valor.toLocaleString(
      'pt-BR',
      {
        maximumFractionDigits: 0,
      },
    )
  }

  return `${Math.round(
    valor / 1000,
  ).toLocaleString('pt-BR')}K`
}

export function formatarMesCurto(
  data: string,
) {
  const dataNormalizada =
    data.includes('T')
      ? data
      : `${data}T00:00:00`

  return new Date(dataNormalizada)
    .toLocaleDateString(
      'pt-BR',
      {
        month: 'short',
        year: '2-digit',
      },
    )
    .replace(/\s*de\s*/i, ' ')
    .trim()
}

export function normalizarDataEvolucao(
  data: string,
) {
  return data.slice(0, 10)
}

export function ordenarDecrescente(
  a: { value: number },
  b: { value: number },
) {
  return b.value - a.value
}

const CORES_INVESTIDORES:
Record<string, string> = {
  silvio: '#4ea1ff',
  anna: '#ff6fae',
  lucas: '#63e6c8',
  simone: '#a78bfa',
  silene: '#eaff00',
  mae: '#ff9f43',
}

function normalizarNomeChave(
  nome: string,
) {
  return nome
    .normalize('NFD')
    .replace(
      /[\u0300-\u036f]/g,
      '',
    )
    .trim()
    .toLowerCase()
}

export function obterCorInvestidor(
  nome: string,
) {
  return (
    CORES_INVESTIDORES[
      normalizarNomeChave(nome)
    ] ?? '#8da7ff'
  )
}