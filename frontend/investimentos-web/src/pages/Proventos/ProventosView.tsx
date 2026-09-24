import {
  useEffect,
  useMemo,
  useState,
} from 'react'

import {
  PageHeader,
} from '../../components/PageHeader'

import type {
  Provento,
} from '../../types/dashboard'

import type {
  Investidor,
} from '../../types/investidor'

import {
  atualizarProvento,
  criarProventoRateado,
  excluirProvento,
  listarProventos,
  type SalvarProventoRequest,
  type SalvarProventoTotalRequest,
} from '../../api/proventosApi'

import {
  ProventosResumo,
} from './components/ProventosResumo'

import {
  ProventosGraficos,
} from './components/ProventosGraficos'

import {
  ProventosTabela,
} from './components/ProventosTabela'

import {
  ProventoModal,
} from './components/ProventoModal'

import './Proventos.css'

interface ProventosViewProps {
  investidores:
    readonly Investidor[]

  selectedInvestor: string

  onSelectInvestor: (
    nome: string,
  ) => void

  modo?: 'consulta' | 'administracao'
}

const ITENS_POR_PAGINA =
  10

export function ProventosView({
  investidores,
  selectedInvestor,
  onSelectInvestor,
  modo = 'consulta',
}: ProventosViewProps) {
  const modoAdministracao =
    modo === 'administracao'

  const [
    investidoresComProventos,
    setInvestidoresComProventos,
  ] = useState<Investidor[]>(
    modoAdministracao
      ? [...investidores]
      : [],
  )

  const [
    proventos,
    setProventos,
  ] = useState<Provento[]>(
    [],
  )

  const [
    carregando,
    setCarregando,
  ] = useState(true)

  const [
    erro,
    setErro,
  ] = useState<
    string | null
  >(null)

  const [
    modalAberto,
    setModalAberto,
  ] = useState(false)

  const [
    proventoEditando,
    setProventoEditando,
  ] = useState<
    Provento | null
  >(null)

  const [
    salvando,
    setSalvando,
  ] = useState(false)

  const [
    erroModal,
    setErroModal,
  ] = useState<
    string | null
  >(null)

  const [
    ano,
    setAno,
  ] = useState(
    new Date()
      .getFullYear(),
  )

  const [
    tipo,
    setTipo,
  ] = useState(
    'TODOS',
  )

  const [
    ticker,
    setTicker,
  ] = useState(
    'TODOS',
  )

  const [
    busca,
    setBusca,
  ] = useState('')

  const [
    pagina,
    setPagina,
  ] = useState(1)

  const [
    investidorConsulta,
    setInvestidorConsulta,
  ] = useState(
    modoAdministracao ? selectedInvestor : 'TOTAL',
  )

  const investidorAtivo =
    modoAdministracao
      ? selectedInvestor
      : investidorConsulta

  const investidor =
    investidores.find(
      (item) =>
        item.nome ===
        investidorAtivo,
    )

  async function carregar() {
    if (!investidor && (investidorAtivo !== 'TOTAL' || modoAdministracao)) {
      setProventos([])
      setCarregando(false)
      return
    }

    try {
      setCarregando(true)
      setErro(null)

      const dados =
        investidorAtivo === 'TOTAL' && !modoAdministracao
          ? (
              await Promise.all(
                investidoresComProventos.map((item) =>
                  listarProventos(item.id),
                ),
              )
            ).flat()
          : await listarProventos(
              investidor!.id,
            )

      setProventos(
        dados,
      )
    } catch (error) {
      console.error(
        error,
      )

      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível carregar os proventos.',
      )

      setProventos([])
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    void carregar()
  }, [
    investidor?.id,
    investidorAtivo,
    modoAdministracao,
    investidoresComProventos,
  ])

  useEffect(() => {
    if (modoAdministracao) {
      setInvestidoresComProventos(
        [...investidores],
      )
      return
    }

    let ativo = true

    void Promise.all(
      investidores.map(
        async (item) => ({
          investidor: item,
          proventos:
            await listarProventos(
              item.id,
            ),
        }),
      ),
    ).then(
      (resultados) => {
        if (!ativo) {
          return
        }

        setInvestidoresComProventos(
          resultados
            .filter(
              (item) =>
                item.proventos.length >
                0,
            )
            .map(
              (item) =>
                item.investidor,
            ),
        )
      },
    ).catch(
      (error) =>
        console.error(
          'Não foi possível filtrar investidores com proventos.',
          error,
        ),
    )

    return () => {
      ativo = false
    }
  }, [
    investidores,
    modoAdministracao,
  ])

  useEffect(() => {
    setPagina(1)
  }, [
    ano,
    tipo,
    ticker,
    busca,
  ])

  const anos =
    useMemo(() => {
      const encontrados =
        new Set<number>()

      encontrados.add(
        new Date()
          .getFullYear(),
      )

      proventos.forEach(
        (item) =>
          encontrados.add(
            new Date(
              item.dataPagamento,
            ).getFullYear(),
          ),
      )

      return Array.from(
        encontrados,
      ).sort(
        (a, b) =>
          b - a,
      )
    }, [
      proventos,
    ])

  const tickers =
    useMemo(
      () =>
        Array.from(
          new Set(
            proventos.map(
              (item) =>
                item.ticker,
            ),
          ),
        ).sort(),
      [proventos],
    )

  const proventosAno =
    useMemo(
      () =>
        proventos.filter(
          (item) =>
            new Date(
              item.dataPagamento,
            ).getFullYear() ===
            ano,
        ),
      [
        proventos,
        ano,
      ],
    )

  const totalAno =
    proventosAno.reduce(
      (total, item) =>
        total +
        item.valorRecebido,
      0,
    )

  const agora =
    new Date()

  const totalMes =
    proventosAno
      .filter(
        (item) => {
          const data =
            new Date(
              item.dataPagamento,
            )

          return (
            ano ===
              agora.getFullYear() &&
            data.getMonth() ===
              agora.getMonth()
          )
        },
      )
      .reduce(
        (total, item) =>
          total +
          item.valorRecebido,
        0,
      )

  const mesesDecorridos =
    ano ===
    agora.getFullYear()
      ? agora.getMonth() +
        1
      : 12

  const mediaMensal =
    mesesDecorridos > 0
      ? totalAno /
        mesesDecorridos
      : 0

  const maiorPagador =
    useMemo(() => {
      const mapa =
        new Map<
          string,
          number
        >()

      proventosAno.forEach(
        (item) => {
          mapa.set(
            item.ticker,
            (
              mapa.get(
                item.ticker,
              ) ?? 0
            ) +
              item.valorRecebido,
          )
        },
      )

      const primeiro =
        Array.from(
          mapa.entries(),
        ).sort(
          (a, b) =>
            b[1] -
            a[1],
        )[0]

      if (!primeiro) {
        return undefined
      }

      return {
        ticker:
          primeiro[0],

        valor:
          primeiro[1],
      }
    }, [
      proventosAno,
    ])

  const filtrados =
    useMemo(() => {
      const texto =
        busca
          .trim()
          .toUpperCase()

      return proventos
        .filter(
          (item) =>
            new Date(
              item.dataPagamento,
            ).getFullYear() ===
            ano,
        )
        .filter(
          (item) =>
            tipo ===
              'TODOS' ||
            item.tipo ===
              tipo,
        )
        .filter(
          (item) =>
            ticker ===
              'TODOS' ||
            item.ticker ===
              ticker,
        )
        .filter(
          (item) =>
            !texto ||
            item.ticker
              .toUpperCase()
              .includes(
                texto,
              ) ||
            item.tipo
              .toUpperCase()
              .includes(
                texto,
              ),
        )
        .sort(
          (a, b) =>
            b.dataPagamento
              .localeCompare(
                a.dataPagamento,
              ),
        )
    }, [
      proventos,
      ano,
      tipo,
      ticker,
      busca,
    ])

  function novo() {
    if (
      !modoAdministracao
    ) {
      return
    }

    setProventoEditando(
      null,
    )

    setErroModal(
      null,
    )

    setModalAberto(
      true,
    )
  }

  function editar(
    item: Provento,
  ) {
    if (
      !modoAdministracao
    ) {
      return
    }

    setProventoEditando(
      item,
    )

    setErroModal(
      null,
    )

    setModalAberto(
      true,
    )
  }

  async function salvar(
    request:
      SalvarProventoRequest |
      SalvarProventoTotalRequest,
  ) {
    if (
      !modoAdministracao
    ) {
      return
    }

    try {
      setSalvando(true)
      setErroModal(null)

      if (
        proventoEditando
      ) {
        const {
          investidorId:
            _investidorId,
          ...dados
        } = request

        await atualizarProvento(
          proventoEditando.id,
          dados,
        )
      } else {
        await criarProventoRateado(
          request as SalvarProventoTotalRequest,
        )
      }

      setModalAberto(
        false,
      )

      setProventoEditando(
        null,
      )

      await carregar()
    } catch (error) {
      console.error(
        error,
      )

      setErroModal(
        error instanceof Error
          ? error.message
          : 'Não foi possível salvar o provento.',
      )
    } finally {
      setSalvando(false)
    }
  }

  async function excluir(
    item: Provento,
  ) {
    if (
      !modoAdministracao
    ) {
      return
    }

    const confirmou =
      window.confirm(
        `Excluir o provento de ${item.ticker} no valor recebido de ${item.valorRecebido.toLocaleString(
          'pt-BR',
          {
            style:
              'currency',
            currency:
              'BRL',
          },
        )}?`,
      )

    if (!confirmou) {
      return
    }

    try {
      setErro(null)

      await excluirProvento(
        item.id,
      )

      await carregar()
    } catch (error) {
      console.error(
        error,
      )

      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível excluir o provento.',
      )
    }
  }

  return (
    <section className="portfolio-view proventos-page">
      <PageHeader
        titulo={
          modoAdministracao
            ? 'Administração de Proventos'
            : 'Proventos'
        }
        investidores={
          investidoresComProventos
        }
        selectedInvestor={
          investidorAtivo
        }
        onSelectInvestor={(nome) => {
          if (modoAdministracao) {
            onSelectInvestor(nome)
          } else {
            setInvestidorConsulta(nome)
            if (nome !== 'TOTAL') {
              onSelectInvestor(nome)
            }
          }
        }}
        incluirTodos={
          !modoAdministracao
        }
        rotuloTodos="Todos"
      />

      {erro ? (
        <div className="proventos-error">
          {erro}
        </div>
      ) : null}

      {carregando ? (
        <article className="panel proventos-loading">
          <div className="loader" />

          <span>
            Carregando proventos...
          </span>
        </article>
      ) : (
        <>
          {!modoAdministracao ? (
            <>
              <ProventosResumo
                totalAno={
                  totalAno
                }
                totalMes={
                  totalMes
                }
                mediaMensal={
                  mediaMensal
                }
                maiorPagador={
                  maiorPagador
                }
              />

              <ProventosGraficos
                proventos={
                  proventos
                }
                ano={
                  ano
                }
              />
            </>
          ) : null}

          <article className="panel proventos-history">
            <header className="proventos-history-header">
              <div>
                <span>
                  {modoAdministracao
                    ? 'Manutenção'
                    : 'Movimentações'}
                </span>

                <strong>
                  Histórico de proventos
                </strong>
              </div>

              {modoAdministracao ? (
                <button
                  type="button"
                  className="proventos-new-button"
                  onClick={
                    novo
                  }
                >
                  + Novo provento
                </button>
              ) : null}
            </header>

            <div className="proventos-filters">
              <label>
                <span>
                  Ano
                </span>

                <select
                  value={
                    ano
                  }
                  onChange={(
                    event,
                  ) =>
                    setAno(
                      Number(
                        event
                          .target
                          .value,
                      ),
                    )
                  }
                >
                  {anos.map(
                    (item) => (
                      <option
                        key={
                          item
                        }
                        value={
                          item
                        }
                      >
                        {
                          item
                        }
                      </option>
                    ),
                  )}
                </select>
              </label>

              <label>
                <span>
                  Tipo
                </span>

                <select
                  value={
                    tipo
                  }
                  onChange={(
                    event,
                  ) =>
                    setTipo(
                      event
                        .target
                        .value,
                    )
                  }
                >
                  <option value="TODOS">
                    Todos
                  </option>

                  <option value="DIVIDENDO">
                    Dividendo
                  </option>

                  <option value="JCP">
                    JCP
                  </option>

                  <option value="RENDIMENTO">
                    Rendimento
                  </option>
                </select>
              </label>

              <label>
                <span>
                  Ativo
                </span>

                <select
                  value={
                    ticker
                  }
                  onChange={(
                    event,
                  ) =>
                    setTicker(
                      event
                        .target
                        .value,
                    )
                  }
                >
                  <option value="TODOS">
                    Todos
                  </option>

                  {tickers.map(
                    (item) => (
                      <option
                        key={
                          item
                        }
                        value={
                          item
                        }
                      >
                        {
                          item
                        }
                      </option>
                    ),
                  )}
                </select>
              </label>

              <label className="proventos-search">
                <span>
                  Buscar
                </span>

                <input
                  value={
                    busca
                  }
                  onChange={(
                    event,
                  ) =>
                    setBusca(
                      event
                        .target
                        .value,
                    )
                  }
                  placeholder="Ticker ou tipo..."
                />
              </label>
            </div>

            <ProventosTabela
              proventos={
                filtrados
              }
              pagina={
                pagina
              }
              itensPorPagina={
                ITENS_POR_PAGINA
              }
              onPaginaChange={
                setPagina
              }
              modoAdministracao={
                modoAdministracao
              }
              onEditar={
                modoAdministracao
                  ? editar
                  : undefined
              }
              onExcluir={
                modoAdministracao
                  ? excluir
                  : undefined
              }
            />
          </article>
        </>
      )}

      {modoAdministracao &&
      modalAberto &&
      investidor ? (
        <ProventoModal
          provento={
            proventoEditando
          }
          investidorId={
            investidor.id
          }
          tickers={
            tickers
          }
          salvando={
            salvando
          }
          erro={
            erroModal
          }
          onClose={() => {
            if (
              !salvando
            ) {
              setModalAberto(
                false,
              )
            }
          }}
          onSalvar={
            salvar
          }
        />
      ) : null}
    </section>
  )
}