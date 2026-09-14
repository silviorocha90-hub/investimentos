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
  obterAdministracao,
} from '../../api/administracaoApi'

import type {
  Administracao,
  AtivoAdministracao,
} from '../../types/administracao'

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
  | 'investidores'
  | 'carteira'
  | 'parametros'
  | 'usuarios'

type ModoModalAtivo =
  | 'novo'
  | 'editar'

export interface ModalAtivoState {
  modo: ModoModalAtivo
  ativo?: AtivoAdministracao
}

export interface FormularioAtivo {
  ticker: string
  nome: string
  tipoAtivoId: string
  cotacao: string
  dataCotacao: string
}

export function AdministracaoView() {
  const hoje =
    new Date()
      .toISOString()
      .slice(0, 10)

  const [
    dados,
    setDados,
  ] =
    useState<Administracao | null>(
      null,
    )

  const [
    aba,
    setAba,
  ] =
    useState<AbaAdministracao>(
      'ativos',
    )

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
    useState<string | null>(
      null,
    )

  const [
    filtroAtivo,
    setFiltroAtivo,
  ] =
    useState('TODOS')

  const [
    filtroClasse,
    setFiltroClasse,
  ] =
    useState('TODOS')

  const [
    filtroTipo,
    setFiltroTipo,
  ] =
    useState('TODOS')

  const [
    pagina,
    setPagina,
  ] =
    useState(1)

  const [
    investidorSelecionadoId,
    setInvestidorSelecionadoId,
  ] =
    useState<string | null>(
      null,
    )

  const [
    modalAtivo,
    setModalAtivo,
  ] =
    useState<ModalAtivoState | null>(
      null,
    )

  const [
    formularioAtivo,
    setFormularioAtivo,
  ] =
    useState<FormularioAtivo>({
      ticker: '',
      nome: '',
      tipoAtivoId: '',
      cotacao: '',
      dataCotacao: hoje,
    })

  async function carregar() {
    try {
      setCarregando(true)
      setErro(null)

      const resultado =
        await obterAdministracao()

      setDados(resultado)

      setInvestidorSelecionadoId(
        (atual) =>
          atual &&
          resultado.investidores.some(
            (investidor) =>
              investidor.id === atual,
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
      setCarregando(false)
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
    setAba(novaAba)
    setErro(null)
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
      dataCotacao: hoje,
    })

    setErro(null)

    setModalAtivo({
      modo: 'novo',
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
        ativo.dataCotacao
          ? ativo.dataCotacao.slice(
              0,
              10,
            )
          : hoje,
    })

    setErro(null)

    setModalAtivo({
      modo: 'editar',
      ativo,
    })
  }

  function fecharModalAtivo() {
    if (salvando) {
      return
    }

    setModalAtivo(null)
    setErro(null)
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

    const cotacao =
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
      setSalvando(true)
      setErro(null)

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
          },
        )
      }

      setModalAtivo(null)
      setAba('ativos')

      await carregar()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível salvar o ativo.',
      )
    } finally {
      setSalvando(false)
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
            aba === 'ativos'
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
          Carteira
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

      {aba === 'ativos' ? (
        <AtivosTab
          dados={dados}
          erro={erro}
          filtroAtivo={
            filtroAtivo
          }
          filtroClasse={
            filtroClasse
          }
          filtroTipo={
            filtroTipo
          }
          pagina={pagina}
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
        />
      ) : null}

      {aba ===
      'investidores' ? (
        <InvestidoresTab
          dados={dados}
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

      {aba === 'carteira' ? (
        <CarteiraTab
          dados={dados}
        />
      ) : null}

      {aba ===
      'parametros' ? (
        <ParametrosTab
          dados={dados}
        />
      ) : null}

      {aba ===
      'usuarios' ? (
        <UsuariosTab
          dados={dados}
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
          erro={erro}
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