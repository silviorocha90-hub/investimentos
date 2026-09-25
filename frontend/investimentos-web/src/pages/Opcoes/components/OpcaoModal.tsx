import {
  useEffect,
  useState,
} from 'react'

import { Modal } from '../../../components/Modal'
import type {
  OperacaoOpcao,
} from '../../../types/dashboard'

export interface NovaOpcaoForm {
  dataOperacao: string
  ticker: string
  tipoOpcao: string
  natureza: string
  quantidade: string
  strike: string
  vencimento: string
  premioUnitario: string
}

interface OpcaoModalNovoProps {
  modo: 'novo'
  salvando: boolean
  erro: string | null
  onClose: () => void
  onAdicionar: (
    opcao: NovaOpcaoForm,
  ) => Promise<void>
}

interface OpcaoModalEditarProps {
  modo: 'editar'
  opcao: OperacaoOpcao
  salvando: boolean
  erro: string | null
  onClose: () => void
  onSalvar: (
    opcao: OperacaoOpcao,
  ) => Promise<void>
}

type OpcaoModalProps =
  | OpcaoModalNovoProps
  | OpcaoModalEditarProps

function hojeLocal() {
  const agora = new Date()

  const ano =
    agora.getFullYear()

  const mes =
    String(
      agora.getMonth() + 1,
    ).padStart(2, '0')

  const dia =
    String(
      agora.getDate(),
    ).padStart(2, '0')

  return `${ano}-${mes}-${dia}`
}

function formatarMoedaEntrada(
  valor: string,
) {
  const digitos =
    valor.replace(/\D/g, '')

  if (!digitos) {
    return ''
  }

  return (
    Number(digitos) / 100
  ).toLocaleString(
    'pt-BR',
    {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    },
  )
}

function moedaParaNumero(
  valor: string,
) {
  if (!valor) {
    return 0
  }

  return Number(
    valor
      .replace(/\./g, '')
      .replace(',', '.'),
  )
}

function criarNovaOpcao():
  NovaOpcaoForm {
  const hoje = hojeLocal()

  return {
    dataOperacao: hoje,
    ticker: '',
    tipoOpcao: 'PUT',
    natureza: 'VENDA',
    quantidade: '100',
    strike: '',
    vencimento: hoje,
    premioUnitario: '',
  }
}

