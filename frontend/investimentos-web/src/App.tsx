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
  obterPerformance,
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
  OpcoesView,
} from './pages/Opcoes/OpcoesView'

import {
  ProventosView,
} from './pages/Proventos/ProventosView'

import {
  AdministracaoView,
} from './pages/Administracao/AdministracaoView'

import {
  AtivosView,
} from './pages/Ativos/AtivosView'

import {
  AssistenteIaView,
} from './pages/AssistenteIa/AssistenteIaView'

import { consultarGeracaoIa, gerarRelatorioIa, type StatusGeracaoIa } from './api/assistenteIaApi'

import { AnalisesView } from './pages/Analises/AnalisesView'
import { AnaliseEstrategicaView } from './pages/AnaliseEstrategica/AnaliseEstrategicaView'
import { AlterarSenhaModal } from './pages/AlterarSenha/AlterarSenhaModal'

import type {
  Dashboard,
  EvolucaoInvestidor,
  OperacaoCarteira,
  PerformanceCarteira,
} from './types/dashboard'

import type {
  Investidor,
} from './types/investidor'

import {
  definirOcultacaoValores,
} from './utils/formatters'

import './styles/legacy.css'
import { invalidateRequests } from './api/requestCache'

type Tela =
  | 'painel'
  | 'carteira'
  | 'ativos'
  | 'opcoes'
  | 'proventos'
  | 'assistente-ia'
  | 'analise-estrategica'
  | 'analises'
  | 'administracao'

interface ConfiguracaoMenu {
  tela: Tela
  permissao: string
  titulo: string
  icone: string
}

const menuPrincipal:
  ConfiguracaoMenu[] = [
    { tela: 'painel', permissao: 'Dashboard', titulo: 'Painel', icone: '▣' },
    { tela: 'carteira', permissao: 'Carteira', titulo: 'Carteira', icone: '◫' },
    { tela: 'ativos', permissao: 'Ativos', titulo: 'Ativos', icone: '◈' },
    { tela: 'opcoes', permissao: 'Opcoes', titulo: 'Opções', icone: '◌' },
    { tela: 'proventos', permissao: 'Proventos', titulo: 'Proventos', icone: '$' },
    { tela: 'analises', permissao: 'Analises', titulo: 'Análises', icone: '⌁' },
  ]

const menuAssistenteIa: ConfiguracaoMenu = {
  tela: 'assistente-ia', permissao: 'AssistenteIa', titulo: 'Assistente IA', icone: '✦',
}

const menuEstrategia: ConfiguracaoMenu = {
  tela: 'analise-estrategica', permissao: 'Administracao', titulo: 'Análise Estratégica', icone: '◎',
}

const menuAdministracao: ConfiguracaoMenu = {
  tela: 'administracao', permissao: 'Administracao', titulo: 'Administração', icone: '⚙',
}

