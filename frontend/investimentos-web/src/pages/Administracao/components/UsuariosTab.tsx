import {
  useEffect,
  useMemo,
  useState,
} from 'react'

import {
  alterarAcessosUsuario,
  aprovarUsuario,
  bloquearUsuario,
  desbloquearUsuario,
  listarUsuarios,
  rejeitarUsuario,
} from '../../../api/usuariosApi'

import type {
  Administracao,
} from '../../../types/administracao'

import type {
  PermissaoSistema,
  UsuarioAdministracao,
} from '../../../types/usuario'

import {
  permissoesVisualizacao,
} from '../../../types/usuario'

import {
  UsuarioModal,
} from './UsuarioModal'

import type {
  FormularioUsuario,
} from './UsuarioModal'

import './Usuarios.css'

type FiltroUsuario =
  | 'TODOS'
  | 'Pendente'
  | 'Ativo'
  | 'Bloqueado'
  | 'Rejeitado'

interface UsuariosTabProps {
  dados: Administracao
}

export function UsuariosTab({
  dados,
}: UsuariosTabProps) {
  const [
    usuarios,
    setUsuarios,
  ] =
    useState<UsuarioAdministracao[]>(
      [],
    )

  const [
    filtro,
    setFiltro,
  ] =
    useState<FiltroUsuario>(
      'TODOS',
    )

  const [
    carregando,
    setCarregando,
  ] =
    useState(true)

  const [
    processandoId,
    setProcessandoId,
  ] =
    useState<string | null>(
      null,
    )

  const [
    erro,
    setErro,
  ] =
    useState<string | null>(
      null,
    )

  const [
    usuarioEditando,
    setUsuarioEditando,
  ] =
    useState<UsuarioAdministracao | null>(
      null,
    )

  const [
    formulario,
    setFormulario,
  ] =
    useState<FormularioUsuario>({
      perfil: 'Usuario',
      permissoes: [],
      investidoresIds: [],
    })

  async function carregar() {
    try {
      setCarregando(true)
      setErro(null)

      const resultado =
        await listarUsuarios()

      setUsuarios(resultado)
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível carregar os usuários.',
      )
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    void carregar()
  }, [])

  const resumo =
    useMemo(
      () => ({
        total:
          usuarios.length,

        pendentes:
          usuarios.filter(
            (usuario) =>
              usuario.status ===
              'Pendente',
          ).length,

        ativos:
          usuarios.filter(
            (usuario) =>
              usuario.status ===
              'Ativo',
          ).length,

        bloqueados:
          usuarios.filter(
            (usuario) =>
              usuario.status ===
              'Bloqueado',
          ).length,
      }),
      [usuarios],
    )

  const usuariosFiltrados =
    useMemo(
      () =>
        usuarios.filter(
          (usuario) =>
            filtro === 'TODOS' ||
            usuario.status === filtro,
        ),
      [
        usuarios,
        filtro,
      ],
    )

  function abrirAcessos(
    usuario:
      UsuarioAdministracao,
  ) {
    if (
      usuario.perfil ===
      'Admin'
    ) {
      return
    }

    setUsuarioEditando(
      usuario,
    )

    setFormulario({
      perfil:
        usuario.perfil,

      permissoes:
        usuario.perfil === 'Usuario'
          ? [...permissoesVisualizacao]
          : usuario.permissoes as
              PermissaoSistema[],

      investidoresIds: [
        ...usuario
          .investidoresIds,
      ],
    })

    setErro(null)
  }

  function fecharModal() {
    if (processandoId) {
      return
    }

    setUsuarioEditando(null)
    setErro(null)
  }

  async function executarAcao(
    usuario:
      UsuarioAdministracao,
    acao: () =>
      Promise<void>,
  ) {
    try {
      setProcessandoId(
        usuario.id,
      )

      setErro(null)

      await acao()

      await carregar()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível realizar a operação.',
      )
    } finally {
      setProcessandoId(null)
    }
  }

  async function rejeitar(
    usuario:
      UsuarioAdministracao,
  ) {
    const confirmado =
      window.confirm(
        `Rejeitar o cadastro de ${usuario.nome}?`,
      )

    if (!confirmado) {
      return
    }

    await executarAcao(
      usuario,
      async () => {
        await rejeitarUsuario(
          usuario.id,
        )
      },
    )
  }

  async function bloquear(
    usuario:
      UsuarioAdministracao,
  ) {
    if (
      usuario.perfil ===
      'Admin'
    ) {
      return
    }

    const confirmado =
      window.confirm(
        `Bloquear o acesso de ${usuario.nome}?`,
      )

    if (!confirmado) {
      return
    }

    await executarAcao(
      usuario,
      async () => {
        await bloquearUsuario(
          usuario.id,
        )
      },
    )
  }

  async function desbloquear(
    usuario:
      UsuarioAdministracao,
  ) {
    await executarAcao(
      usuario,
      async () => {
        await desbloquearUsuario(
          usuario.id,
        )
      },
    )
  }

  async function salvarAcessos() {
    if (!usuarioEditando) {
      return
    }

    try {
      setProcessandoId(
        usuarioEditando.id,
      )

      setErro(null)

      await alterarAcessosUsuario(
        usuarioEditando.id,
        formulario,
      )

      if (
        usuarioEditando.status ===
        'Pendente'
      ) {
        await aprovarUsuario(
          usuarioEditando.id,
        )
      }

      setUsuarioEditando(null)

      await carregar()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : usuarioEditando.status ===
              'Pendente'
            ? 'Não foi possível configurar e aprovar o usuário.'
            : 'Não foi possível salvar os acessos.',
      )
    } finally {
      setProcessandoId(null)
    }
  }

  function quantidadeAcessos(
    usuario:
      UsuarioAdministracao,
  ) {
    if (
      usuario.perfil ===
      'Admin'
    ) {
      return 'Todos'
    }

    if (
      usuario.permissoes.length ===
      0
    ) {
      return 'Não configurado'
    }

    return `${usuario.permissoes.length} tela(s)`
  }

  function quantidadeInvestidores(
    usuario:
      UsuarioAdministracao,
  ) {
    if (
      usuario.perfil ===
      'Admin'
    ) {
      return 'Todos'
    }

    if (
      usuario.investidoresIds
        .length === 0
    ) {
      return 'Não configurado'
    }

    return `${usuario.investidoresIds.length} investidor(es)`
  }

  function formatarData(
    data?: string | null,
  ) {
    if (!data) {
      return '—'
    }

    return new Date(
      data,
    ).toLocaleDateString(
      'pt-BR',
    )
  }

  if (carregando) {
    return (
      <div className="admin-state">
        Carregando usuários...
      </div>
    )
  }

  return (
    <div className="users-page">
      <section className="users-summary-grid">
        <button
          type="button"
          className={
            filtro === 'TODOS'
              ? 'users-summary-card active'
              : 'users-summary-card'
          }
          onClick={() =>
            setFiltro('TODOS')
          }
        >
          <span>
            Usuários
          </span>

          <strong>
            {resumo.total}
          </strong>

          <small>
            Total cadastrado
          </small>
        </button>

        <button
          type="button"
          className={
            filtro === 'Pendente'
              ? 'users-summary-card users-summary-warning active'
              : 'users-summary-card users-summary-warning'
          }
          onClick={() =>
            setFiltro(
              'Pendente',
            )
          }
        >
          <span>
            Pendentes
          </span>

          <strong>
            {resumo.pendentes}
          </strong>

          <small>
            Aguardando liberação
          </small>
        </button>

        <button
          type="button"
          className={
            filtro === 'Ativo'
              ? 'users-summary-card users-summary-success active'
              : 'users-summary-card users-summary-success'
          }
          onClick={() =>
            setFiltro('Ativo')
          }
        >
          <span>
            Ativos
          </span>

          <strong>
            {resumo.ativos}
          </strong>

          <small>
            Com acesso liberado
          </small>
        </button>

        <button
          type="button"
          className={
            filtro === 'Bloqueado'
              ? 'users-summary-card users-summary-danger active'
              : 'users-summary-card users-summary-danger'
          }
          onClick={() =>
            setFiltro(
              'Bloqueado',
            )
          }
        >
          <span>
            Bloqueados
          </span>

          <strong>
            {resumo.bloqueados}
          </strong>

          <small>
            Sem acesso ao sistema
          </small>
        </button>
      </section>

      <section className="users-panel panel">
        <header className="users-panel-header">
          <div>
            <strong>
              Gerenciamento de usuários
            </strong>

            <span>
              Perfis, permissões e
              acesso aos investidores
            </span>
          </div>

          <div className="users-filter-tabs">
            <button
              type="button"
              className={
                filtro === 'TODOS'
                  ? 'active'
                  : ''
              }
              onClick={() =>
                setFiltro(
                  'TODOS',
                )
              }
            >
              Todos
            </button>

            <button
              type="button"
              className={
                filtro ===
                'Pendente'
                  ? 'active'
                  : ''
              }
              onClick={() =>
                setFiltro(
                  'Pendente',
                )
              }
            >
              Pendentes

              {resumo.pendentes >
              0 ? (
                <span>
                  {
                    resumo.pendentes
                  }
                </span>
              ) : null}
            </button>

            <button
              type="button"
              className={
                filtro === 'Ativo'
                  ? 'active'
                  : ''
              }
              onClick={() =>
                setFiltro(
                  'Ativo',
                )
              }
            >
              Ativos
            </button>

            <button
              type="button"
              className={
                filtro ===
                'Bloqueado'
                  ? 'active'
                  : ''
              }
              onClick={() =>
                setFiltro(
                  'Bloqueado',
                )
              }
            >
              Bloqueados
            </button>

            <button
              type="button"
              className={
                filtro ===
                'Rejeitado'
                  ? 'active'
                  : ''
              }
              onClick={() =>
                setFiltro(
                  'Rejeitado',
                )
              }
            >
              Rejeitados
            </button>
          </div>
        </header>

        {erro &&
        !usuarioEditando ? (
          <div className="admin-inline-error">
            {erro}
          </div>
        ) : null}

        <div className="users-list">
          {usuariosFiltrados.map(
            (usuario) => {
              const processando =
                processandoId ===
                usuario.id

              const admin =
                usuario.perfil ===
                'Admin'

              return (
                <article
                  key={usuario.id}
                  className="user-card"
                >
                  <div className="user-card-top">
                    <div className="user-identity">
                      <div className="user-avatar">
                        {usuario.nome
                          .trim()
                          .charAt(0)
                          .toUpperCase()}
                      </div>

                      <div>
                        <div className="user-name-row">
                          <strong>
                            {
                              usuario.nome
                            }
                          </strong>

                          {admin ? (
                            <span className="user-admin-badge">
                              Administrador
                            </span>
                          ) : null}
                        </div>

                        <span>
                          {
                            usuario.email
                          }
                        </span>
                      </div>
                    </div>

                    <span
                      className={`user-status status-${usuario.status.toLowerCase()}`}
                    >
                      {usuario.status}
                    </span>
                  </div>

                  <div className="user-details-grid">
                    <div>
                      <span>
                        Perfil
                      </span>

                      <strong>
                        {admin
                          ? 'Administrador'
                          : 'Usuário'}
                      </strong>
                    </div>

                    <div>
                      <span>
                        Telas
                      </span>

                      <strong
                        className={
                          !admin &&
                          usuario
                            .permissoes
                            .length ===
                            0
                            ? 'user-detail-warning'
                            : ''
                        }
                      >
                        {
                          quantidadeAcessos(
                            usuario,
                          )
                        }
                      </strong>
                    </div>

                    <div>
                      <span>
                        Investidores
                      </span>

                      <strong
                        className={
                          !admin &&
                          usuario
                            .investidoresIds
                            .length ===
                            0
                            ? 'user-detail-warning'
                            : ''
                        }
                      >
                        {
                          quantidadeInvestidores(
                            usuario,
                          )
                        }
                      </strong>
                    </div>

                    <div>
                      <span>
                        Cadastro
                      </span>

                      <strong>
                        {formatarData(
                          usuario
                            .dataCadastro,
                        )}
                      </strong>
                    </div>

                    <div>
                      <span>
                        Último acesso
                      </span>

                      <strong>
                        {formatarData(
                          usuario
                            .ultimoLogin,
                        )}
                      </strong>
                    </div>
                  </div>

                  <footer className="user-card-footer">
                    {admin ? (
                      <div className="user-admin-protected">
                        <span>
                          ✓
                        </span>

                        Perfil administrativo
                        protegido
                      </div>
                    ) : (
                      <>
                        <div className="user-card-hint">
                          {usuario.status ===
                          'Pendente'
                            ? 'Configure os acessos antes de liberar o usuário.'
                            : usuario.status ===
                                'Bloqueado'
                              ? 'Este usuário está sem acesso ao sistema.'
                              : 'Acesso configurável pelo administrador.'}
                        </div>

                        <div className="user-card-actions">
                          {usuario.status ===
                          'Pendente' ? (
                            <>
                              <button
                                type="button"
                                className="user-secondary-action user-danger-action"
                                disabled={
                                  processando
                                }
                                onClick={() =>
                                  void rejeitar(
                                    usuario,
                                  )
                                }
                              >
                                Rejeitar
                              </button>

                              <button
                                type="button"
                                className="user-primary-action"
                                disabled={
                                  processando
                                }
                                onClick={() =>
                                  abrirAcessos(
                                    usuario,
                                  )
                                }
                              >
                                Configurar acesso
                              </button>
                            </>
                          ) : null}

                          {usuario.status ===
                          'Ativo' ? (
                            <>
                              <button
                                type="button"
                                className="user-secondary-action user-danger-action"
                                disabled={
                                  processando
                                }
                                onClick={() =>
                                  void bloquear(
                                    usuario,
                                  )
                                }
                              >
                                Bloquear
                              </button>

                              <button
                                type="button"
                                className="user-primary-action"
                                disabled={
                                  processando
                                }
                                onClick={() =>
                                  abrirAcessos(
                                    usuario,
                                  )
                                }
                              >
                                Gerenciar acesso
                              </button>
                            </>
                          ) : null}

                          {usuario.status ===
                          'Bloqueado' ? (
                            <>
                              <button
                                type="button"
                                className="user-secondary-action"
                                disabled={
                                  processando
                                }
                                onClick={() =>
                                  abrirAcessos(
                                    usuario,
                                  )
                                }
                              >
                                Gerenciar acesso
                              </button>

                              <button
                                type="button"
                                className="user-primary-action"
                                disabled={
                                  processando
                                }
                                onClick={() =>
                                  void desbloquear(
                                    usuario,
                                  )
                                }
                              >
                                Desbloquear
                              </button>
                            </>
                          ) : null}
                        </div>
                      </>
                    )}
                  </footer>
                </article>
              )
            },
          )}

          {usuariosFiltrados.length ===
          0 ? (
            <div className="users-empty">
              <strong>
                Nenhum usuário
                encontrado
              </strong>

              <span>
                Não existem usuários
                para o filtro
                selecionado.
              </span>
            </div>
          ) : null}
        </div>
      </section>

      {usuarioEditando ? (
        <UsuarioModal
          usuario={
            usuarioEditando
          }
          investidores={
            dados.investidores
          }
          formulario={
            formulario
          }
          setFormulario={
            setFormulario
          }
          salvando={
            processandoId ===
            usuarioEditando.id
          }
          erro={erro}
          fechar={
            fecharModal
          }
          salvar={() => {
            void salvarAcessos()
          }}
        />
      ) : null}
    </div>
  )
}