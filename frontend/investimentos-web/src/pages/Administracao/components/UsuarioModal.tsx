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

  function alternarAcessoInvestidor(
    investidorId: string,
    permissao: PermissaoSistema,
  ) {
    setFormulario((atual) => {
      const existente = atual.acessosInvestidores.find(
        (item) => item.investidorId === investidorId,
      )
      const permissoes = existente?.permissoes ?? []
      const novas = permissoes.includes(permissao)
        ? permissoes.filter((item) => item !== permissao)
        : [...permissoes, permissao]
      const demais = atual.acessosInvestidores.filter(
        (item) => item.investidorId !== investidorId,
      )
      const acessosInvestidores = novas.length > 0
        ? [...demais, { investidorId, permissoes: novas }]
        : demais

      return {
        ...atual,
        acessosInvestidores,
        investidoresIds: acessosInvestidores.map(
          (item) => item.investidorId,
        ),
      }
    })
  }

  const semPermissoes =
    formulario.perfil ===
      'Usuario' &&
    formulario.permissoes
      .length === 0

  const semInvestidores =
    formulario.perfil ===
      'Usuario' &&
    formulario.acessosInvestidores
      .length === 0

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
              <span>Chat ID do Telegram</span>
              <input
                type="text"
                inputMode="numeric"
                placeholder="Ex.: 123456789"
                value={formulario.telegramChatId}
                disabled={salvando}
                onChange={(event) => setFormulario((atual) => ({ ...atual, telegramChatId: event.target.value }))}
              />
              <small>O e-mail do usuário já será usado automaticamente. Informe o Chat ID para receber também pelo Telegram.</small>
            </label>

            <label className="user-profile-field">
              <span>Receber relatório IA</span>
              <select
                value={formulario.receberRelatorioIa ? 'SIM' : 'NAO'}
                disabled={salvando}
                onChange={(event) => setFormulario((atual) => ({ ...atual, receberRelatorioIa: event.target.value === 'SIM' }))}
              >
                <option value="NAO">Não receber</option>
                <option value="SIM">Receber por e-mail e Telegram</option>
              </select>
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
          <>
            <section className="user-modal-section">
              <header>
                <div className="user-modal-step">
                  2
                </div>

                <div>
                  <strong>
                    Telas permitidas
                  </strong>

                  <span>
                    Usuários possuem acesso
                    a todas as telas de
                    visualização.
                  </span>
                </div>
              </header>

              <div className="user-permission-grid">
                {permissoesVisualizacao.map(
                  (permissao) => {
                    const selecionada =
                      formulario
                        .permissoes
                        .includes(
                          permissao,
                        )

                    return (
                      <label
                        key={
                          permissao
                        }
                        className={
                          selecionada
                            ? 'user-permission-option selected'
                            : 'user-permission-option'
                        }
                      >
                        <input
                          type="checkbox"
                          checked={
                            selecionada
                          }
                          disabled
                          onChange={() =>
                            alternarPermissao(
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

                          <span>
                            {permissao ===
                            'Dashboard'
                              ? 'Visão geral da carteira'
                              : permissao ===
                                  'Carteira'
                                ? 'Posições e patrimônio'
                                : permissao ===
                                    'Operacoes'
                                  ? 'Compras e vendas'
                                  : permissao ===
                                      'Opcoes'
                                    ? 'Operações com opções'
                                    : permissao ===
                                        'Proventos'
                                      ? 'Dividendos e rendimentos'
                                      : 'Cadastros e configurações'}
                          </span>
                        </div>
                      </label>
                    )
                  },
                )}
              </div>

              {semPermissoes ? (
                <small className="user-validation-message">
                  Selecione pelo menos
                  uma tela.
                </small>
              ) : null}
            </section>

            <section className="user-modal-section">
              <header>
                <div className="user-modal-step">
                  3
                </div>

                <div>
                  <strong>
                    Investidores
                  </strong>

                  <span>
                    Escolha quais
                    carteiras poderão
                    ser consultadas.
                  </span>
                </div>
              </header>

              <div className="user-investor-access-list">
                {investidores.map((investidor) => {
                  const acesso = formulario.acessosInvestidores.find(
                    (item) => item.investidorId === investidor.id,
                  )
                  return (
                    <article className="user-investor-access-card" key={investidor.id}>
                      <header>
                        <strong>{investidor.nome}</strong>
                        <span>
                          {acesso?.permissoes.length ?? 0} tela(s) liberada(s)
                        </span>
                      </header>
                      <div className="user-investor-permissions">
                        {permissoesVisualizacao.map((permissao) => {
                          const selecionada =
                            acesso?.permissoes.includes(permissao) ?? false
                          return (
                            <label
                              key={permissao}
                              className={selecionada ? 'selected' : ''}
                            >
                              <input
                                type="checkbox"
                                checked={selecionada}
                                disabled={salvando}
                                onChange={() =>
                                  alternarAcessoInvestidor(
                                    investidor.id,
                                    permissao,
                                  )
                                }
                              />
                              <span>{nomesPermissoes[permissao]}</span>
                            </label>
                          )
                        })}
                      </div>
                    </article>
                  )
                })}
              </div>

              {semInvestidores ? (
                <small className="user-validation-message">
                  Selecione pelo menos
                  um investidor.
                </small>
              ) : null}
            </section>
          </>
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