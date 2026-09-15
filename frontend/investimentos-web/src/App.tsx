import {
  useEffect,
  useMemo,
  useState,
} from 'react'

import {
  atualizarOperacao,
  obterDashboardConsolidado,
  obterDashboardPorInvestidor,
  obterEvolucaoConsolidada,
  obterOperacoes,
} from './api/dashboardApi'

import {
  listarInvestidores,
} from './api/investidoresApi'

import {
  useAuth,
} from './auth/AuthContext'

import {
  dashboardSnapshot,
} from './data/dashboardSnapshot'

import {
  CarteiraView,
} from './pages/Carteira/CarteiraView'

import {
  DashboardView,
} from './pages/Dashboard/DashboardView'

import {
  OperacoesView,
} from './pages/Operacoes/OperacoesView'

import {
  OpcoesView,
} from './pages/Opcoes/OpcoesView'

import {
  ProventosView,
} from './pages/Proventos/ProventosView'

import {
  AdministracaoView,
} from './pages/Administracao/AdministracaoView'

import type {
  Dashboard,
  EvolucaoInvestidor,
  OperacaoCarteira,
} from './types/dashboard'

import type {
  Investidor,
} from './types/investidor'

import {
  definirOcultacaoValores,
} from './utils/formatters'

import './styles/legacy.css'

type Tela =
  | 'painel'
  | 'carteira'
  | 'operacoes'
  | 'opcoes'
  | 'proventos'
  | 'administracao'

interface ConfiguracaoMenu {
  tela: Tela
  permissao: string
  titulo: string
  icone: string
}

const menuPrincipal:
  ConfiguracaoMenu[] = [
    {
      tela: 'painel',
      permissao: 'Dashboard',
      titulo: 'Painel',
      icone: '▣',
    },
    {
      tela: 'carteira',
      permissao: 'Carteira',
      titulo: 'Carteira',
      icone: '◫',
    },
    {
      tela: 'operacoes',
      permissao: 'Operacoes',
      titulo: 'Operações',
      icone: '↕',
    },
    {
      tela: 'opcoes',
      permissao: 'Opcoes',
      titulo: 'Opções',
      icone: '◌',
    },
    {
      tela: 'proventos',
      permissao: 'Proventos',
      titulo: 'Proventos',
      icone: '$',
    },
  ]

const menuAdministracao:
  ConfiguracaoMenu = {
    tela: 'administracao',
    permissao: 'Administracao',
    titulo: 'Administração',
    icone: '⚙',
  }

