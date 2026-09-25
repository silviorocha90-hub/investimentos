import {
  useEffect,
  useMemo,
  useState,
} from 'react'

import type {
  Dashboard,
  OperacaoOpcao,
} from '../../types/dashboard'

import type {
  Investidor,
} from '../../types/investidor'

import {
  OpcoesGraficos,
} from './components/OpcoesGraficos'

import {
  OpcoesFiltros,
} from './components/OpcoesFiltros'

import {
  OpcoesTabela,
} from './components/OpcoesTabela'

import {
  CallsDisponiveis,
  obterPosicoesConsulta,
} from './components/CallsDisponiveis'

import {
  OpcaoModal,
  type NovaOpcaoForm,
} from './components/OpcaoModal'

import './Opcoes.css'

interface OpcoesViewProps {
  investidores:
    readonly Investidor[]

  carteiras:
    ReadonlyArray<{
      nome: string
      dashboard: Dashboard
    }>

  selectedInvestor: string

  onSelectInvestor: (
    nome: string,
  ) => void

  modo?: 'consulta' | 'administracao'
}

async function lerErroApi(
  response: Response,
) {
  const texto =
    await response.text()

  if (!texto.trim()) {
    return `HTTP ${response.status}`
  }

  try {
    const json =
      JSON.parse(texto) as {
        detail?: string
        title?: string
      }

    return (
      json.detail ??
      json.title ??
      texto
    )
  } catch {
    return texto
  }
}

