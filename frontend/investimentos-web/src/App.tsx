import {
  useEffect,
  useState,
} from 'react'
import {
  atualizarOperacao,
  obterDashboardConsolidado,
  obterDashboardPorInvestidor,
  obterEvolucaoConsolidada,
  obterOperacoes,
} from './api/dashboardApi'
import { listarInvestidores } from './api/investidoresApi'
import { dashboardSnapshot } from './data/dashboardSnapshot'
import { CarteiraView } from './pages/Carteira/CarteiraView'
import { DashboardView } from './pages/Dashboard/DashboardView'
import { OperacoesView } from './pages/Operacoes/OperacoesView'
import { OpcoesView } from './pages/Opcoes/OpcoesView'
import { ProventosView } from './pages/Proventos/ProventosView'
import type {
  Dashboard,
  EvolucaoInvestidor,
  OperacaoCarteira,
} from './types/dashboard'
import type { Investidor } from './types/investidor'
import { definirOcultacaoValores } from './utils/formatters'
import { AdministracaoView } from './pages/Administracao/AdministracaoView'
import './styles/legacy.css'

type Tela =
  | 'painel'
  | 'carteira'
  | 'operacoes'
  | 'opcoes'
  | 'proventos'
  | 'administracao'
  
function App() {
  const [
    telaAtual,
    setTelaAtual,
  ] = useState<Tela>('painel')

  const [
    investidores,
    setInvestidores,
  ] = useState<Investidor[]>(
    [],
  )

  const [
    dashboard,
    setDashboard,
  ] = useState<Dashboard | null>(
    null,
  )

  const [
    carteirasPorInvestidor,
    setCarteirasPorInvestidor,
  ] = useState<
    Array<{
      nome: string
      dashboard: Dashboard
    }>
  >([])

  const [
    operacoes,
    setOperacoes,
  ] = useState<
    OperacaoCarteira[]
  >([])

  const [
    evolucao,
    setEvolucao,
  ] = useState<
    EvolucaoInvestidor[]
  >([])

  const [
    carregandoInvestidores,
    setCarregandoInvestidores,
  ] = useState(true)

  const [
    apiDisponivel,
    setApiDisponivel,
  ] = useState(true)

  const [
    erro,
    setErro,
  ] = useState<string | null>(
    null,
  )

  const [
    investidorSelecionado,
    setInvestidorSelecionado,
  ] = useState('')

  const [
    valoresOcultos,
    setValoresOcultos,
  ] = useState(false)

  definirOcultacaoValores(
    valoresOcultos,
  )

  useEffect(() => {
    async function carregarInvestidores() {
      try {
        setCarregandoInvestidores(
          true,
        )

        const dados =
          await listarInvestidores()

        if (dados.length > 0) {
          setInvestidores(
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

        setInvestidores(
          dashboardSnapshot.investidores.map(
            (nome) => ({
              id: nome,
              nome,
            }),
          ),
        )
      } finally {
        setCarregandoInvestidores(
          false,
        )
      }
    }

    carregarInvestidores()
  }, [])

  useEffect(() => {
    if (
      investidores.length >
        0 &&
      !investidores.some(
        (item) =>
          item.nome ===
          investidorSelecionado,
      )
    ) {
      setInvestidorSelecionado(
        investidores[0].nome,
      )
    }
  }, [
    investidores,
    investidorSelecionado,
  ])

  useEffect(() => {
    if (
      !apiDisponivel ||
      !investidorSelecionado
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
      return
    }

    obterOperacoes(
      investidor.id,
    )
      .then(setOperacoes)
      .catch((error) => {
        console.error(error)
        setOperacoes([])
      })
  }, [
    apiDisponivel,
    investidorSelecionado,
    investidores,
  ])

  useEffect(() => {
    if (!apiDisponivel) {
      return
    }

    const controller =
      new AbortController()

    async function carregarDashboard() {
      try {
        setErro(null)

        const [
          dados,
          dadosEvolucao,
        ] = await Promise.all(
          [
            obterDashboardConsolidado(),
            obterEvolucaoConsolidada(),
          ],
        )

        if (
          !controller.signal
            .aborted
        ) {
          setDashboard(dados)
          setEvolucao(
            dadosEvolucao,
          )
        }
      } catch (error) {
        console.error(error)

        if (
          !controller.signal
            .aborted
        ) {
          setApiDisponivel(
            false,
          )

          setDashboard(null)
          setEvolucao([])
          setErro(null)
        }
      }
    }

    carregarDashboard()

    return () =>
      controller.abort()
  }, [apiDisponivel])

  useEffect(() => {
    if (
      !apiDisponivel ||
      investidores.length ===
        0
    ) {
      return
    }

    async function carregarCarteirasPorInvestidor() {
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

    carregarCarteirasPorInvestidor()
  }, [
    apiDisponivel,
    investidores,
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
    ] = await Promise.all([
      obterDashboardPorInvestidor(
        investidor.id,
      ),

      obterOperacoes(
        investidor.id,
      ),
    ])

    setCarteirasPorInvestidor(
      (atuais) =>
        atuais.map((item) =>
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
    investidores.length ===
      0 ||
    dashboard === null

  if (
    carregandoInvestidores
  ) {
    return (
      <main className="estado-pagina">
        <div className="loader" />

        <p>
          Carregando
          investidores...
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
              Silvio Rocha
            </strong>

            <span>
              Dashboard de Carteira
            </span>
          </div>
        </div>

        <nav className="menu">
          <button
            className={`menu-item ${
              telaAtual ===
              'painel'
                ? 'ativo'
                : ''
            }`}
            type="button"
            onClick={() =>
              setTelaAtual(
                'painel',
              )
            }
          >
            <span className="menu-icone">
              ▣
            </span>

            Painel
          </button>

          <button
            className={`menu-item ${
              telaAtual ===
              'carteira'
                ? 'ativo'
                : ''
            }`}
            type="button"
            onClick={() =>
              setTelaAtual(
                'carteira',
              )
            }
          >
            <span className="menu-icone">
              ◫
            </span>

            Carteira
          </button>

          <button
            className={`menu-item ${
              telaAtual ===
              'operacoes'
                ? 'ativo'
                : ''
            }`}
            type="button"
            onClick={() =>
              setTelaAtual(
                'operacoes',
              )
            }
          >
            <span className="menu-icone">
              ↕
            </span>

            Operações
          </button>

          <button
            className={`menu-item ${
              telaAtual ===
              'opcoes'
                ? 'ativo'
                : ''
            }`}
            type="button"
            onClick={() =>
              setTelaAtual(
                'opcoes',
              )
            }
          >
            <span className="menu-icone">
              ◌
            </span>

            Opções
          </button>

          <button
            className={`menu-item ${
              telaAtual ===
              'proventos'
                ? 'ativo'
                : ''
            }`}
            type="button"
            onClick={() =>
              setTelaAtual(
                'proventos',
              )
            }
          >
            <span className="menu-icone">
              $
            </span>

            Proventos
          </button>

          <div className="menu-separator" />

<button
  className={`menu-item ${
    telaAtual === 'administracao'
      ? 'ativo'
      : ''
  }`}
  type="button"
  onClick={() =>
    setTelaAtual('administracao')
  }
>
  <span className="menu-icone">
    ⚙
  </span>
  Administração
</button>

        </nav>

        <div className="sidebar-rodape">
          <span>
            {
              dashboardSnapshot.referencia
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
        'painel' ? (
          <DashboardView
            dashboard={
              dashboard
            }
            carteiras={
              carteirasPorInvestidor
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
        'carteira' ? (
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
              dashboardSnapshot.timelinePorPessoa
            }
            saldosDisponiveis={
              dashboard?.saldosDisponiveis ??
              []
            }
          />
        ) : null}

        {telaAtual ===
        'operacoes' ? (
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
        'opcoes' ? (
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
        'proventos' ? (
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

        {telaAtual === 'administracao' ? (
  <AdministracaoView />
) : null}

      </main>
    </div>
  )
}

export default App