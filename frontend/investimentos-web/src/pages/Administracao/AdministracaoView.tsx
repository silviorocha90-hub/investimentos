import {
  useEffect,
  useMemo,
  useState,
} from 'react'

import type {
  FormEvent,
} from 'react'

import {
  atualizarAtivo,
  criarAtivo,
  excluirAtivo,
  obterAdministracao,
} from '../../api/administracaoApi'

import type {
  Administracao,
  AtivoAdministracao,
} from '../../types/administracao'

import type {
  Dashboard,
  OperacaoCarteira,
} from '../../types/dashboard'

import type {
  Investidor,
} from '../../types/investidor'

import {
  OperacoesView,
} from '../Operacoes/OperacoesView'

import {
  OpcoesView,
} from '../Opcoes/OpcoesView'

import {
  ProventosView,
} from '../Proventos/ProventosView'

import {
  AtivosTab,
} from './components/AtivosTab'

import {
  InvestidoresTab,
} from './components/InvestidoresTab'

import {
  CarteiraTab,
} from './components/CarteiraTab'

import {
  ParametrosTab,
} from './components/ParametrosTab'

import {
  UsuariosTab,
} from './components/UsuariosTab'

import {
  AtivoModal,
} from './components/AtivoModal'

import './Administracao.css'

type AbaAdministracao =
  | 'ativos'
  | 'operacoes'
  | 'opcoes'
  | 'proventos'
  | 'investidores'
  | 'carteira'
  | 'parametros'
  | 'usuarios'

type ModoModalAtivo =
  | 'novo'
  | 'editar'

export interface ModalAtivoState {
  modo:
    ModoModalAtivo

  ativo?:
    AtivoAdministracao
}

export interface FormularioAtivo {
  ticker: string
  nome: string
  tipoAtivoId: string
  cotacao: string
  dataCotacao: string
  valorPatrimonial: string
  dataValorPatrimonial: string
}

interface AdministracaoViewProps {
  investidores:
    Investidor[]

  carteiras:
    ReadonlyArray<{
      nome: string
      dashboard: Dashboard
    }>

  selectedInvestor:
    string

  onSelectInvestor:
    (
      investidor: string,
    ) => void

  operacoes:
    OperacaoCarteira[]

  onSaveOperation:
    (
      operacao: Pick<
        OperacaoCarteira,
        | 'id'
        | 'data'
        | 'quantidade'
        | 'precoUnitario'
        | 'taxas'
      >,
    ) => Promise<void>
}