function NovoOpcaoModal({
  salvando,
  erro,
  onClose,
  onAdicionar,
}: OpcaoModalNovoProps) {
  const [
    form,
    setForm,
  ] = useState<NovaOpcaoForm>(
    criarNovaOpcao,
  )

  const formularioValido =
    form.ticker.trim().length > 0 &&
    Number(form.quantidade) > 0 &&
    moedaParaNumero(form.strike) > 0 &&
    form.vencimento.length > 0 &&
    form.premioUnitario !== '' &&
    moedaParaNumero(
      form.premioUnitario,
    ) >= 0

  return (
    <Modal
      title="Nova Opção"
      subtitle="Registrar operação de opção"
      onClose={onClose}
      closeDisabled={salvando}
      className="options-modal"
      footer={
        <>
          <button
            type="button"
            className="options-clear-button"
            disabled={salvando}
            onClick={onClose}
          >
            Cancelar
          </button>

          <button
            type="button"
            className="options-primary-button"
            disabled={
              salvando ||
              !formularioValido
            }
            onClick={() =>
              onAdicionar(form)
            }
          >
            {salvando
              ? 'Salvando...'
              : 'Salvar opção'}
          </button>
        </>
      }
    >
      <div className="options-modal-body">
        {erro ? (
          <div className="options-modal-error">
            {erro}
          </div>
        ) : null}

        <div className="options-form-grid">
          <label>
            <span>Data</span>

            <input
              type="date"
              value={
                form.dataOperacao
              }
              onChange={(event) =>
                setForm({
                  ...form,
                  dataOperacao:
                    event.target.value,
                })
              }
            />
          </label>

          <label>
            <span>Ticker</span>

            <input
              autoFocus
              value={form.ticker}
              placeholder="Ex.: ITUBU407"
              onChange={(event) =>
                setForm({
                  ...form,
                  ticker:
                    event.target.value
                      .toUpperCase(),
                })
              }
            />
          </label>

          <label>
            <span>Tipo</span>

            <select
              value={
                form.tipoOpcao
              }
              onChange={(event) =>
                setForm({
                  ...form,
                  tipoOpcao:
                    event.target.value,
                })
              }
            >
              <option value="PUT">
                PUT
              </option>

              <option value="CALL">
                CALL
              </option>
            </select>
          </label>

          <label>
            <span>Natureza</span>

            <select
              value={
                form.natureza
              }
              onChange={(event) =>
                setForm({
                  ...form,
                  natureza:
                    event.target.value,
                })
              }
            >
              <option value="COMPRA">
                COMPRA
              </option>

              <option value="VENDA">
                VENDA
              </option>
            </select>
          </label>

          <label>
            <span>Quantidade</span>

            <input
              type="number"
              min="1"
              step="1"
              value={
                form.quantidade
              }
              onChange={(event) =>
                setForm({
                  ...form,
                  quantidade:
                    event.target.value,
                })
              }
            />
          </label>

          <label>
            <span>Strike</span>

            <div className="admin-money-input">
              <span>R$</span>
              <input
                inputMode="numeric"
                value={form.strike}
                onChange={(event) =>
                  setForm({
                    ...form,
                    strike:
                      formatarMoedaEntrada(
                        event.target.value,
                      ),
                  })
                }
              />
            </div>
          </label>

          <label>
            <span>Vencimento</span>

            <input
              type="date"
              value={
                form.vencimento
              }
              onChange={(event) =>
                setForm({
                  ...form,
                  vencimento:
                    event.target.value,
                })
              }
            />
          </label>

          <label>
            <span>Prêmio Unit.</span>

            <div className="admin-money-input">
              <span>R$</span>
              <input
                inputMode="numeric"
                value={
                  form.premioUnitario
                }
                onChange={(event) =>
                  setForm({
                    ...form,
                    premioUnitario:
                      formatarMoedaEntrada(
                        event.target.value,
                      ),
                  })
                }
              />
            </div>
          </label>

          <label className="options-status-field">
            <span>Status</span>

            <input
              value="EXECUTADA"
              disabled
            />
          </label>
        </div>
      </div>
    </Modal>
  )
}