function App() {
  const {
    usuario,
    possuiPermissao,
  } = useAuth()

  const [
    telaAtual,
    setTelaAtual,
  ] =
    useState<Tela>(
      'painel',
    )

  const [
    todosInvestidores,
    setTodosInvestidores,
  ] =
    useState<Investidor[]>(
      [],
    )

  const [
    dashboard,
    setDashboard,
  ] =
    useState<Dashboard | null>(
      null,
    )

  /*
   * Coleção exclusiva do Painel.
   *
   * Quando o usuário possui
   * permissão Dashboard, esta
   * coleção contém TODOS os
   * investidores, independentemente
   * dos investidores associados
   * ao usuário.
   */
  const [
    carteirasPainel,
    setCarteirasPainel,
  ] =
    useState<
      Array<{
        nome: string
        dashboard: Dashboard
      }>
    >([])

  /*
   * Coleção utilizada pelas telas
   * que respeitam o vínculo entre
   * usuário e investidor.
   */
  const [
    carteirasPorInvestidor,
    setCarteirasPorInvestidor,
  ] =
    useState<
      Array<{
        nome: string
        dashboard: Dashboard
      }>
    >([])

  const [
    operacoes,
    setOperacoes,
  ] =
    useState<
      OperacaoCarteira[]
    >([])

  const [
    evolucao,
    setEvolucao,
  ] =
    useState<
      EvolucaoInvestidor[]
    >([])

  const [
    carregandoInvestidores,
    setCarregandoInvestidores,
  ] =
    useState(true)

  const [
    apiDisponivel,
    setApiDisponivel,
  ] =
    useState(true)

  const [
    erro,
    setErro,
  ] =
    useState<string | null>(
      null,
    )

  const [
    investidorSelecionado,
    setInvestidorSelecionado,
  ] =
    useState('')

  const [
    valoresOcultos,
    setValoresOcultos,
  ] =
    useState(false)

  definirOcultacaoValores(
    valoresOcultos,
  )

  /*
   * Investidores disponíveis para
   * Carteira, Operações, Opções
   * e Proventos.
   *
   * Admin vê todos.
   * Usuário comum vê somente os
   * investidores associados.
   */
  const investidores =
    useMemo(() => {
      if (!usuario) {
        return []
      }

      if (
        usuario.perfil ===
        'Admin'
      ) {
        return todosInvestidores
      }

      const idsPermitidos =
        new Set(
          usuario.investidoresIds,
        )

      return todosInvestidores.filter(
        (investidor) =>
          idsPermitidos.has(
            investidor.id,
          ),
      )
    }, [
      todosInvestidores,
      usuario,
    ])

  const primeiraTelaPermitida =
    useMemo(() => {
      const configuracoes = [
        ...menuPrincipal,
        menuAdministracao,
      ]

      return (
        configuracoes.find(
          (item) =>
            possuiPermissao(
              item.permissao,
            ),
        )?.tela ??
        null
      )
    }, [
      usuario,
      possuiPermissao,
    ])

  function telaPermitida(
    tela: Tela,
  ) {
    const configuracao = [
      ...menuPrincipal,
      menuAdministracao,
    ].find(
      (item) =>
        item.tela === tela,
    )

    if (!configuracao) {
      return false
    }

    return possuiPermissao(
      configuracao.permissao,
    )
  }

  function navegar(
    tela: Tela,
  ) {
    if (
      !telaPermitida(tela)
    ) {
      return
    }

    setTelaAtual(tela)
  }

  /*
   * Se o usuário estiver em uma
   * tela que deixou de possuir,
   * redirecionamos para a primeira
   * tela permitida.
   */
  useEffect(() => {
    if (!usuario) {
      return
    }

    if (
      telaPermitida(
        telaAtual,
      )
    ) {
      return
    }

    if (
      primeiraTelaPermitida
    ) {
      setTelaAtual(
        primeiraTelaPermitida,
      )
    }
  }, [
    usuario,
    telaAtual,
    primeiraTelaPermitida,
  ])

  /*
   * A lista-base de investidores é
   * carregada integralmente.
   *
   * A filtragem ocorre depois,
   * conforme o módulo utilizado.
   */
  useEffect(() => {
    async function carregarInvestidores() {
      try {
        setCarregandoInvestidores(
          true,
        )

        const dados =
          await listarInvestidores()

        if (
          dados.length > 0
        ) {
          setTodosInvestidores(
            dados,
          )

          setApiDisponivel(
            true,
          )

          setErro(null)

          return
        }

        throw new Error(
          'Lista vazia',
        )
      } catch (error) {
        console.error(error)

        setApiDisponivel(
          false,
        )

        setErro(null)

        /*
         * O snapshot local continua
         * disponível apenas para o
         * administrador.
         */
        if (
          usuario?.perfil ===
          'Admin'
        ) {
          setTodosInvestidores(
            dashboardSnapshot
              .investidores
              .map(
                (nome) => ({
                  id: nome,
                  nome,
                }),
              ),
          )
        } else {
          setTodosInvestidores(
            [],
          )
        }
      } finally {
        setCarregandoInvestidores(
          false,
        )
      }
    }

    void carregarInvestidores()
  }, [usuario])

  /*
   * Mantém um investidor válido
   * selecionado nas telas
   * individuais.
   */
  useEffect(() => {
    if (
      investidores.length ===
      0
    ) {
      setInvestidorSelecionado(
        '',
      )

      return
    }

    const selecionadoValido =
      investidores.some(
        (item) =>
          item.nome ===
          investidorSelecionado,
      )

    if (!selecionadoValido) {
      setInvestidorSelecionado(
        investidores[0].nome,
      )
    }
  }, [
    investidores,
    investidorSelecionado,
  ])

  /*
   * Operações respeitam o
   * investidor autorizado.
   */
  useEffect(() => {
    if (
      !apiDisponivel ||
      !investidorSelecionado ||
      !possuiPermissao(
        'Operacoes',
      )
    ) {
      setOperacoes([])

      return
    }

    const investidor =
      investidores.find(
        (item) =>
          item.nome ===
          investidorSelecionado,
      )

    if (!investidor) {
      setOperacoes([])

      return
    }

    obterOperacoes(
      investidor.id,
    )
      .then(
        setOperacoes,
      )
      .catch(
        (error) => {
          console.error(
            error,
          )

          setOperacoes(
            [],
          )
        },
      )
  }, [
    apiDisponivel,
    investidorSelecionado,
    investidores,
    usuario,
  ])

  /*
   * PAINEL GLOBAL
   *
   * A permissão Dashboard significa
   * acesso à mesma visão consolidada
   * apresentada ao administrador.
   */
  useEffect(() => {
    if (
      !apiDisponivel ||
      !possuiPermissao(
        'Dashboard',
      )
    ) {
      setDashboard(null)
      setEvolucao([])
      setCarteirasPainel([])

      return
    }

    const controller =
      new AbortController()

    async function carregarPainel() {
      try {
        setErro(null)

        const [
          dadosDashboard,
          dadosEvolucao,
          resultadosCarteiras,
        ] =
          await Promise.all([
            obterDashboardConsolidado(),

            obterEvolucaoConsolidada(),

            Promise.allSettled(
              todosInvestidores.map(
                async (
                  investidor,
                ) => ({
                  nome:
                    investidor.nome,

                  dashboard:
                    await obterDashboardPorInvestidor(
                      investidor.id,
                    ),
                }),
              ),
            ),
          ])

        if (
          controller.signal
            .aborted
        ) {
          return
        }

        const carteirasComSucesso =
          resultadosCarteiras.flatMap(
            (resultado) => {
              if (
                resultado.status ===
                'fulfilled'
              ) {
                return [
                  resultado.value,
                ]
              }

              console.error(
                resultado.reason,
              )

              return []
            },
          )

        setDashboard(
          dadosDashboard,
        )

        setEvolucao(
          dadosEvolucao,
        )

        setCarteirasPainel(
          carteirasComSucesso,
        )
      } catch (error) {
        console.error(error)

        if (
          !controller.signal
            .aborted
        ) {
          setDashboard(null)
          setEvolucao([])
          setCarteirasPainel([])

          setErro(
            'Não foi possível carregar o painel.',
          )
        }
      }
    }

    if (
      todosInvestidores.length >
      0
    ) {
      void carregarPainel()
    }

    return () =>
      controller.abort()
  }, [
    apiDisponivel,
    todosInvestidores,
    usuario,
  ])

  /*
   * CARTEIRAS RESTRITAS
   *
   * Utilizadas pelas demais telas.
   * Aqui permanece a regra dos
   * investidores associados ao
   * usuário.
   */
  useEffect(() => {
    if (
      !apiDisponivel ||
      investidores.length ===
        0
    ) {
      setCarteirasPorInvestidor(
        [],
      )

      return
    }

    const podeConsultarCarteiras =
      possuiPermissao(
        'Carteira',
      ) ||
      possuiPermissao(
        'Opcoes',
      )

    if (
      !podeConsultarCarteiras
    ) {
      setCarteirasPorInvestidor(
        [],
      )

      return
    }

    async function carregarCarteirasRestritas() {
      const resultados =
        await Promise.allSettled(
          investidores.map(
            async (
              investidor,
            ) => ({
              nome:
                investidor.nome,

              dashboard:
                await obterDashboardPorInvestidor(
                  investidor.id,
                ),
            }),
          ),
        )

      const sucesso =
        resultados.flatMap(
          (resultado) => {
            if (
              resultado.status ===
              'fulfilled'
            ) {
              return [
                resultado.value,
              ]
            }

            console.error(
              resultado.reason,
            )

            return []
          },
        )

      setCarteirasPorInvestidor(
        sucesso,
      )
    }

    void carregarCarteirasRestritas()
  }, [
    apiDisponivel,
    investidores,
    usuario,
  ])

  async function salvarOperacao(
    operacao: Pick<
      OperacaoCarteira,
      | 'id'
      | 'data'
      | 'quantidade'
      | 'precoUnitario'
      | 'taxas'
    >,
  ) {
    if (
      !possuiPermissao(
        'Operacoes',
      )
    ) {
      throw new Error(
        'Você não possui permissão para alterar operações.',
      )
    }

    await atualizarOperacao(
      operacao.id,
      operacao,
    )

    const investidor =
      investidores.find(
        (item) =>
          item.nome ===
          investidorSelecionado,
      )

    if (!investidor) {
      return
    }

    const [
      dashboardAtualizado,
      operacoesAtualizadas,
    ] =
      await Promise.all([
        obterDashboardPorInvestidor(
          investidor.id,
        ),

        obterOperacoes(
          investidor.id,
        ),
      ])

    setCarteirasPorInvestidor(
      (atuais) =>
        atuais.map(
          (item) =>
            item.nome ===
            investidorSelecionado
              ? {
                  ...item,

                  dashboard:
                    dashboardAtualizado,
                }
              : item,
        ),
    )

    /*
     * Caso o usuário também possua
     * acesso ao Painel, mantemos a
     * coleção global sincronizada.
     */
    setCarteirasPainel(
      (atuais) =>
        atuais.map(
          (item) =>
            item.nome ===
            investidorSelecionado
              ? {
                  ...item,

                  dashboard:
                    dashboardAtualizado,
                }
              : item,
        ),
    )

    setOperacoes(
      operacoesAtualizadas,
    )
  }

  const modoSnapshot =
    !apiDisponivel ||
    todosInvestidores.length ===
      0 ||
    (
      possuiPermissao(
        'Dashboard',
      ) &&
      dashboard === null
    )

  if (
    carregandoInvestidores
  ) {
    return (
      <main className="estado-pagina">
        <div className="loader" />

        <p>
          Carregando investidores...
        </p>
      </main>
    )
  }

  if (
    !primeiraTelaPermitida
  ) {
    return (
      <main className="estado-pagina">
        <p>
          Seu usuário não possui
          nenhum módulo liberado.
        </p>
      </main>
    )
  }

  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="marca">
          <img
            className="marca-icone"
            src="/tio-patinhas.png"
            alt="Tio Patinhas mergulhando em moedas"
          />

          <div className="marca-info">
            <strong>
              {usuario?.nome ??
                'Investimentos'}
            </strong>

            <span>
              {usuario?.perfil ===
              'Admin'
                ? 'Administrador'
                : 'Dashboard de Carteira'}
            </span>
          </div>
        </div>

        <nav className="menu">
          {menuPrincipal.map(
            (item) => {
              const permitido =
                possuiPermissao(
                  item.permissao,
                )

              return (
                <button
                  key={
                    item.tela
                  }
                  className={`menu-item ${
                    telaAtual ===
                    item.tela
                      ? 'ativo'
                      : ''
                  } ${
                    !permitido
                      ? 'bloqueado'
                      : ''
                  }`}
                  type="button"
                  disabled={
                    !permitido
                  }
                  title={
                    permitido
                      ? item.titulo
                      : 'Módulo não liberado para este usuário'
                  }
                  onClick={() =>
                    navegar(
                      item.tela,
                    )
                  }
                >
                  <span className="menu-icone">
                    {
                      item.icone
                    }
                  </span>

                  {
                    item.titulo
                  }

                  {!permitido ? (
                    <span
                      className="menu-lock"
                      aria-hidden="true"
                    >
                      🔒
                    </span>
                  ) : null}
                </button>
              )
            },
          )}

          <div className="menu-separator" />

          {(() => {
            const permitido =
              possuiPermissao(
                menuAdministracao
                  .permissao,
              )

            return (
              <button
                className={`menu-item ${
                  telaAtual ===
                  'administracao'
                    ? 'ativo'
                    : ''
                } ${
                  !permitido
                    ? 'bloqueado'
                    : ''
                }`}
                type="button"
                disabled={
                  !permitido
                }
                title={
                  permitido
                    ? 'Administração'
                    : 'Módulo não liberado para este usuário'
                }
                onClick={() =>
                  navegar(
                    'administracao',
                  )
                }
              >
                <span className="menu-icone">
                  ⚙
                </span>

                Administração

                {!permitido ? (
                  <span
                    className="menu-lock"
                    aria-hidden="true"
                  >
                    🔒
                  </span>
                ) : null}
              </button>
            )
          })()}
        </nav>

        <div className="sidebar-rodape">
          <span>
            {
              dashboardSnapshot
                .referencia
            }
          </span>

          <small>
            {modoSnapshot
              ? 'Snapshot local'
              : 'API conectada'}
          </small>
        </div>
      </aside>

      <main className="main">
        <div className="privacy-toolbar">
          <button
            className={`privacy-toggle ${
              valoresOcultos
                ? 'ativo'
                : ''
            }`}
            type="button"
            onClick={() =>
              setValoresOcultos(
                (atual) =>
                  !atual,
              )
            }
            aria-label={
              valoresOcultos
                ? 'Exibir valores'
                : 'Ocultar valores'
            }
            title={
              valoresOcultos
                ? 'Exibir valores'
                : 'Ocultar valores'
            }
          >
            <span
              className="privacy-eye"
              aria-hidden="true"
            >
              ◉
            </span>

            <span>
              {valoresOcultos
                ? 'Exibir valores'
                : 'Ocultar valores'}
            </span>
          </button>
        </div>

        {telaAtual ===
          'painel' &&
        possuiPermissao(
          'Dashboard',
        ) ? (
          <DashboardView
            dashboard={
              dashboard
            }
            carteiras={
              carteirasPainel
            }
            evolucao={
              evolucao
            }
            apiDisponivel={
              apiDisponivel
            }
            erro={erro}
          />
        ) : null}

        {telaAtual ===
          'carteira' &&
        possuiPermissao(
          'Carteira',
        ) ? (
          <CarteiraView
            investidores={
              investidores
            }
            carteiras={
              carteirasPorInvestidor
            }
            selectedInvestor={
              investidorSelecionado
            }
            onSelectInvestor={
              setInvestidorSelecionado
            }
            snapshotSeries={
              usuario?.perfil ===
              'Admin'
                ? dashboardSnapshot
                    .timelinePorPessoa
                : {}
            }
            saldosDisponiveis={
              usuario?.perfil ===
              'Admin'
                ? dashboard
                    ?.saldosDisponiveis ??
                  []
                : []
            }
          />
        ) : null}

        {telaAtual ===
          'operacoes' &&
        possuiPermissao(
          'Operacoes',
        ) ? (
          <OperacoesView
            investidores={
              investidores
            }
            selectedInvestor={
              investidorSelecionado
            }
            onSelectInvestor={
              setInvestidorSelecionado
            }
            operacoes={
              operacoes
            }
            onSaveOperation={
              salvarOperacao
            }
          />
        ) : null}

        {telaAtual ===
          'opcoes' &&
        possuiPermissao(
          'Opcoes',
        ) ? (
          <OpcoesView
            investidores={
              investidores
            }
            carteiras={
              carteirasPorInvestidor
            }
            selectedInvestor={
              investidorSelecionado
            }
            onSelectInvestor={
              setInvestidorSelecionado
            }
          />
        ) : null}

        {telaAtual ===
          'proventos' &&
        possuiPermissao(
          'Proventos',
        ) ? (
          <ProventosView
            investidores={
              investidores
            }
            selectedInvestor={
              investidorSelecionado
            }
            onSelectInvestor={
              setInvestidorSelecionado
            }
          />
        ) : null}

        {telaAtual ===
          'administracao' &&
        possuiPermissao(
          'Administracao',
        ) ? (
          <AdministracaoView />
        ) : null}
      </main>
    </div>
  )
}

export default App