function App() {
  const {
    usuario,
    possuiPermissao,
    logout,
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
    performance,
    setPerformance,
  ] = useState<PerformanceCarteira | null>(null)

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

  const [alterandoSenha, setAlterandoSenha] = useState(false)
  const [geracaoIa, setGeracaoIa] = useState<StatusGeracaoIa | null>(null)
  const [popupIa, setPopupIa] = useState<string | null>(null)
  const [jobNotificado, setJobNotificado] = useState<string | null>(null)

  async function iniciarGeracaoIa() {
    try {
      const status = await gerarRelatorioIa()
      setGeracaoIa(status)
      setPopupIa('Análise iniciada. Você pode navegar enquanto o relatório é gerado.')
    } catch (error) {
      setPopupIa(error instanceof Error ? error.message : 'Falha ao iniciar análise.')
    }
  }

  useEffect(() => {
    if (usuario?.perfil !== 'Admin') return
    let ativo = true
    const consultar = async () => {
      try {
        const status = await consultarGeracaoIa()
        if (!ativo) return
        setGeracaoIa(status)
        if (status.jobId && status.jobId !== jobNotificado &&
          (status.estado === 'concluido' || status.estado === 'erro')) {
          setJobNotificado(status.jobId)
          setPopupIa(status.estado === 'concluido'
            ? 'A análise foi concluída! Acesse o Assistente IA para consultar o relatório.'
            : status.erro ?? 'Falha ao gerar análise.')
        }
      } catch (error) {
        console.error('Falha ao consultar o estado da análise IA', error)
      }
    }
    // Uma consulta inicial; polling apenas durante uma geracao em andamento.
    if (geracaoIa === null || geracaoIa.estado === 'executando') void consultar()
    const timer = geracaoIa?.estado === 'executando'
      ? window.setInterval(() => void consultar(), 5000)
      : null
    return () => { ativo = false; if (timer !== null) window.clearInterval(timer) }
  }, [usuario?.perfil, jobNotificado, geracaoIa?.estado])



  const [
    valoresOcultos,
    setValoresOcultos,
  ] =
    useState(false)

  const [
    atualizandoDados,
    setAtualizandoDados,
  ] = useState(false)

  definirOcultacaoValores(
    valoresOcultos,
  )

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
        menuAssistenteIa,
        menuEstrategia,
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

  function permissaoNoInvestidor(
    permissao: string,
    investidorId?: string,
  ) {
    if (!usuario) return false
    if (usuario.perfil === 'Admin') return true
    if (!investidorId) {
      return usuario.acessosInvestidores?.some(
        (acesso) => acesso.permissoes.some(
          (item) => item.toLowerCase() === permissao.toLowerCase(),
        ),
      ) ?? possuiPermissao(permissao)
    }

    const acesso = usuario.acessosInvestidores?.find(
      (item) => item.investidorId === investidorId,
    )
    return acesso?.permissoes.some(
      (item) => item.toLowerCase() === permissao.toLowerCase(),
    ) ?? false
  }

  function investidoresPorPermissao(
    permissao: string,
  ) {
    if (!usuario || usuario.perfil === 'Admin') {
      return investidores
    }

    return investidores.filter(
      (investidor) =>
        permissaoNoInvestidor(
          permissao,
          investidor.id,
        ),
    )
  }

  const podeVerDadosDeOutrosInvestidores =
    usuario?.perfil === 'Admin' ||
    (usuario?.investidoresIds?.length ?? 0) > 1

  function telaPermitida(
    tela: Tela,
  ) {
    const configuracao = [
      ...menuPrincipal,
      menuAssistenteIa,
      menuEstrategia,
      menuAdministracao,
    ].find(
      (item) =>
        item.tela === tela,
    )

    if (!configuracao) {
      return false
    }

    if (configuracao.tela === 'administracao' || configuracao.tela === 'analise-estrategica') {
      return usuario?.perfil === 'Admin'
    }

    return permissaoNoInvestidor(
      configuracao.permissao,
    )
  }

  async function carregarDadosAtuais() {
    const dadosInvestidores =
      await listarInvestidores()

    const idsPermitidos =
      new Set(
        usuario?.investidoresIds ?? [],
      )

    const investidoresPermitidos =
      !usuario ||
      usuario.perfil === 'Admin'
        ? dadosInvestidores
        : dadosInvestidores.filter(
            (investidor) =>
              idsPermitidos.has(
                investidor.id,
              ),
          )

    const podeDashboard =
      possuiPermissao(
        'Dashboard',
      )

    const podeConsultarCarteiras =
      possuiPermissao(
        'Carteira',
      ) ||
      possuiPermissao(
        'Ativos',
      ) ||
      possuiPermissao(
        'Analises',
      ) ||
      possuiPermissao(
        'Opcoes',
      ) ||
      possuiPermissao(
        'Administracao',
      )

    const [
      dadosDashboard,
      dadosEvolucao,
      dadosPerformance,
      resultadosCarteiras,
    ] = await Promise.all([
      podeDashboard
        ? obterDashboardConsolidado()
        : Promise.resolve(null),

      podeDashboard
        ? obterEvolucaoConsolidada()
        : Promise.resolve([]),

      podeDashboard
        ? obterPerformance()
        : Promise.resolve(null),

      podeDashboard ||
      podeConsultarCarteiras
        ? Promise.allSettled(
            investidoresPermitidos.map(
              async (investidor) => ({
                nome:
                  investidor.nome,

                dashboard:
                  await obterDashboardPorInvestidor(
                    investidor.id,
                  ),
              }),
            ),
          )
        : Promise.resolve([]),
    ])

    const carteirasAtualizadas =
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

    setTodosInvestidores(
      dadosInvestidores,
    )

    setDashboard(
      dadosDashboard,
    )

    setEvolucao(
      dadosEvolucao,
    )

    setPerformance(
      dadosPerformance,
    )

    setCarteirasPainel(
      podeDashboard
        ? carteirasAtualizadas
        : [],
    )

    setCarteirasPorInvestidor(
      podeConsultarCarteiras
        ? carteirasAtualizadas
        : [],
    )

    const investidorAtual =
      investidoresPermitidos.find(
        (item) =>
          item.nome ===
          investidorSelecionado,
      )

    if (
      investidorAtual &&
      possuiPermissao(
        'Operacoes',
      )
    ) {
      setOperacoes(
        await obterOperacoes(
          investidorAtual.id,
        ),
      )
    } else {
      setOperacoes([])
    }

    setApiDisponivel(true)
  }

  async function atualizarTudo() {
    if (atualizandoDados) {
      return
    }

    try {
      setAtualizandoDados(true)
      setErro(null)

      invalidateRequests()
      await carregarDadosAtuais()

    } catch (error) {
      console.error(error)

      setErro(
        'Não foi possível atualizar os dados.',
      )
    } finally {
      setAtualizandoDados(false)
    }
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
   * Operações continuam sendo
   * carregadas normalmente, mas
   * agora sua manutenção acontece
   * dentro de Administração.
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

  useEffect(() => {
    if (
      !apiDisponivel ||
      !possuiPermissao(
        'Dashboard',
      )
    ) {
      setDashboard(null)
      setEvolucao([])
      setPerformance(null)
      setCarteirasPainel([])

      return
    }

    const controller =
      new AbortController()

    async function carregarPainel() {
      try {
        setErro(null)

        const investidoresPainel =
          investidoresPorPermissao(
            'Dashboard',
          )

        /*
         * Somente o administrador consulta os endpoints consolidados,
         * que abrangem todos os investidores. Usuários comuns montam
         * o Painel exclusivamente com os investidores autorizados.
         */
        if (
          usuario?.perfil !==
          'Admin'
        ) {
          const resultados =
            await Promise.allSettled(
              investidoresPainel.map(
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

          if (
            controller.signal
              .aborted
          ) {
            return
          }

          const carteirasPermitidas =
            resultados.flatMap(
              (resultado) =>
                resultado.status ===
                'fulfilled'
                  ? [resultado.value]
                  : [],
            )

          setCarteirasPainel(
            carteirasPermitidas,
          )

          /*
           * Para um único investidor, o próprio dashboard individual
           * alimenta os cards principais. Com mais de um investidor,
           * evitamos expor o consolidado global; as carteiras autorizadas
           * continuam disponíveis no Painel sem dados de terceiros.
           */
          setDashboard(
            carteirasPermitidas.length ===
              1
              ? carteirasPermitidas[0]
                  .dashboard
              : null,
          )

          const evolucoesPermitidas =
            await Promise.all(
              investidoresPainel.map(
                (investidor) =>
                  obterEvolucaoConsolidada(
                    investidor.id,
                  ),
              ),
            )

          if (
            controller.signal
              .aborted
          ) {
            return
          }

          setEvolucao(
            evolucoesPermitidas.flat(),
          )
          setPerformance(null)

          return
        }

        // Performance consolidada é exclusiva do administrador.
        const performancePromise =
          obterPerformance()
            .then((dadosPerformance) => {
              if (!controller.signal.aborted) {
                setPerformance(dadosPerformance)
              }
            })
            .catch((error) => {
              console.error(error)

              if (!controller.signal.aborted) {
                setPerformance(null)
              }
            })

        // O painel principal não precisa aguardar evolução ou carteiras individuais.
        const evolucaoPromise = obterEvolucaoConsolidada()
          .then(value => { if (!controller.signal.aborted) setEvolucao(value) })
          .catch(error => { console.error(error); if (!controller.signal.aborted) setEvolucao([]) })
        const carteirasPromise = Promise.allSettled(
          todosInvestidores.map(async investidor => ({
            nome: investidor.nome,
            dashboard: await obterDashboardPorInvestidor(investidor.id),
          })),
        ).then(resultados => {
          if (controller.signal.aborted) return
          setCarteirasPainel(resultados.flatMap(resultado =>
            resultado.status === 'fulfilled' ? [resultado.value] : []))
        })
        const dadosDashboard = await obterDashboardConsolidado()
        void evolucaoPromise
        void carteirasPromise

        if (
          controller.signal
            .aborted
        ) {
          return
        }

        setDashboard(dadosDashboard)

        void performancePromise
      } catch (error) {
        console.error(error)

        if (
          !controller.signal
            .aborted
        ) {
          setDashboard(null)
          setEvolucao([])
          setPerformance(null)
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

    /*
     * Administração também utiliza
     * os dashboards individuais
     * para a manutenção de opções.
     */
    const podeConsultarCarteiras =
      possuiPermissao(
        'Carteira',
      ) ||
      possuiPermissao(
        'Analises',
      ) ||
      possuiPermissao(
        'Opcoes',
      ) ||
      possuiPermissao(
        'Administracao',
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

    await atualizarTudo()
  }

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
      {popupIa ? (
        <div role="alertdialog" aria-live="polite" style={{
          position: 'fixed', top: '22px', right: '22px', zIndex: 9999,
          maxWidth: '390px', padding: '16px', borderRadius: '12px',
          background: 'var(--panel)', border: '1px solid var(--border)',
          boxShadow: '0 12px 36px rgba(0,0,0,.3)', color: 'var(--text)',
        }}>
          <strong>Assistente IA — Aportiva</strong>
          <p>{popupIa}</p>
          <button type="button" onClick={() => setPopupIa(null)}>Fechar</button>
          {geracaoIa?.estado === 'concluido' ? (
            <button type="button" onClick={() => {
              setTelaAtual('assistente-ia')
              setPopupIa(null)
            }}>Ver análise</button>
          ) : null}
        </div>
      ) : null}
      <aside className="sidebar">
        <div className="marca">
          <img
            className="marca-icone"
            src="/aportiva-logo.svg"
            alt="Aportiva"
          />

        </div>

        <nav className="menu">
          {menuPrincipal.map(
            (item) => {
              const permitido =
                item.tela === 'administracao'
                  ? usuario?.perfil === 'Admin'
                  : permissaoNoInvestidor(item.permissao)

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
                'AssistenteIa',
              )

            return (
              <button
                className={`menu-item ${
                  telaAtual ===
                  'assistente-ia'
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
                    ? 'Assistente IA'
                    : 'Módulo não liberado para este usuário'
                }
                onClick={() =>
                  navegar(
                    'assistente-ia',
                  )
                }
              >
                <span className="menu-icone">
                  ✦
                </span>

                Assistente IA

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

          {usuario?.perfil === 'Admin' ? (
            <button className={`menu-item ${telaAtual === 'analise-estrategica' ? 'ativo' : ''}`}
              type="button" onClick={() => navegar('analise-estrategica')}>
              <span className="menu-icone">◎</span>
              Análise Estratégica
            </button>
          ) : null}

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

          <button
            className="menu-item"
            type="button"
            disabled={atualizandoDados}
            title="Atualizar todos os dados"
            onClick={() => void atualizarTudo()}
          >
            <span className="menu-icone">
              {atualizandoDados ? '◌' : '↻'}
            </span>
            {atualizandoDados ? 'Atualizando...' : 'Atualizar'}
          </button>

          <button
            className="menu-item"
            type="button"
            title="Sair da aplicação"
            onClick={() => { invalidateRequests(); void logout() }}
          >
            <span className="menu-icone">
              ↪
            </span>
            Sair
          </button>
        </nav>

        <div className="sidebar-rodape">
          <span className="sidebar-usuario">
            {usuario?.nome ?? 'Aportiva'}
          </span>
          <small className="sidebar-perfil">
            {usuario?.perfil === 'Admin' ? 'Administrador' : 'Investidor'}
          </small>
        </div>
      </aside>

      <main className="main">
        <div className="privacy-toolbar">
          <button type="button" className="privacy-toggle" title="Alterar senha" onClick={() => setAlterandoSenha(true)}>♙ Alterar senha</button>
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

        {alterandoSenha ? <AlterarSenhaModal onClose={() => setAlterandoSenha(false)} /> : null}

        {telaAtual ===
          'painel' &&
        permissaoNoInvestidor(
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
            performance={
              performance
            }
            apiDisponivel={
              apiDisponivel
            }
            erro={erro}
            somenteEvolucao={!podeVerDadosDeOutrosInvestidores}
          />
        ) : null}

        {telaAtual ===
          'carteira' &&
        permissaoNoInvestidor(
          'Carteira',
        ) ? (
          <CarteiraView
            investidores={
              investidores
            }
            dashboardConsolidado={
              usuario?.perfil === 'Admin'
                ? dashboard
                : null
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
            permitirTodos={
              usuario?.perfil === 'Admin'
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
          'ativos' &&
        permissaoNoInvestidor(
          'Ativos',
        ) ? (
          <AtivosView
            investidores={
              investidores
            }
            carteiras={
              carteirasPorInvestidor
            }
            dashboardConsolidado={
              usuario?.perfil === 'Admin'
                ? dashboard
                : null
            }
            permitirTodos={
              usuario?.perfil === 'Admin'
            }
          />
        ) : null}

        {telaAtual ===
          'opcoes' &&
        permissaoNoInvestidor(
          'Opcoes',
        ) ? (
          <OpcoesView
            investidores={
              investidoresPorPermissao(
                'Opcoes',
              )
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
            modo="consulta"
            permitirTodos={
              usuario?.perfil === 'Admin'
            }
          />
        ) : null}

        {telaAtual ===
          'proventos' &&
        permissaoNoInvestidor(
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
            modo="consulta"
            permitirTodos={
              usuario?.perfil === 'Admin'
            }
          />
        ) : null}

        {telaAtual === 'analises' && permissaoNoInvestidor('Analises') ? (
          <AnalisesView
            investidores={investidoresPorPermissao('Analises')}
            carteiras={carteirasPorInvestidor}
          />
        ) : null}

        {telaAtual === 'analise-estrategica' && usuario?.perfil === 'Admin' ? (
          <AnaliseEstrategicaView dashboard={dashboard} investidoresIds={todosInvestidores.map(i => i.id)} />
        ) : null}

        {telaAtual ===
          'assistente-ia' &&
        permissaoNoInvestidor(
          'AssistenteIa',
        ) ? (
          <AssistenteIaView
            podeGerar={usuario?.perfil === 'Admin'}
            gerando={geracaoIa?.estado === 'executando'}
            iniciarGeracao={iniciarGeracaoIa}
          />
        ) : null}

        {telaAtual ===
          'administracao' &&
        usuario?.perfil ===
          'Admin' ? (
          <AdministracaoView
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
            operacoes={
              operacoes
            }
            onSaveOperation={
              salvarOperacao
            }
            onOperationCreated={
              atualizarTudo
            }
            onDataChanged={
              atualizarTudo
            }
          />
        ) : null}
      </main>
    </div>
  )
}

export default App