export function AdministracaoView({
  investidores,
  carteiras,
  selectedInvestor,
  onSelectInvestor,
  operacoes,
  onSaveOperation,
}: AdministracaoViewProps) {
  const hoje =
    new Date()
      .toISOString()
      .slice(
        0,
        10,
      )

  const [
    dados,
    setDados,
  ] =
    useState<
      Administracao | null
    >(null)

  const [
    aba,
    setAba,
  ] =
    useState<
      AbaAdministracao
    >('ativos')

  const [
    carregando,
    setCarregando,
  ] =
    useState(true)

  const [
    salvando,
    setSalvando,
  ] =
    useState(false)

  const [
    erro,
    setErro,
  ] =
    useState<
      string | null
    >(null)

  const [
    filtroInvestidor,
    setFiltroInvestidor,
  ] =
    useState(
      'TODOS',
    )

  const [
    filtroAtivo,
    setFiltroAtivo,
  ] =
    useState(
      'TODOS',
    )

  const [
    filtroClasse,
    setFiltroClasse,
  ] =
    useState(
      'TODOS',
    )

  const [
    filtroTipo,
    setFiltroTipo,
  ] =
    useState(
      'TODOS',
    )

  const [
    pagina,
    setPagina,
  ] =
    useState(1)

  const [
    investidorSelecionadoId,
    setInvestidorSelecionadoId,
  ] =
    useState<
      string | null
    >(null)

  const [
    modalAtivo,
    setModalAtivo,
  ] =
    useState<
      ModalAtivoState | null
    >(null)

  const [
    formularioAtivo,
    setFormularioAtivo,
  ] =
    useState<FormularioAtivo>({
      ticker: '',
      nome: '',
      tipoAtivoId: '',
      cotacao: '',
      dataCotacao:
        hoje,

      valorPatrimonial: '',

      dataValorPatrimonial:
        hoje,
    })

  async function carregar() {
    try {
      setCarregando(
        true,
      )

      setErro(
        null,
      )

      const resultado =
        await obterAdministracao()

      setDados(
        resultado,
      )

      setInvestidorSelecionadoId(
        (atual) =>
          atual &&
          resultado
            .investidores
            .some(
              (
                investidor,
              ) =>
                investidor.id ===
                atual,
            )
            ? atual
            : resultado
                .investidores[0]
                ?.id ??
              null,
      )
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível carregar a administração.',
      )
    } finally {
      setCarregando(
        false,
      )
    }
  }

  useEffect(() => {
    void carregar()
  }, [])

  const tiposAtivos =
    useMemo(
      () =>
        (
          dados?.tiposAtivo ??
          []
        )
          .filter(
            (tipo) =>
              tipo.ativo,
          )
          .slice()
          .sort(
            (a, b) =>
              a.nome.localeCompare(
                b.nome,
                'pt-BR',
              ),
          ),
      [dados],
    )

  function trocarAba(
    novaAba:
      AbaAdministracao,
  ) {
    setAba(
      novaAba,
    )

    setErro(
      null,
    )
  }

  function abrirNovoAtivo() {
    const primeiroTipo =
      tiposAtivos[0]

    setFormularioAtivo({
      ticker: '',
      nome: '',

      tipoAtivoId:
        primeiroTipo
          ? String(
              primeiroTipo.id,
            )
          : '',

      cotacao: '',

      dataCotacao:
        hoje,

      valorPatrimonial: '',

      dataValorPatrimonial:
        hoje,
    })

    setErro(
      null,
    )

    setModalAtivo({
      modo:
        'novo',
    })
  }

  function abrirEditarAtivo(
    ativo:
      AtivoAdministracao,
  ) {
    setFormularioAtivo({
      ticker:
        ativo.ticker,

      nome:
        ativo.nome ||
        ativo.ticker,

      tipoAtivoId:
        String(
          ativo.tipoAtivoId,
        ),

      cotacao:
        ativo.cotacaoAtual ==
        null
          ? ''
          : String(
              ativo.cotacaoAtual,
            ),

      dataCotacao:
        hoje,

      valorPatrimonial:
        (
          ativo.valorPatrimonialAtual ??
          ativo.valorAtual
        ).toLocaleString(
          'pt-BR',
          {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          },
        ),

      dataValorPatrimonial:
        hoje,
    })

    setErro(
      null,
    )

    setModalAtivo({
      modo:
        'editar',

      ativo,
    })
  }

  async function excluirAtivoSelecionado(
    ativo:
      AtivoAdministracao,
  ) {
    const confirmado =
      window.confirm(
        `Excluir o ativo ${ativo.ticker}? Esta ação só será permitida se ele não possuir histórico de operações, proventos ou opções.`,
      )

    if (!confirmado) {
      return
    }

    try {
      setSalvando(
        true,
      )

      setErro(
        null,
      )

      await excluirAtivo(
        ativo.id,
      )

      await carregar()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível excluir o ativo.',
      )
    } finally {
      setSalvando(
        false,
      )
    }
  }

  function fecharModalAtivo() {
    if (salvando) {
      return
    }

    setModalAtivo(
      null,
    )

    setErro(
      null,
    )
  }

  async function salvarAtivo(
    event:
      FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    if (!modalAtivo) {
      return
    }

    if (
      !formularioAtivo
        .ticker
        .trim()
    ) {
      setErro(
        'Informe o ticker do ativo.',
      )

      return
    }

    if (
      !formularioAtivo
        .tipoAtivoId
    ) {
      setErro(
        'Selecione o tipo do ativo.',
      )

      return
    }

    const tipoSelecionado =
      tiposAtivos.find(
        (tipo) =>
          String(tipo.id) ===
          formularioAtivo.tipoAtivoId,
      )

    const tickerNormalizado =
      formularioAtivo.ticker
        .trim()
        .toUpperCase()

    const tipoNormalizado =
      tipoSelecionado?.codigo
        .trim()
        .toUpperCase() ??
      ''

    const outroInvestimento =
      tipoNormalizado ===
        'PREVIDENCIA' ||
      tipoNormalizado ===
        'FMP' ||
      tickerNormalizado.includes(
        'CDB',
      ) ||
      tickerNormalizado.includes(
        'ELETROBRAS',
      )

    const valorPatrimonial =
      !outroInvestimento ||
      formularioAtivo.valorPatrimonial.trim() === ''
        ? null
        : Number(
            formularioAtivo.valorPatrimonial
              .replace(/\./g, '')
              .replace(',', '.'),
          )

    if (
      valorPatrimonial != null &&
      (
        Number.isNaN(valorPatrimonial) ||
        valorPatrimonial < 0
      )
    ) {
      setErro(
        'Informe um valor atual válido.',
      )

      return
    }

    const cotacao =
      outroInvestimento ||
      formularioAtivo
        .cotacao
        .trim() === ''
        ? null
        : Number(
            formularioAtivo
              .cotacao,
          )

    if (
      cotacao != null &&
      (
        Number.isNaN(
          cotacao,
        ) ||
        cotacao < 0
      )
    ) {
      setErro(
        'Informe uma cotação válida.',
      )

      return
    }

    try {
      setSalvando(
        true,
      )

      setErro(
        null,
      )

      const nome =
        formularioAtivo
          .nome
          .trim() ||
        formularioAtivo
          .ticker
          .trim()
          .toUpperCase()

      if (
        modalAtivo.modo ===
        'novo'
      ) {
        await criarAtivo({
          ticker:
            formularioAtivo
              .ticker
              .trim()
              .toUpperCase(),

          nome,

          tipoAtivoId:
            Number(
              formularioAtivo
                .tipoAtivoId,
            ),

          cotacao,

          dataCotacao:
            cotacao == null
              ? null
              : formularioAtivo
                  .dataCotacao,

          valorPatrimonial,

          dataValorPatrimonial:
            valorPatrimonial == null
              ? null
              : formularioAtivo
                  .dataValorPatrimonial,
        })
      } else {
        await atualizarAtivo(
          modalAtivo
            .ativo!.id,
          {
            nome,

            tipoAtivoId:
              Number(
                formularioAtivo
                  .tipoAtivoId,
              ),

            cotacao,

            dataCotacao:
              cotacao == null
                ? null
                : formularioAtivo
                    .dataCotacao,

            valorPatrimonial,

            dataValorPatrimonial:
              valorPatrimonial == null
                ? null
                : formularioAtivo
                    .dataValorPatrimonial,
          },
        )
      }

      setModalAtivo(
        null,
      )

      setAba(
        'ativos',
      )

      await carregar()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível salvar o ativo.',
      )
    } finally {
      setSalvando(
        false,
      )
    }
  }

  if (carregando) {
    return (
      <section className="admin-page">
        <div className="admin-state">
          Carregando administração...
        </div>
      </section>
    )
  }

  if (!dados) {
    return (
      <section className="admin-page">
        <div className="admin-state admin-error">
          {erro ??
            'Administração indisponível.'}
        </div>
      </section>
    )
  }

  return (
    <section className="admin-page">
      <header className="admin-heading">
        <div>
          <h1>
            Administração
          </h1>
        </div>
      </header>

      <nav className="admin-main-tabs">
        <button
          type="button"
          className={
            aba ===
            'ativos'
              ? 'active'
              : ''
          }
          onClick={() =>
            trocarAba(
              'ativos',
            )
          }
        >
          Ativos
        </button>

        <button
          type="button"
          className={
            aba ===
            'operacoes'
              ? 'active'
              : ''
          }
          onClick={() =>
            trocarAba(
              'operacoes',
            )
          }
        >
          Operações
        </button>

        <button
          type="button"
          className={
            aba ===
            'opcoes'
              ? 'active'
              : ''
          }
          onClick={() =>
            trocarAba(
              'opcoes',
            )
          }
        >
          Opções
        </button>

        <button
          type="button"
          className={
            aba ===
            'proventos'
              ? 'active'
              : ''
          }
          onClick={() =>
            trocarAba(
              'proventos',
            )
          }
        >
          Proventos
        </button>

        <button
          type="button"
          className={
            aba ===
            'investidores'
              ? 'active'
              : ''
          }
          onClick={() =>
            trocarAba(
              'investidores',
            )
          }
        >
          Investidores
        </button>

        <button
          type="button"
          className={
            aba ===
            'carteira'
              ? 'active'
              : ''
          }
          onClick={() =>
            trocarAba(
              'carteira',
            )
          }
        >
          DARF
        </button>

        <button
          type="button"
          className={
            aba ===
            'parametros'
              ? 'active'
              : ''
          }
          onClick={() =>
            trocarAba(
              'parametros',
            )
          }
        >
          Parâmetros
        </button>

        <button
          type="button"
          className={
            aba ===
            'usuarios'
              ? 'active'
              : ''
          }
          onClick={() =>
            trocarAba(
              'usuarios',
            )
          }
        >
          Usuários
        </button>
      </nav>

      {aba ===
      'ativos' ? (
        <AtivosTab
          dados={
            dados
          }
          erro={
            erro
          }
          filtroInvestidor={
            filtroInvestidor
          }
          filtroAtivo={
            filtroAtivo
          }
          filtroClasse={
            filtroClasse
          }
          filtroTipo={
            filtroTipo
          }
          pagina={
            pagina
          }
          setFiltroInvestidor={
            setFiltroInvestidor
          }
          setFiltroAtivo={
            setFiltroAtivo
          }
          setFiltroClasse={
            setFiltroClasse
          }
          setFiltroTipo={
            setFiltroTipo
          }
          setPagina={
            setPagina
          }
          abrirNovoAtivo={
            abrirNovoAtivo
          }
          abrirEditarAtivo={
            abrirEditarAtivo
          }
          excluirAtivo={
            excluirAtivoSelecionado
          }
          excluindo={
            salvando
          }
        />
      ) : null}

      {aba ===
      'operacoes' ? (
        <OperacoesView
          investidores={
            investidores
          }
          selectedInvestor={
            selectedInvestor
          }
          onSelectInvestor={
            onSelectInvestor
          }
          operacoes={
            operacoes
          }
          onSaveOperation={
            onSaveOperation
          }
        />
      ) : null}

      {aba ===
      'opcoes' ? (
        <OpcoesView
          investidores={
            investidores
          }
          carteiras={
            carteiras
          }
          selectedInvestor={
            selectedInvestor
          }
          onSelectInvestor={
            onSelectInvestor
          }
          modo="administracao"
        />
      ) : null}

      {aba ===
      'proventos' ? (
        <ProventosView
          investidores={
            investidores
          }
          selectedInvestor={
            selectedInvestor
          }
          onSelectInvestor={
            onSelectInvestor
          }
          modo="administracao"
        />
      ) : null}

      {aba ===
      'investidores' ? (
        <InvestidoresTab
          dados={
            dados
          }
          investidorSelecionadoId={
            investidorSelecionadoId
          }
          setInvestidorSelecionadoId={
            setInvestidorSelecionadoId
          }
          recarregar={
            carregar
          }
        />
      ) : null}

      {aba ===
      'carteira' ? (
        <CarteiraTab
          dados={
            dados
          }
          recarregar={
            carregar
          }
        />
      ) : null}

      {aba ===
      'parametros' ? (
        <ParametrosTab
          dados={
            dados
          }
          recarregar={
            carregar
          }
        />
      ) : null}

      {aba ===
      'usuarios' ? (
        <UsuariosTab
          dados={
            dados
          }
        />
      ) : null}

      {modalAtivo ? (
        <AtivoModal
          modalAtivo={
            modalAtivo
          }
          formularioAtivo={
            formularioAtivo
          }
          setFormularioAtivo={
            setFormularioAtivo
          }
          tipos={
            tiposAtivos
          }
          salvando={
            salvando
          }
          erro={
            erro
          }
          fechar={
            fecharModalAtivo
          }
          salvar={
            salvarAtivo
          }
        />
      ) : null}
    </section>
  )
}