export function OpcoesView({
  investidores,
  carteiras,
  selectedInvestor,
  onSelectInvestor,
  modo = 'consulta',
}: OpcoesViewProps) {
  const modoAdministracao =
    modo === 'administracao'

  const investidoresComOpcoes =
    useMemo(
      () => {
        if (modoAdministracao) {
          return investidores
        }

        return investidores.filter(
          (investidor) => {
            const carteiraInvestidor =
              carteiras.find(
                (item) =>
                  item.nome.trim().toLocaleLowerCase('pt-BR') ===
                  investidor.nome.trim().toLocaleLowerCase('pt-BR'),
              )

            return (
              carteiraInvestidor?.dashboard.opcoes ??
              []
            ).length > 0
          },
        )
      },
      [
        investidores,
        carteiras,
        modoAdministracao,
      ],
    )

  const apiUrl =
    import.meta.env.VITE_API_URL ??
    'https://localhost:7237'

  const [
    tickerSelecionado,
    setTickerSelecionado,
  ] = useState('TODOS')

  const [
    tipoSelecionado,
    setTipoSelecionado,
  ] = useState('TODOS')

  const [
    naturezaSelecionada,
    setNaturezaSelecionada,
  ] = useState('TODOS')

  const [
    statusSelecionado,
    setStatusSelecionado,
  ] = useState('TODOS')

  const [
    vencimentoSelecionado,
    setVencimentoSelecionado,
  ] = useState('TODOS')

  const [
    salvando,
    setSalvando,
  ] = useState(false)

  const [
    erroOpcao,
    setErroOpcao,
  ] = useState<
    string | null
  >(null)

  const [
    modalInclusaoAberto,
    setModalInclusaoAberto,
  ] = useState(false)

  const [
    opcaoEmEdicao,
    setOpcaoEmEdicao,
  ] = useState<
    OperacaoOpcao | null
  >(null)

  const [
    opcoesLocais,
    setOpcoesLocais,
  ] = useState<
    OperacaoOpcao[] | null
  >(null)

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

  const carteira =
    carteiras.find(
      (item) =>
        item.nome ===
        investidorAtivo,
    )?.dashboard

  const posicoesConsulta =
    useMemo(
      () =>
        obterPosicoesConsulta(
          investidorAtivo,
          carteira,
          carteiras,
        ),
      [
        investidorAtivo,
        carteira,
        carteiras,
      ],
    )

  const opcoesCarteira =
    useMemo(
      () =>
        investidorAtivo === 'TOTAL'
          ? carteiras.flatMap((item) => item.dashboard.opcoes ?? [])
          : carteira?.opcoes ?? [],
      [carteira?.opcoes, carteiras, modoAdministracao, investidorAtivo],
    )

  const opcoes =
    useMemo(
      () =>
        (
          opcoesLocais ??
          opcoesCarteira
        ).filter(
          (opcao) =>
            opcao.situacao ===
              'ABERTA' ||
            opcao.situacao ===
              'EXECUTADA' ||
            opcao.situacao ===
              'ENCERRADA' ||
            opcao.situacao ===
              'EXPIRADA',
        ),
      [opcoesCarteira, opcoesLocais],
    )

  useEffect(() => {
    setOpcoesLocais(
      [...opcoesCarteira],
    )
  }, [
    opcoesCarteira,
  ])

  useEffect(() => {
    setTickerSelecionado(
      'TODOS',
    )

    setTipoSelecionado(
      'TODOS',
    )

    setNaturezaSelecionada(
      'TODOS',
    )

    setStatusSelecionado(
      'TODOS',
    )

    setVencimentoSelecionado(
      'TODOS',
    )

    setModalInclusaoAberto(
      false,
    )

    setOpcaoEmEdicao(
      null,
    )

    setErroOpcao(
      null,
    )
  }, [
    investidorAtivo,
  ])

  const tickers =
    useMemo(
      () =>
        Array.from(
          new Set(
            opcoes.map(
              (opcao) =>
                opcao.tickerOpcao,
            ),
          ),
        )
          .filter(Boolean)
          .sort(
            (a, b) =>
              a.localeCompare(
                b,
                'pt-BR',
              ),
          ),
      [opcoes],
    )

  const opcoesAntesVencimento =
    useMemo(
      () =>
        opcoes.filter(
          (opcao) =>
            (
              tickerSelecionado ===
                'TODOS' ||
              opcao.tickerOpcao ===
                tickerSelecionado
            ) &&
            (
              tipoSelecionado ===
                'TODOS' ||
              opcao.tipoOpcao ===
                tipoSelecionado
            ) &&
            (
              naturezaSelecionada ===
                'TODOS' ||
              opcao.natureza ===
                naturezaSelecionada
            ) &&
            (
              statusSelecionado ===
                'TODOS' ||
              opcao.situacao ===
                statusSelecionado
            ),
        ),
      [
        naturezaSelecionada,
        opcoes,
        statusSelecionado,
        tickerSelecionado,
        tipoSelecionado,
      ],
    )

  const vencimentos =
    useMemo(
      () =>
        Array.from(
          new Set(
            opcoesAntesVencimento.map(
              (opcao) =>
                opcao.vencimento.slice(
                  0,
                  10,
                ),
            ),
          ),
        )
          .filter(Boolean)
          .sort(
            (a, b) =>
              b.localeCompare(a),
          ),
      [opcoesAntesVencimento],
    )

  useEffect(() => {
    if (
      vencimentoSelecionado !==
        'TODOS' &&
      !vencimentos.includes(
        vencimentoSelecionado,
      )
    ) {
      setVencimentoSelecionado(
        'TODOS',
      )
    }
  }, [
    vencimentos,
    vencimentoSelecionado,
  ])

  const opcoesFiltradas =
    useMemo(
      () =>
        opcoesAntesVencimento
          .filter(
            (opcao) =>
              vencimentoSelecionado ===
                'TODOS' ||
              opcao.vencimento.slice(
                0,
                10,
              ) ===
                vencimentoSelecionado,
          )
          .slice()
          .sort(
            (a, b) => {
              const porVencimento =
                b.vencimento.localeCompare(
                  a.vencimento,
                )

              if (
                porVencimento !==
                0
              ) {
                return porVencimento
              }

              return b.dataOperacao.localeCompare(
                a.dataOperacao,
              )
            },
          ),
      [
        opcoesAntesVencimento,
        vencimentoSelecionado,
      ],
    )

  const obterInvestidor =
    () =>
      investidores.find(
        (item) =>
          item.nome ===
          selectedInvestor,
      )

  const handleAdicionar =
    async (
      novaOpcao:
        NovaOpcaoForm,
    ) => {
      if (
        !modoAdministracao
      ) {
        return
      }

      setErroOpcao(null)

      const investidor =
        obterInvestidor()

      if (!investidor) {
        setErroOpcao(
          'Investidor não selecionado.',
        )

        return
      }

      if (
        !novaOpcao.ticker
          .trim()
      ) {
        setErroOpcao(
          'Ticker é obrigatório.',
        )

        return
      }

      if (
        Number(
          novaOpcao.quantidade,
        ) <= 0
      ) {
        setErroOpcao(
          'Quantidade deve ser maior que zero.',
        )

        return
      }

      if (
        Number(
          novaOpcao.strike
            .replace(/\./g, '')
            .replace(',', '.'),
        ) <= 0
      ) {
        setErroOpcao(
          'Strike deve ser maior que zero.',
        )

        return
      }

      if (
        Number(
          novaOpcao.premioUnitario
            .replace(/\./g, '')
            .replace(',', '.'),
        ) < 0
      ) {
        setErroOpcao(
          'Prêmio unitário inválido.',
        )

        return
      }

      try {
        setSalvando(true)

        const response =
          await fetch(
            `${apiUrl}/api/opcoes`,
            {
              method:
                'POST',

              headers: {
                'Content-Type':
                  'application/json',
              },

              body:
                JSON.stringify({
                  investidorId:
                    investidor.id,

                  dataOperacao:
                    novaOpcao
                      .dataOperacao,

                  ticker:
                    novaOpcao
                      .ticker
                      .trim()
                      .toUpperCase(),

                  tipoOpcao:
                    novaOpcao
                      .tipoOpcao,

                  natureza:
                    novaOpcao
                      .natureza,

                  quantidade:
                    Number(
                      novaOpcao
                        .quantidade,
                    ),

                  strike:
                    Number(
                      novaOpcao.strike
                        .replace(/\./g, '')
                        .replace(',', '.'),
                    ),

                  vencimento:
                    novaOpcao
                      .vencimento,

                  premioUnitario:
                    Number(
                      novaOpcao.premioUnitario
                        .replace(/\./g, '')
                        .replace(',', '.'),
                    ),
                }),
            },
          )

        if (!response.ok) {
          throw new Error(
            await lerErroApi(
              response,
            ),
          )
        }

        const responseAtualizada =
          await fetch(
            `${apiUrl}/api/opcoes/${investidor.id}?_=${Date.now()}`,
            {
              cache: 'no-store',
            },
          )

        if (!responseAtualizada.ok) {
          throw new Error(
            await lerErroApi(
              responseAtualizada,
            ),
          )
        }

        const opcoesAtualizadas =
          await responseAtualizada.json() as OperacaoOpcao[]

        setOpcoesLocais(
          opcoesAtualizadas,
        )

        setModalInclusaoAberto(
          false,
        )
      } catch (error) {
        setErroOpcao(
          error instanceof Error
            ? error.message
            : 'Não foi possível incluir a opção.',
        )
      } finally {
        setSalvando(false)
      }
    }

  const handleSalvarEdicao =
    async (
      opcao:
        OperacaoOpcao,
    ) => {
      if (
        !modoAdministracao
      ) {
        return
      }

      try {
        setSalvando(true)
        setErroOpcao(null)

        const response =
          await fetch(
            `${apiUrl}/api/opcoes/${opcao.id}`,
            {
              method:
                'PUT',

              headers: {
                'Content-Type':
                  'application/json',
              },

              body:
                JSON.stringify({
                  dataOperacao:
                    opcao.dataOperacao.slice(
                      0,
                      10,
                    ),

                  vencimento:
                    opcao.vencimento.slice(
                      0,
                      10,
                    ),

                  strike:
                    Number(
                      opcao.strike,
                    ),

                  contratos:
                    opcao.contratos >
                    0
                      ? opcao.contratos
                      : 1,

                  quantidade:
                    Number(
                      opcao.quantidade,
                    ),

                  premioUnitario:
                    Number(
                      opcao.premioUnitario,
                    ),

                  situacao:
                    opcao.situacao,

                  dataFinalizacao:
                    opcao.dataFinalizacao ??
                    null,

                  precoRecompraUnitario:
                    opcao.precoRecompraUnitario ??
                    null,

                  valorExecucao:
                    opcao.valorExecucao ??
                    null,

                  resultadoInformado:
                    opcao.resultadoInformado ??
                    null,
                }),
            },
          )

        if (!response.ok) {
          throw new Error(
            await lerErroApi(
              response,
            ),
          )
        }

        /*
         * O PUT retorna 204 No Content. Portanto não há JSON
         * para desserializar aqui. Atualizamos a opção localmente
         * com os mesmos dados que acabaram de ser persistidos.
         */
        /*
         * Campos exibidos na tabela, como Resultado Bruto,
         * Resultado Líquido e IR, são calculados pelo backend.
         * Portanto, após o PUT precisamos consultar novamente
         * as opções do investidor em vez de apenas copiar os
         * campos editados para o estado local.
         */
        const investidor =
          obterInvestidor()

        if (!investidor) {
          throw new Error(
            'Investidor não selecionado.',
          )
        }

        const responseAtualizada =
          await fetch(
            `${apiUrl}/api/opcoes/${investidor.id}?_=${Date.now()}`,
            {
              cache: 'no-store',
            },
          )

        if (!responseAtualizada.ok) {
          throw new Error(
            await lerErroApi(
              responseAtualizada,
            ),
          )
        }

        const opcoesAtualizadas =
          await responseAtualizada.json() as OperacaoOpcao[]

        setOpcoesLocais(
          opcoesAtualizadas,
        )

        setOpcaoEmEdicao(null)
      } catch (error) {
        setErroOpcao(
          error instanceof Error
            ? error.message
            : 'Não foi possível editar a opção.',
        )
      } finally {
        setSalvando(false)
      }
    }

  const handleExcluir =
    async (
      opcao:
        OperacaoOpcao,
    ) => {
      if (
        !modoAdministracao
      ) {
        return
      }

      if (
        !window.confirm(
          `Excluir ${opcao.tickerOpcao}? Esta ação não pode ser desfeita.`,
        )
      ) {
        return
      }

      try {
        setSalvando(true)
        setErroOpcao(null)

        const response =
          await fetch(
            `${apiUrl}/api/opcoes/${opcao.id}`,
            {
              method:
                'DELETE',
            },
          )

        if (!response.ok) {
          throw new Error(
            await lerErroApi(
              response,
            ),
          )
        }

        window.location.reload()
      } catch (error) {
        const mensagem =
          error instanceof Error
            ? error.message
            : 'Não foi possível excluir a opção.'

        setErroOpcao(
          mensagem,
        )

        alert(
          mensagem,
        )
      } finally {
        setSalvando(false)
      }
    }

  const limparFiltros =
    () => {
      setTickerSelecionado(
        'TODOS',
      )

      setTipoSelecionado(
        'TODOS',
      )

      setNaturezaSelecionada(
        'TODOS',
      )

      setStatusSelecionado(
        'TODOS',
      )

      setVencimentoSelecionado(
        'TODOS',
      )
    }

  const abrirNovaOpcao =
    () => {
      if (
        !modoAdministracao
      ) {
        return
      }

      setOpcaoEmEdicao(
        null,
      )

      setErroOpcao(
        null,
      )

      setModalInclusaoAberto(
        true,
      )
    }

  const abrirEdicao = (
    opcao:
      OperacaoOpcao,
  ) => {
    if (
      !modoAdministracao
    ) {
      return
    }

    setModalInclusaoAberto(
      false,
    )

    setErroOpcao(
      null,
    )

    setOpcaoEmEdicao(
      opcao,
    )
  }

  const fecharModal =
    () => {
      if (salvando) {
        return
      }

      setModalInclusaoAberto(
        false,
      )

      setOpcaoEmEdicao(
        null,
      )

      setErroOpcao(
        null,
      )
    }

  return (
    <section className="portfolio-view options-page">
      <article className="panel options-header-panel">
        <div className="options-page-heading">
          <h1 className="options-page-title">
            {modoAdministracao
              ? 'Administração de Opções'
              : 'Opções'}
          </h1>
        </div>

        <label className="options-investor-label">
          <span>
            Investidor
          </span>

          <select
            value={
              investidorAtivo
            }
            onChange={(event) => {
              const nome = event.target.value
              if (modoAdministracao) {
                onSelectInvestor(nome)
              } else {
                setInvestidorConsulta(nome)
                if (nome !== 'TOTAL') {
                  onSelectInvestor(nome)
                }
              }
            }}
          >
            <option value="TOTAL">
              Todos
            </option>
            {investidoresComOpcoes.map(
              (
                investidor,
              ) => (
                <option
                  key={
                    investidor.id
                  }
                  value={
                    investidor.nome
                  }
                >
                  {
                    investidor.nome
                  }
                </option>
              ),
            )}
          </select>
        </label>
      </article>

      {!modoAdministracao ? (
        <OpcoesGraficos
          opcoes={
            opcoes
          }
        />
      ) : null}

      {!modoAdministracao ? (
        <CallsDisponiveis
          posicoes={
            posicoesConsulta
          }
          opcoes={
            opcoes
          }
        />
      ) : null}

      <OpcoesFiltros
        tickers={
          tickers
        }
        vencimentos={
          vencimentos
        }
        ticker={
          tickerSelecionado
        }
        tipo={
          tipoSelecionado
        }
        natureza={
          naturezaSelecionada
        }
        status={
          statusSelecionado
        }
        vencimento={
          vencimentoSelecionado
        }
        onTickerChange={
          setTickerSelecionado
        }
        onTipoChange={
          setTipoSelecionado
        }
        onNaturezaChange={
          setNaturezaSelecionada
        }
        onStatusChange={
          setStatusSelecionado
        }
        onVencimentoChange={
          setVencimentoSelecionado
        }
        onLimpar={
          limparFiltros
        }
      />

      <OpcoesTabela
        opcoes={
          opcoesFiltradas
        }
        salvando={
          salvando
        }
        modoAdministracao={
          modoAdministracao
        }
        onNovaOpcao={
          modoAdministracao
            ? abrirNovaOpcao
            : undefined
        }
        onEditar={
          modoAdministracao
            ? abrirEdicao
            : undefined
        }
        onExcluir={
          modoAdministracao
            ? handleExcluir
            : undefined
        }
      />

      {modoAdministracao &&
      modalInclusaoAberto ? (
        <OpcaoModal
          modo="novo"
          salvando={
            salvando
          }
          erro={
            erroOpcao
          }
          onClose={
            fecharModal
          }
          onAdicionar={
            handleAdicionar
          }
        />
      ) : null}

      {modoAdministracao &&
      opcaoEmEdicao ? (
        <OpcaoModal
          modo="editar"
          opcao={
            opcaoEmEdicao
          }
          salvando={
            salvando
          }
          erro={
            erroOpcao
          }
          onClose={
            fecharModal
          }
          onSalvar={
            handleSalvarEdicao
          }
        />
      ) : null}
    </section>
  )
}