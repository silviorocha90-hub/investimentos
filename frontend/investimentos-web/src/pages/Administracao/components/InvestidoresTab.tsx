import {
  useEffect,
  useState,
} from 'react'
import type {
  Dispatch,
  FormEvent,
  SetStateAction,
} from 'react'
import {
  atualizarHistoricoPatrimonio,
  atualizarInvestidor,
  criarHistoricoPatrimonio,
  excluirHistoricoPatrimonio,
} from '../../../api/administracaoApi'
import {
  Modal,
} from '../../../components/Modal'
import type {
  Administracao,
  HistoricoPatrimonioAdministracao,
} from '../../../types/administracao'
import {
  formatarDataCurta,
  formatarMoeda,
} from '../../../utils/formatters'

type AbaInvestidor =
  | 'resumo'
  | 'historico'

type ModoHistorico =
  | 'novo'
  | 'editar'

interface InvestidoresTabProps {
  dados: Administracao

  investidorSelecionadoId:
    string | null

  setInvestidorSelecionadoId:
    Dispatch<
      SetStateAction<
        string | null
      >
    >

  recarregar:
    () => Promise<void>
}

interface FormularioHistorico {
  dataReferencia: string
  valorCarteira: string
}

interface ModalHistoricoState {
  modo: ModoHistorico

  historico?:
    HistoricoPatrimonioAdministracao
}

