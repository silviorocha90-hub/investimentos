import type {
  FormEvent,
} from 'react'

import { Modal } from '../../../components/Modal'

export interface NovaOperacaoForm {
  ticker: string
  tipoOperacaoCodigo: string
  quantidade: string
  precoUnitario: string
  taxas: string
  data: string
}

interface OperacaoModalProps {
  form: NovaOperacaoForm
  salvando: boolean
  erro: string | null
  onChange: (
    form: NovaOperacaoForm,
  ) => void
  onClose: () => void
  onSubmit: (
    event: FormEvent<HTMLFormElement>,
  ) => void
}

export function OperacaoModal({
  form,
  salvando,
  erro,
  onChange,
  onClose,
  onSubmit,
}: OperacaoModalProps) {
  const footer = (
    <>
      <button
        type="button"
        className="operations-modal-cancel"
        disabled={salvando}
        onClick={onClose}
      >
        Cancelar
      </button>

      <button
        type="submit"
        form="nova-operacao-form"
        className="operations-modal-save"
        disabled={salvando}
      >
        {salvando
          ? 'Adicionando...'
          : 'Adicionar Operação'}
      </button>
    </>
  )

  return (
    <Modal
      title="Nova Operação"
      subtitle="Adicionar operação à carteira"
      onClose={onClose}
      closeDisabled={salvando}
      footer={footer}
      className="operations-modal"
    >
      <form
        id="nova-operacao-form"
        className="operations-modal-form"
        onSubmit={onSubmit}
      >
        {erro ? (
          <div className="operations-modal-error">
            {erro}
          </div>
        ) : null}

        <div className="operations-modal-grid">
          <label>
            <span>Ticker *</span>

            <input
              type="text"
              placeholder="Ex: PETR4"
              autoFocus
              value={form.ticker}
              disabled={salvando}
              onChange={(event) =>
                onChange({
                  ...form,
                  ticker:
                    event.target.value,
                })
              }
            />
          </label>

          <label>
            <span>Tipo *</span>

            <select
              value={
                form.tipoOperacaoCodigo
              }
              disabled={salvando}
              onChange={(event) =>
                onChange({
                  ...form,
                  tipoOperacaoCodigo:
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
            <span>Quantidade *</span>

            <input
              type="number"
              placeholder="0"
              min="0.0001"
              step="any"
              value={form.quantidade}
              disabled={salvando}
              onChange={(event) =>
                onChange({
                  ...form,
                  quantidade:
                    event.target.value,
                })
              }
            />
          </label>

          <label>
            <span>
              Preço Unitário *
            </span>

            <input
              type="number"
              placeholder="0,00"
              min="0"
              step="0.01"
              value={
                form.precoUnitario
              }
              disabled={salvando}
              onChange={(event) =>
                onChange({
                  ...form,
                  precoUnitario:
                    event.target.value,
                })
              }
            />
          </label>

          <label>
            <span>Taxas</span>

            <input
              type="number"
              placeholder="0,00"
              min="0"
              step="0.01"
              value={form.taxas}
              disabled={salvando}
              onChange={(event) =>
                onChange({
                  ...form,
                  taxas:
                    event.target.value,
                })
              }
            />
          </label>

          <label>
            <span>Data *</span>

            <input
              type="date"
              value={form.data}
              disabled={salvando}
              onChange={(event) =>
                onChange({
                  ...form,
                  data:
                    event.target.value,
                })
              }
            />
          </label>
        </div>
      </form>
    </Modal>
  )
}