function EditarOpcaoModal({
  opcao,
  salvando,
  erro,
  onClose,
  onSalvar,
}: OpcaoModalEditarProps) {
  const [
    form,
    setForm,
  ] =
    useState<OperacaoOpcao>({
      ...opcao,
    })

  useEffect(() => {
    setForm({
      ...opcao,
    })
  }, [opcao])

  const executada =
    opcao.situacao === 'EXECUTADA'

  const encerrada =
    opcao.situacao === 'ENCERRADA'

  const formularioValido =
    form.dataFinalizacao != null &&
    form.dataFinalizacao !== '' &&
    form.precoRecompraUnitario != null

  return (
    <Modal
      title={opcao.tickerOpcao}
      subtitle="Editar opção"
      onClose={onClose}
      closeDisabled={salvando}
      className="options-modal"
      footer={
        <>
          <button
            type="button"
            className="options-clear-button"
            disabled={salvando}
            onClick={onClose}
          >
            Cancelar
          </button>

          <button
            type="button"
            className="options-primary-button"
            disabled={
              salvando ||
              !formularioValido
            }
            onClick={() =>
              onSalvar(form)
            }
          >
            {salvando
              ? 'Salvando...'
              : 'Salvar alterações'}
          </button>
        </>
      }
    >
      <div className="options-modal-body">
        {erro ? (
          <div className="options-modal-error">
            {erro}
          </div>
        ) : null}

        <div className="options-form-grid">
          <label>
            <span>Data inicial</span>

            <input
              type="date"
              value={
                form.dataOperacao.slice(
                  0,
                  10,
                )
              }
              disabled
            />
          </label>

          <label>
            <span>Ticker</span>

            <input
              value={
                form.tickerOpcao
              }
              disabled
            />
          </label>

          <label>
            <span>Tipo</span>

            <input
              value={
                form.tipoOpcao
              }
              disabled
            />
          </label>

          <label>
            <span>Natureza</span>

            <input
              value={
                form.natureza
              }
              disabled
            />
          </label>

          <label>
            <span>Quantidade</span>

            <input
              type="number"
              min="1"
              step="1"
              value={
                form.quantidade
              }
              disabled
            />
          </label>

          <label>
            <span>Strike</span>

            <div className="admin-money-input">
              <span>R$</span>
              <input
                inputMode="numeric"
                value={
                  form.strike.toLocaleString(
                    'pt-BR',
                    {
                      minimumFractionDigits: 2,
                      maximumFractionDigits: 2,
                    },
                  )
                }
                disabled
              />
            </div>
          </label>

          <label>
            <span>Vencimento</span>

            <input
              type="date"
              value={
                form.vencimento.slice(
                  0,
                  10,
                )
              }
              disabled
            />
          </label>

          <label>
            <span>Prêmio Unit.</span>

            <div className="admin-money-input">
              <span>R$</span>
              <input
                inputMode="numeric"
                value={
                  form.premioUnitario.toLocaleString(
                    'pt-BR',
                    {
                      minimumFractionDigits: 2,
                      maximumFractionDigits: 2,
                    }
                disabled
              />
            </div>
          </label>

          <label>
            <span>Status</span>

            {executada ? (
              <select
                value={form.situacao}
                onChange={() =>
                  setForm({
                    ...form,
                    situacao: 'ENCERRADA',
                  })
                }
              >
                <option value="EXECUTADA" disabled>
                  EXECUTADA
                </option>
                <option value="ENCERRADA">
                  ENCERRADA
                </option>
              </select>
            ) : (
              <input
                value={
                  encerrada
                    ? 'ENCERRADA'
                    : form.situacao
                }
                disabled
              />
            )}
          </label>

          <label>
            <span>Data finalização</span>

            <input
              type="date"
              value={
                form.dataFinalizacao?.slice(
                  0,
                  10,
                ) ?? ''
              }
              onChange={(event) =>
                setForm({
                  ...form,
                  dataFinalizacao:
                    event.target.value ||
                    null,
                })
              }
            />
          </label>

          <label>
            <span>Preço recompra unit.</span>

            <div className="admin-money-input">
              <span>R$</span>
              <input
                inputMode="numeric"
                value={
                  form.precoRecompraUnitario == null
                    ? ''
                    : form.precoRecompraUnitario.toLocaleString(
                        'pt-BR',
                        {
                          minimumFractionDigits: 2,
                          maximumFractionDigits: 2,
                        },
                      )
                }
                onChange={(event) => {
                  const valor =
                    formatarMoedaEntrada(
                      event.target.value,
                    )

                  setForm({
                    ...form,
                    precoRecompraUnitario:
                      valor === ''
                        ? null
                        : moedaParaNumero(
                            valor,
                          ),
                  })
                }}
                placeholder="Opcional"
              />
            </div>
          </label>



          <label>
            <span>Resultado informado</span>

            <div className="admin-money-input">
              <span>R$</span>
              <input
                inputMode="numeric"
                value={
                  form.resultadoInformado == null
                    ? ''
                    : form.resultadoInformado.toLocaleString(
                        'pt-BR',
                        {
                          minimumFractionDigits: 2,
                          maximumFractionDigits: 2,
                        },
                      )
                }
                onChange={(event) => {
                  const valor =
                    formatarMoedaEntrada(
                      event.target.value,
                    )

                  setForm({
                    ...form,
                    resultadoInformado:
                      valor === ''
                        ? null
                        : moedaParaNumero(
                            valor,
                          ),
                  })
                }}
                placeholder="Opcional"
              />
            </div>
          </label>
        </div>
      </div>
    </Modal>
  )
}

export function OpcaoModal(
  props: OpcaoModalProps,
) {
  if (props.modo === 'novo') {
    return (
      <NovoOpcaoModal
        {...props}
      />
    )
  }

  return (
    <EditarOpcaoModal
      {...props}
    />
  )
}