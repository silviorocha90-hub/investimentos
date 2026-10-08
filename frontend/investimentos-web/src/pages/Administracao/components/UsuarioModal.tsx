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
  permissoesVisualizacao,
  permissoesInvestidor,
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
  acessosInvestidores: Array<{
    investidorId: string
    permissoes: PermissaoSistema[]
  }>
  telegramChatId: string
  receberRelatorioIa: boolean
  frequenciaRelatorioIa: 'DIARIO' | 'SEMANAL' | 'QUINZENAL' | 'MENSAL'
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
  const pendente =
    usuario.status ===
    'Pendente'

  function selecionarInvestidor(
    investidorId: string,
  ) {
    setFormulario((atual) => {
      const acessoAtual =
        atual.acessosInvestidores.find(
          (item) =>
            item.investidorId ===
            investidorId,
        )

      const permissoes =
        acessoAtual?.permissoes ??
        atual.permissoes

      return {
        ...atual,
        investidoresIds:
          investidorId
            ? [investidorId]
            : [],
        acessosInvestidores:
          investidorId
            ? [{
                investidorId,
                permissoes: [
                  ...permissoes,
                ],
              }]
            : [],
        permissoes: [
          ...permissoes,
        ],
      }
    })
  }

  function alternarAcessoInvestidor(
    investidorId: string,
    permissao: PermissaoSistema,
  ) {
    setFormulario((atual) => {
      const acesso =
        atual.acessosInvestidores.find(
          (item) =>
            item.investidorId ===
            investidorId,
        )

      const permissoesAtuais =
        acesso?.permissoes ??
        atual.permissoes

      const permissoes =
        permissoesAtuais.includes(
          permissao,
        )
          ? permissoesAtuais.filter(
              (item) =>
                item !== permissao,
            )
          : [
              ...permissoesAtuais,
              permissao,
            ]

      return {
        ...atual,
        permissoes,
        investidoresIds: [
          investidorId,
        ],
        acessosInvestidores: [
          {
            investidorId,
            permissoes,
          },
        ],
      }
    })
  }

  const acessoSelecionado =
    formulario
      .acessosInvestidores[0]

  const semPermissoes =
    formulario.perfil ===
      'Usuario' &&
    (acessoSelecionado
      ?.permissoes.length ?? 0) === 0

  const semInvestidores =
    formulario.perfil ===
      'Usuario' &&
    !acessoSelecionado

  const formularioIncompleto =
    semPermissoes ||
    semInvestidores

  return (
    <Modal
      title={usuario.nome}
      subtitle={
        pendente
          ? 'Configurar novo usuário'
          : 'Gerenciar acesso'
      }
      onClose={fechar}
      closeDisabled={salvando}
      className="user-access-modal"
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
            disabled={
              salvando ||
              formularioIncompleto
            }
            onClick={salvar}
          >
            {salvando
              ? pendente
                ? 'Salvando e aprovando...'
                : 'Salvando...'
              : pendente
                ? 'Salvar e aprovar'
                : 'Salvar alterações'}
          </button>
        </>
      }
    >
      <div className="user-modal-content">
        <section className="user-modal-person">
          <div className="user-modal-avatar">
            {usuario.nome
              .trim()
              .charAt(0)
              .toUpperCase()}
          </div>

          <div>
            <strong>
              {usuario.nome}
            </strong>

            <span>
              {usuario.email}
            </span>
          </div>

          <span
            className={`user-status status-${usuario.status.toLowerCase()}`}
          >
            {usuario.status}
          </span>
        </section>

        {pendente ? (
          <div className="user-modal-info">
            <strong>
              Liberação de acesso
            </strong>

            <span>
              Configure as telas e
              investidores antes de
              aprovar este cadastro.
              Ao finalizar, o usuário
              poderá entrar no sistema.
            </span>
          </div>
        ) : null}

        {erro ? (
          <div className="admin-inline-error">
            {erro}
          </div>
        ) : null}

        <section className="user-modal-section">
          <header>
            <div className="user-modal-step">
              1
            </div>

            <div>
              <strong>
                Perfil
              </strong>

              <span>
                Defina o nível de
                acesso do usuário.
              </span>
            </div>
          </header>

          <label className="user-profile-field">
            <span>
              Perfil do usuário
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
                        : [
                            ...permissoesVisualizacao,
                          ],

                    investidoresIds:
                      perfil ===
                      'Admin'
                        ? []
                        : atual.investidoresIds
                            .slice(0, 1),

                    acessosInvestidores:
                      perfil ===
                      'Admin'
                        ? []
                        : atual
                            .acessosInvestidores
                            .slice(0, 1),
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

            {formulario.perfil ===
            'Admin' ? (
              <small>
                Administradores possuem
                acesso global e, após
                definidos como
                administradores, seus
                acessos passam a ser
                protegidos.
              </small>
            ) : null}
          </label>
        </section>

        <section className="user-modal-section">
            <header>
              <div className="user-modal-step">2</div>
              <div>
                <strong>Relatório do Assistente IA</strong>
                <span>{formulario.perfil === 'Admin' ? 'Configure o envio da visão consolidada de todas as carteiras.' : 'Configure o envio das informações das carteiras vinculadas a este usuário.'}</span>
              </div>
            </header>

            <label className="user-profile-field">
              <span>E-mail para relatórios</span>
              <input
                type="email"
                value={usuario.email}
                readOnly
                disabled
              />
              <small>O relatório será enviado automaticamente para o e-mail cadastrado deste usuário.</small>
            </label>

            <label className="user-profile-field">
              <span>Chat ID do Telegram (opcional)</span>
              <input
                type="text"
                inputMode="numeric"
                placeholder="Ex.: 123456789"
                value={formulario.telegramChatId}
                disabled={salvando}
                onChange={(event) => setFormulario((atual) => ({ ...atual, telegramChatId: event.target.value }))}
              />
              <small>Se informado, o mesmo relatório também será enviado pelo Telegram. O número de telefone não é utilizado.</small>
            </label>

            <label className="user-profile-field">
              <span>Receber relatório IA</span>
              <select
                value={formulario.receberRelatorioIa ? 'SIM' : 'NAO'}
                disabled={salvando}
                onChange={(event) => setFormulario((atual) => ({ ...atual, receberRelatorioIa: event.target.value === 'SIM' }))}
              >
                <option value="NAO">Não receber</option>
                <option value="SIM">Receber por e-mail{formulario.telegramChatId.trim() ? ' + Telegram' : ''}</option>
              </select>
              <small>
                {formulario.telegramChatId.trim()
                  ? 'Envio ativo por e-mail e Telegram.'
                  : 'Envio ativo por e-mail. Cadastre o Chat ID para adicionar o Telegram.'}
              </small>
            </label>

            <label className="user-profile-field">
              <span>Frequência do relatório</span>
              <select
                value={formulario.frequenciaRelatorioIa}
                disabled={salvando || !formulario.receberRelatorioIa}
                onChange={(event) => setFormulario((atual) => ({ ...atual, frequenciaRelatorioIa: event.target.value as FormularioUsuario['frequenciaRelatorioIa'] }))}
              >
                <option value="DIARIO">Diário</option>
                <option value="SEMANAL">Semanal</option>
                <option value="QUINZENAL">Quinzenal</option>
                <option value="MENSAL">Mensal</option>
              </select>
            </label>
          </section>

        {formulario.perfil ===
        'Usuario' ? (
          <section className="user-modal-section">
            <header>
              <div className="user-modal-step">
                3
              </div>

              <div>
                <strong>
                  Acesso do investidor
                </strong>

                <span>
                  Vincule este usuário a
                  um único investidor e
                  defina somente o que
                  ele poderá consultar.
                </span>
              </div>
            </header>


            {acessoSelecionado ? (
              <div className="user-permission-grid">
                {permissoesInvestidor.map(
                  (permissao) => {
                    const selecionada =
                      acessoSelecionado
                        .permissoes
                        .includes(
                          permissao,
                        )

                    return (
                      <label
                        key={permissao}
                        className={
                          selecionada
                            ? 'user-permission-option selected'
                            : 'user-permission-option'
                        }
                      >
                        <input
                          type="checkbox"
                          checked={selecionada}
                          disabled={salvando}
                          onChange={() =>
                            alternarAcessoInvestidor(
                              acessoSelecionado
                                .investidorId,
                              permissao,
                            )
                          }
                        />

                        <div>
                          <strong>
                            {
                              nomesPermissoes[
                                permissao
                              ]
                            }
                          </strong>
                        </div>
                      </label>
                    )
                  },
                )}
              </div>
            ) : null}

            {semInvestidores ? (
              <small className="user-validation-message">
                Selecione o investidor
                deste usuário.
              </small>
            ) : semPermissoes ? (
              <small className="user-validation-message">
                Libere pelo menos uma
                tela para o investidor
                selecionado.
              </small>
            ) : null}
          </section>
        ) : (
          <section className="user-admin-access-info">
            <div>
              ✓
            </div>

            <div>
              <strong>
                Acesso administrativo
                completo
              </strong>

              <span>
                Este perfil terá acesso
                a todas as telas e a
                todos os investidores.
              </span>
            </div>
          </section>
        )}
      </div>
    </Modal>
  )
}