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
  UsuarioModal,
} from './UsuarioModal'

import type {
  FormularioUsuario,
} from './UsuarioModal'

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

  const usuariosFiltrados =
    useMemo(
      () =>
        usuarios.filter(
          (usuario) =>
            filtro ===
              'TODOS' ||
            usuario.status ===
              filtro,
        ),
      [
        usuarios,
        filtro,
      ],
    )

  const pendentes =
    usuarios.filter(
      (usuario) =>
        usuario.status ===
        'Pendente',
    ).length

  function editar(
    usuario:
      UsuarioAdministracao,
  ) {
    setUsuarioEditando(
      usuario,
    )

    setFormulario({
      perfil:
        usuario.perfil,

      permissoes:
        usuario.permissoes as
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

  async function aprovar(
    usuario:
      UsuarioAdministracao,
  ) {
    await executarAcao(
      usuario,
      async () => {
        await aprovarUsuario(
          usuario.id,
        )
      },
    )
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

      setUsuarioEditando(null)

      await carregar()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível salvar os acessos.',
      )
    } finally {
      setProcessandoId(null)
    }
  }

  if (carregando) {
    return (
      <div className="admin-state">
        Carregando usuários...
      </div>
    )
  }

  return (
    <>
      <div className="admin-panel panel">
        <div className="admin-toolbar admin-users-toolbar">
          <div>
            <strong>
              Usuários
            </strong>

            <span>
              {usuarios.length}
              {' '}
              cadastrado(s)
              {pendentes > 0
                ? ` · ${pendentes} pendente(s)`
                : ''}
            </span>
          </div>

          <select
            value={filtro}
            onChange={(
              event,
            ) =>
              setFiltro(
                event.target
                  .value as
                  FiltroUsuario,
              )
            }
          >
            <option value="TODOS">
              Todos
            </option>

            <option value="Pendente">
              Pendentes
            </option>

            <option value="Ativo">
              Ativos
            </option>

            <option value="Bloqueado">
              Bloqueados
            </option>

            <option value="Rejeitado">
              Rejeitados
            </option>
          </select>
        </div>

        {erro &&
        !usuarioEditando ? (
          <div className="admin-inline-error">
            {erro}
          </div>
        ) : null}

        <div className="admin-table-wrap">
          <table className="admin-table admin-users-table">
            <thead>
              <tr>
                <th>
                  Nome
                </th>

                <th>
                  E-mail
                </th>

                <th>
                  Perfil
                </th>

                <th>
                  Status
                </th>

                <th>
                  Cadastro
                </th>

                <th>
                  Ações
                </th>
              </tr>
            </thead>

            <tbody>
              {usuariosFiltrados.map(
                (usuario) => {
                  const processando =
                    processandoId ===
                    usuario.id

                  return (
                    <tr
                      key={
                        usuario.id
                      }
                    >
                      <td>
                        <strong>
                          {
                            usuario.nome
                          }
                        </strong>
                      </td>

                      <td>
                        {
                          usuario.email
                        }
                      </td>

                      <td>
                        {
                          usuario.perfil ===
                          'Admin'
                            ? 'Administrador'
                            : 'Usuário'
                        }
                      </td>

                      <td>
                        <span
                          className={`admin-user-status status-${usuario.status.toLowerCase()}`}
                        >
                          {
                            usuario.status
                          }
                        </span>
                      </td>

                      <td>
                        {new Date(
                          usuario
                            .dataCadastro,
                        ).toLocaleDateString(
                          'pt-BR',
                        )}
                      </td>

                      <td>
                        <div className="admin-user-actions">
                          <button
                            type="button"
                            className="admin-action"
                            disabled={
                              processando
                            }
                            onClick={() =>
                              editar(
                                usuario,
                              )
                            }
                          >
                            Acessos
                          </button>

                          {usuario.status ===
                          'Pendente' ? (
                            <>
                              <button
                                type="button"
                                className="admin-action"
                                disabled={
                                  processando
                                }
                                onClick={() =>
                                  void aprovar(
                                    usuario,
                                  )
                                }
                              >
                                Aprovar
                              </button>

                              <button
                                type="button"
                                className="admin-action admin-action-danger"
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
                            </>
                          ) : null}

                          {usuario.status ===
                          'Ativo' ? (
                            <button
                              type="button"
                              className="admin-action admin-action-danger"
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
                          ) : null}

                          {usuario.status ===
                          'Bloqueado' ? (
                            <button
                              type="button"
                              className="admin-action"
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
                          ) : null}
                        </div>
                      </td>
                    </tr>
                  )
                },
              )}

              {usuariosFiltrados.length ===
              0 ? (
                <tr>
                  <td
                    colSpan={6}
                    className="admin-user-empty"
                  >
                    Nenhum usuário
                    encontrado.
                  </td>
                </tr>
              ) : null}
            </tbody>
          </table>
        </div>
      </div>

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
    </>
  )
}