export function InvestidoresTab({
  dados,
  investidorSelecionadoId,
  setInvestidorSelecionadoId,
  recarregar,
}: InvestidoresTabProps) {
  const [
    abaInvestidor,
    setAbaInvestidor,
  ] = useState<AbaInvestidor>(
    'resumo',
  )

  const [
    saldoDisponivel,
    setSaldoDisponivel,
  ] = useState('')

  const [
    salvandoDados,
    setSalvandoDados,
  ] = useState(false)

  const [
    modalHistorico,
    setModalHistorico,
  ] =
    useState<ModalHistoricoState | null>(
      null,
    )

  const [
    formularioHistorico,
    setFormularioHistorico,
  ] =
    useState<FormularioHistorico>({
      dataReferencia: '',
      valorCarteira: '',
    })

  const [
    salvandoHistorico,
    setSalvandoHistorico,
  ] = useState(false)

  const [
    erro,
    setErro,
  ] =
    useState<string | null>(
      null,
    )

  const investidorSelecionado =
    dados.investidores.find(
      (investidor) =>
        investidor.id ===
        investidorSelecionadoId,
    )

  useEffect(() => {
    if (
      !investidorSelecionado
    ) {
      setSaldoDisponivel('')
      return
    }

    setSaldoDisponivel(
      String(
        investidorSelecionado
          .saldoDisponivel ??
          0,
      ),
    )

    setErro(null)
  }, [
    investidorSelecionado?.id,
    investidorSelecionado
      ?.saldoDisponivel,
  ])

  async function salvarDados(
    event:
      FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    if (
      !investidorSelecionado
    ) {
      return
    }

    const saldo =
      Number(
        saldoDisponivel
          .replace(',', '.'),
      )

    if (
      Number.isNaN(saldo)
    ) {
      setErro(
        'Informe um saldo válido.',
      )

      return
    }

    try {
      setSalvandoDados(true)
      setErro(null)

      await atualizarInvestidor(
        investidorSelecionado.id,
        {
          nome:
            investidorSelecionado.nome,

          saldoDisponivel:
            saldo,
        },
      )

      await recarregar()

      setAbaInvestidor(
        'resumo',
      )
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível atualizar o saldo.',
      )
    } finally {
      setSalvandoDados(false)
    }
  }

  function abrirNovoHistorico() {
    setFormularioHistorico({
      dataReferencia:
        new Date()
          .toISOString()
          .slice(0, 10),

      valorCarteira: '',
    })

    setModalHistorico({
      modo: 'novo',
    })

    setErro(null)
  }

  function abrirEditarHistorico(
    historico:
      HistoricoPatrimonioAdministracao,
  ) {
    setFormularioHistorico({
      dataReferencia:
        historico
          .dataReferencia
          .slice(0, 10),

      valorCarteira:
        String(
          historico
            .valorCarteira,
        ),
    })

    setModalHistorico({
      modo: 'editar',
      historico,
    })

    setErro(null)
  }

  async function salvarHistorico(
    event:
      FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    if (
      !investidorSelecionado ||
      !modalHistorico
    ) {
      return
    }

    if (
      !formularioHistorico
        .dataReferencia
    ) {
      setErro(
        'Informe a data do patrimônio.',
      )

      return
    }

    const valor =
      Number(
        formularioHistorico
          .valorCarteira
          .replace(',', '.'),
      )

    if (
      Number.isNaN(valor) ||
      valor < 0
    ) {
      setErro(
        'Informe um patrimônio válido.',
      )

      return
    }

    try {
      setSalvandoHistorico(
        true,
      )

      setErro(null)

      const request = {
        dataReferencia:
          formularioHistorico
            .dataReferencia,

        valorCarteira:
          valor,
      }

      if (
        modalHistorico.modo ===
        'novo'
      ) {
        await criarHistoricoPatrimonio(
          investidorSelecionado.id,
          request,
        )
      } else {
        await atualizarHistoricoPatrimonio(
          modalHistorico
            .historico!.id,
          request,
        )
      }

      setModalHistorico(null)

      await recarregar()

      setAbaInvestidor(
        'historico',
      )
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível salvar o patrimônio.',
      )
    } finally {
      setSalvandoHistorico(
        false,
      )
    }
  }

  async function excluirHistorico(
    historico:
      HistoricoPatrimonioAdministracao,
  ) {
    const confirmado =
      window.confirm(
        `Deseja excluir o patrimônio de ${formatarDataCurta(
          historico.dataReferencia,
        )}?`,
      )

    if (!confirmado) {
      return
    }

    try {
      setErro(null)

      await excluirHistoricoPatrimonio(
        historico.id,
      )

      await recarregar()

      setAbaInvestidor(
        'historico',
      )
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível excluir o patrimônio.',
      )
    }
  }

  return (
    <>
      <article className="panel admin-panel">
        <div className="admin-investor-grid">
          {dados.investidores.map(
            (investidor) => (
              <button
                key={
                  investidor.id
                }
                type="button"
                className={`admin-investor-card ${
                  investidor.id ===
                  investidorSelecionadoId
                    ? 'active'
                    : ''
                }`}
                onClick={() => {
                  setInvestidorSelecionadoId(
                    investidor.id,
                  )

                  setErro(null)
                }}
              >
                <span>
                  {
                    investidor.nome
                  }
                </span>

                <strong>
                  {formatarMoeda(
                    investidor
                      .saldoDisponivel,
                  )}
                </strong>

                <small>
                  Saldo disponível
                </small>
              </button>
            ),
          )}
        </div>
      </article>

      {investidorSelecionado ? (
        <article className="panel admin-panel">
          <div className="admin-subtabs">
            <button
              type="button"
              className={
                abaInvestidor ===
                'resumo'
                  ? 'active'
                  : ''
              }
              onClick={() =>
                setAbaInvestidor(
                  'resumo',
                )
              }
            >
              Dados e Saldo
            </button>

            <button
              type="button"
              className={
                abaInvestidor ===
                'historico'
                  ? 'active'
                  : ''
              }
              onClick={() =>
                setAbaInvestidor(
                  'historico',
                )
              }
            >
              Histórico Patrimonial
            </button>
          </div>

          {erro ? (
            <div className="admin-state admin-error">
              {erro}
            </div>
          ) : null}

          {abaInvestidor ===
          'resumo' ? (
            <form
              className="admin-modal-grid"
              onSubmit={
                salvarDados
              }
            >
              <label>
                <span>
                  Investidor
                </span>

                <input
                  type="text"
                  value={
                    investidorSelecionado.nome
                  }
                  readOnly
                />
              </label>

              <label>
                <span>
                  Saldo disponível
                </span>

                <input
                  type="number"
                  step="0.01"
                  value={
                    saldoDisponivel
                  }
                  onChange={(
                    event,
                  ) =>
                    setSaldoDisponivel(
                      event
                        .target
                        .value,
                    )
                  }
                />
              </label>

              <div className="admin-modal-actions">
                <button
                  type="submit"
                  className="admin-primary-button"
                  disabled={
                    salvandoDados
                  }
                >
                  {salvandoDados
                    ? 'Salvando...'
                    : 'Salvar Alterações'}
                </button>
              </div>
            </form>
          ) : null}

          {abaInvestidor ===
          'historico' ? (
            <>
              <div className="admin-toolbar">
                <strong>
                  Histórico Patrimonial
                </strong>

                <button
                  type="button"
                  className="admin-primary-button"
                  onClick={
                    abrirNovoHistorico
                  }
                >
                  + Novo Registro
                </button>
              </div>

              <div className="admin-table-wrap">
                <table className="data-table admin-table">
                  <thead>
                    <tr>
                      <th>
                        Data
                      </th>

                      <th>
                        Patrimônio
                      </th>

                      <th>
                        Ações
                      </th>
                    </tr>
                  </thead>

                  <tbody>
                    {investidorSelecionado
                      .historicoPatrimonio
                      .slice()
                      .sort(
                        (
                          a,
                          b,
                        ) =>
                          b.dataReferencia.localeCompare(
                            a.dataReferencia,
                          ),
                      )
                      .map(
                        (
                          historico,
                        ) => (
                          <tr
                            key={
                              historico.id
                            }
                          >
                            <td>
                              {formatarDataCurta(
                                historico
                                  .dataReferencia,
                              )}
                            </td>

                            <td>
                              {formatarMoeda(
                                historico
                                  .valorCarteira,
                              )}
                            </td>

                            <td>
                              <button
                                type="button"
                                className="admin-action"
                                onClick={() =>
                                  abrirEditarHistorico(
                                    historico,
                                  )
                                }
                              >
                                Editar
                              </button>

                              {' '}

                              <button
                                type="button"
                                className="admin-action"
                                onClick={() =>
                                  excluirHistorico(
                                    historico,
                                  )
                                }
                              >
                                Excluir
                              </button>
                            </td>
                          </tr>
                        ),
                      )}

                    {investidorSelecionado
                      .historicoPatrimonio
                      .length ===
                    0 ? (
                      <tr>
                        <td
                          colSpan={3}
                        >
                          Nenhum patrimônio registrado.
                        </td>
                      </tr>
                    ) : null}
                  </tbody>
                </table>
              </div>
            </>
          ) : null}
        </article>
      ) : null}

      {modalHistorico ? (
        <Modal
          title={
            modalHistorico.modo ===
            'novo'
              ? 'Novo Registro'
              : 'Editar Registro'
          }
          subtitle="Histórico Patrimonial"
          onClose={() =>
            setModalHistorico(
              null,
            )
          }
          closeDisabled={
            salvandoHistorico
          }
        >
          <form
            onSubmit={
              salvarHistorico
            }
          >
            <div className="admin-modal-grid">
              <label>
                <span>
                  Data
                </span>

                <input
                  type="date"
                  value={
                    formularioHistorico
                      .dataReferencia
                  }
                  onChange={(
                    event,
                  ) =>
                    setFormularioHistorico(
                      {
                        ...formularioHistorico,
                        dataReferencia:
                          event
                            .target
                            .value,
                      },
                    )
                  }
                />
              </label>

              <label>
                <span>
                  Patrimônio
                </span>

                <input
                  type="number"
                  step="0.01"
                  min="0"
                  value={
                    formularioHistorico
                      .valorCarteira
                  }
                  onChange={(
                    event,
                  ) =>
                    setFormularioHistorico(
                      {
                        ...formularioHistorico,
                        valorCarteira:
                          event
                            .target
                            .value,
                      },
                    )
                  }
                />
              </label>
            </div>

            <div className="admin-modal-actions">
              <button
                type="button"
                className="admin-clear-button"
                disabled={
                  salvandoHistorico
                }
                onClick={() =>
                  setModalHistorico(
                    null,
                  )
                }
              >
                Cancelar
              </button>

              <button
                type="submit"
                className="admin-primary-button"
                disabled={
                  salvandoHistorico
                }
              >
                {salvandoHistorico
                  ? 'Salvando...'
                  : 'Salvar'}
              </button>
            </div>
          </form>
        </Modal>
      ) : null}
    </>
  )
}