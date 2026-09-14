import type {
  Dispatch,
  SetStateAction,
} from 'react'

import {
  Modal,
} from '../../../components/Modal'

import type {
  InvestidorAdministracao,
} from '../../../types/administracao'

import {
  nomesPermissoes,
  permissoesSistema,
} from '../../../types/usuario'

import type {
  PerfilUsuario,
  PermissaoSistema,
  UsuarioAdministracao,
} from '../../../types/usuario'

export interface FormularioUsuario {
  perfil: PerfilUsuario
  permissoes:
    PermissaoSistema[]
  investidoresIds: string[]
}

interface UsuarioModalProps {
  usuario:
    UsuarioAdministracao

  investidores:
    InvestidorAdministracao[]

  formulario:
    FormularioUsuario

  setFormulario:
    Dispatch<
      SetStateAction<
        FormularioUsuario
      >
    >

  salvando: boolean

  erro: string | null

  fechar: () => void

  salvar: () => void
}

export function UsuarioModal({
  usuario,
  investidores,
  formulario,
  setFormulario,
  salvando,
  erro,
  fechar,
  salvar,
}: UsuarioModalProps) {
  const admin =
    formulario.perfil ===
    'Admin'

  function alternarPermissao(
    permissao:
      PermissaoSistema,
  ) {
    setFormulario(
      (atual) => ({
        ...atual,

        permissoes:
          atual.permissoes
            .includes(permissao)
            ? atual.permissoes
                .filter(
                  (item) =>
                    item !==
                    permissao,
                )
            : [
                ...atual
                  .permissoes,
                permissao,
              ],
      }),
    )
  }

  function alternarInvestidor(
    investidorId: string,
  ) {
    setFormulario(
      (atual) => ({
        ...atual,

        investidoresIds:
          atual.investidoresIds
            .includes(
              investidorId,
            )
            ? atual
                .investidoresIds
                .filter(
                  (item) =>
                    item !==
                    investidorId,
                )
            : [
                ...atual
                  .investidoresIds,
                investidorId,
              ],
      }),
    )
  }

  return (
    <Modal
      title={usuario.nome}
      subtitle="Acessos do usuário"
      onClose={fechar}
      closeDisabled={salvando}
      className="admin-user-modal"
      footer={
        <>
          <button
            type="button"
            className="admin-clear-button"
            disabled={salvando}
            onClick={fechar}
          >
            Cancelar
          </button>

          <button
            type="button"
            className="admin-primary-button"
            disabled={salvando}
            onClick={salvar}
          >
            {salvando
              ? 'Salvando...'
              : 'Salvar acessos'}
          </button>
        </>
      }
    >
      <div className="admin-user-modal-content">
        <div className="admin-user-identification">
          <span>
            E-mail
          </span>

          <strong>
            {usuario.email}
          </strong>

          <small>
            Status: {usuario.status}
          </small>
        </div>

        {erro ? (
          <div className="admin-inline-error">
            {erro}
          </div>
        ) : null}

        <label className="admin-user-profile">
          <span>
            Perfil
          </span>

          <select
            value={
              formulario.perfil
            }
            disabled={salvando}
            onChange={(
              event,
            ) => {
              const perfil =
                event.target
                  .value as
                  PerfilUsuario

              setFormulario(
                (atual) => ({
                  ...atual,
                  perfil,

                  permissoes:
                    perfil ===
                    'Admin'
                      ? [
                          ...permissoesSistema,
                        ]
                      : atual
                          .permissoes,

                  investidoresIds:
                    perfil ===
                    'Admin'
                      ? []
                      : atual
                          .investidoresIds,
                }),
              )
            }}
          >
            <option value="Usuario">
              Usuário
            </option>

            <option value="Admin">
              Administrador
            </option>
          </select>
        </label>

        <section className="admin-user-access-section">
          <div>
            <strong>
              Telas permitidas
            </strong>

            <span>
              Defina quais áreas
              poderão ser acessadas.
            </span>
          </div>

          <div className="admin-user-check-grid">
            {permissoesSistema.map(
              (permissao) => (
                <label
                  key={
                    permissao
                  }
                  className="admin-user-check"
                >
                  <input
                    type="checkbox"
                    checked={
                      admin ||
                      formulario
                        .permissoes
                        .includes(
                          permissao,
                        )
                    }
                    disabled={
                      salvando ||
                      admin
                    }
                    onChange={() =>
                      alternarPermissao(
                        permissao,
                      )
                    }
                  />

                  <span>
                    {
                      nomesPermissoes[
                        permissao
                      ]
                    }
                  </span>
                </label>
              ),
            )}
          </div>

          {admin ? (
            <small className="admin-user-note">
              Administradores possuem
              acesso a todas as telas.
            </small>
          ) : null}
        </section>

        <section className="admin-user-access-section">
          <div>
            <strong>
              Investidores
            </strong>

            <span>
              Defina quais carteiras
              este usuário poderá
              consultar.
            </span>
          </div>

          {admin ? (
            <div className="admin-user-note">
              Administradores possuem
              acesso global aos
              investidores.
            </div>
          ) : (
            <div className="admin-user-check-grid">
              {investidores.map(
                (investidor) => (
                  <label
                    key={
                      investidor.id
                    }
                    className="admin-user-check"
                  >
                    <input
                      type="checkbox"
                      checked={
                        formulario
                          .investidoresIds
                          .includes(
                            investidor.id,
                          )
                      }
                      disabled={
                        salvando
                      }
                      onChange={() =>
                        alternarInvestidor(
                          investidor.id,
                        )
                      }
                    />

                    <span>
                      {
                        investidor.nome
                      }
                    </span>
                  </label>
                ),
              )}
            </div>
          )}
        </section>
      </div>
    </Modal>
  )
}