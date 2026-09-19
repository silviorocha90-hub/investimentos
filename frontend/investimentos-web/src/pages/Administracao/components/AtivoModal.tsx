import type {
  Dispatch,
  FormEvent,
  SetStateAction,
} from 'react'
import type {
  TipoAtivoAdministracao,
} from '../../../types/administracao'
import type {
  FormularioAtivo,
  ModalAtivoState,
} from '../AdministracaoView'

interface AtivoModalProps {
  modalAtivo:
    ModalAtivoState

  formularioAtivo:
    FormularioAtivo

  setFormularioAtivo:
    Dispatch<
      SetStateAction<
        FormularioAtivo
      >
    >

  tipos:
    TipoAtivoAdministracao[]

  salvando:
    boolean

  erro:
    string | null

  fechar:
    () => void

  salvar:
    (
      event:
        FormEvent<HTMLFormElement>,
    ) => Promise<void>
}

export function AtivoModal({
  modalAtivo,
  formularioAtivo,
  setFormularioAtivo,
  tipos,
  salvando,
  erro,
  fechar,
  salvar,
}: AtivoModalProps) {
  return (
    <div className="admin-modal-backdrop">
      <form
        className="admin-modal"
        onSubmit={salvar}
      >
        <div className="admin-modal-header">
          <div>
            <span>
              {modalAtivo.modo ===
              'novo'
                ? 'Cadastro'
                : 'Manutenção'}
            </span>

            <strong>
              {modalAtivo.modo ===
              'novo'
                ? 'Novo Ativo'
                : formularioAtivo
                    .ticker}
            </strong>
          </div>

          <button
            type="button"
            onClick={fechar}
          >
            ×
          </button>
        </div>

        <div className="admin-modal-grid">
          <label>
            <span>
              Ticker
            </span>

            <input
              value={
                formularioAtivo
                  .ticker
              }
              disabled={
                modalAtivo.modo ===
                'editar'
              }
              onChange={(
                event,
              ) =>
                setFormularioAtivo(
                  {
                    ...formularioAtivo,

                    ticker:
                      event
                        .target
                        .value
                        .toUpperCase(),
                  },
                )
              }
              placeholder="ITUB4"
            />
          </label>

          <label>
            <span>
              Nome
            </span>

            <input
              value={
                formularioAtivo
                  .nome
              }
              onChange={(
                event,
              ) =>
                setFormularioAtivo(
                  {
                    ...formularioAtivo,

                    nome:
                      event
                        .target
                        .value,
                  },
                )
              }
              placeholder="Opcional"
            />
          </label>

          <label>
            <span>
              Tipo
            </span>

            <select
              value={
                formularioAtivo
                  .tipoAtivoId
              }
              onChange={(
                event,
              ) =>
                setFormularioAtivo(
                  {
                    ...formularioAtivo,

                    tipoAtivoId:
                      event
                        .target
                        .value,
                  },
                )
              }
            >
             {tipos.map(
  (tipo) => (
    <option
      key={
        tipo.id
      }
      value={
        tipo.id
      }
    >
      {tipo.nome}
    </option>
  ),
)}
            </select>
          </label>

          <label>
            <span>
              Cotação
            </span>

            <input
              type="number"
              step="0.01"
              min="0"
              value={
                formularioAtivo
                  .cotacao
              }
              onChange={(
                event,
              ) =>
                setFormularioAtivo(
                  {
                    ...formularioAtivo,

                    cotacao:
                      event
                        .target
                        .value,
                  },
                )
              }
              placeholder="0,00"
            />
          </label>

          <label>
            <span>
              Data Cotação
            </span>

            <input
              type="date"
              value={
                formularioAtivo
                  .dataCotacao
              }
              onChange={(
                event,
              ) =>
                setFormularioAtivo(
                  {
                    ...formularioAtivo,

                    dataCotacao:
                      event
                        .target
                        .value,
                  },
                )
              }
            />
          </label>
        </div>

        {erro ? (
          <div className="admin-inline-error">
            {erro}
          </div>
        ) : null}

        <div className="admin-modal-actions">
          <button
            type="button"
            className="admin-clear-button"
            onClick={fechar}
            disabled={salvando}
          >
            Cancelar
          </button>

          <button
            type="submit"
            className="admin-primary-button"
            disabled={salvando}
          >
            {salvando
              ? 'Salvando...'
              : modalAtivo.modo ===
                  'novo'
                ? 'Cadastrar Ativo'
                : 'Salvar Alterações'}
          </button>
        </div>
      </form>
    </div>
  )
}