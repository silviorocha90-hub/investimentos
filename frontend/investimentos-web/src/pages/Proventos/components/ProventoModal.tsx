import {
  useEffect,
  useMemo,
  useState,
} from 'react'
import { Modal } from '../../../components/Modal'
import type {
  Provento,
} from '../../../types/dashboard'
import {
  formatarMoeda,
} from '../../../utils/formatters'
import type {
  SalvarProventoRequest,
  SalvarProventoTotalRequest,
} from '../../../api/proventosApi'

interface ProventoModalProps {
  provento?: Provento | null
  investidorId: string
  investidores: readonly { id: string; nome: string }[]
  modoRateio: boolean
  tickers: readonly string[]
  salvando: boolean
  erro?: string | null
  onClose: () => void
  onSalvar: (
    request: SalvarProventoRequest | SalvarProventoTotalRequest,
  ) => Promise<void>
}

function paraDataInput(
  valor?: string | null,
) {
  if (!valor) {
    return ''
  }

  return valor.slice(
    0,
    10,
  )
}

export function ProventoModal({
  provento,
  investidorId,
  investidores,
  modoRateio,
  tickers,
  salvando,
  erro,
  onClose,
  onSalvar,
}: ProventoModalProps) {
  const [
    investidorSelecionadoId,
    setInvestidorSelecionadoId,
  ] = useState(investidorId)

  const [
    ticker,
    setTicker,
  ] = useState('')

  const [
    tipo,
    setTipo,
  ] = useState('')

  const [
    descricao,
    setDescricao,
  ] = useState('')

  const [
    dataCom,
    setDataCom,
  ] = useState('')

  const [
    dataPagamento,
    setDataPagamento,
  ] = useState('')

  const [
    quantidadeBase,
    setQuantidadeBase,
  ] = useState('')

  const [
    valorTotal,
    setValorTotal,
  ] = useState('')

  const [
    valorPorUnidade,
    setValorPorUnidade,
  ] = useState('')

  const [
    valorRecebido,
    setValorRecebido,
  ] = useState('')

  useEffect(() => {
    if (provento) {
      setTicker(
        provento.ticker,
      )

      setTipo(
        provento.tipo,
      )

      setDescricao(
        provento.descricao ?? '',
      )

      setDataCom(
        paraDataInput(
          provento.dataCom,
        ),
      )

      setDataPagamento(
        paraDataInput(
          provento.dataPagamento,
        ),
      )

      setQuantidadeBase(
        String(
          provento.quantidadeBase,
        ),
      )

      setValorPorUnidade(
        String(
          provento.valorPorUnidade,
        ),
      )

      setValorRecebido(
        String(
          provento.valorRecebido,
        ),
      )

      return
    }

    setInvestidorSelecionadoId('')

    setTicker('')

    setTipo('')

    setDescricao('')
    setDataCom('')

    setDataPagamento(
      new Date()
        .toISOString()
        .slice(0, 10),
    )

    setQuantidadeBase('')
    setValorPorUnidade('')
    setValorRecebido('')
    setValorTotal('')
  }, [
    provento,
    tickers,
    investidorId,
    investidores,
  ])

  const quantidade =
    Number(
      quantidadeBase.replace(
        ',',
        '.',
      ),
    ) || 0

  const valorUnitario =
    Number(
      valorPorUnidade.replace(
        ',',
        '.',
      ),
    ) || 0

  const recebido =
    Number(
      valorRecebido.replace(
        ',',
        '.',
      ),
    ) || 0

  const valorBruto =
    quantidade *
    valorUnitario

  const imposto =
    Math.max(
      0,
      valorBruto -
        recebido,
    )

  const total =
    Number(
      valorTotal.replace(
        ',',
        '.',
      ),
    ) || 0

  const formularioValido =
    useMemo(
      () =>
        ticker.trim()
          .length > 0 &&
        tipo.length > 0 &&
        dataPagamento
          .length > 0 &&
        (
          modoRateio && !provento
            ? total > 0
            : investidorSelecionadoId.length > 0 &&
              quantidade > 0 &&
              valorUnitario >= 0 &&
              recebido >= 0
        ),
      [
        ticker,
        tipo,
        dataPagamento,
        quantidade,
        valorUnitario,
        recebido,
        total,
        provento,
        modoRateio,
        investidorSelecionadoId,
      ],
    )

  async function salvar() {
    if (
      !formularioValido
    ) {
      return
    }

    if (!provento && modoRateio) {
      await onSalvar({
        ticker:
          ticker.trim().toUpperCase(),
        tipo,
        descricao:
          descricao.trim() || null,
        dataCom:
          dataCom || null,
        dataPagamento,
        valorTotal: total,
      })
      return
    }

    await onSalvar({
      investidorId:
        investidorSelecionadoId,

      ticker:
        ticker
          .trim()
          .toUpperCase(),

      tipo,

      descricao:
        descricao.trim() ||
        null,

      dataCom:
        dataCom ||
        null,

      dataPagamento,

      quantidadeBase:
        quantidade,

      valorPorUnidade:
        valorUnitario,

      valorRecebido:
        recebido,
    })
  }

  return (
    <Modal
      title={
        provento
          ? 'Editar provento'
          : 'Novo provento'
      }
      subtitle={
        provento
          ? `${provento.ticker} · ${provento.tipo}`
          : 'Registrar rendimento recebido'
      }
      onClose={
        onClose
      }
      closeDisabled={
        salvando
      }
      className="provento-modal"
      footer={
        <>
          <button
            type="button"
            className="admin-btn secondary"
            disabled={
              salvando
            }
            onClick={
              onClose
            }
          >
            Cancelar
          </button>

          <button
            type="button"
            className="admin-btn primary"
            disabled={
              salvando ||
              !formularioValido
            }
            onClick={
              salvar
            }
          >
            {salvando
              ? 'Salvando...'
              : 'Salvar provento'}
          </button>
        </>
      }
    >
      <div className="provento-modal-body">
        {erro ? (
          <div className="provento-form-error">
            {erro}
          </div>
        ) : null}

        <div className="provento-form-grid">
          {!modoRateio ? (
            <label className="provento-field-full">
              <span>
                Investidor
              </span>
              <select
                value={investidorSelecionadoId}
                onChange={(event) =>
                  setInvestidorSelecionadoId(event.target.value)
                }
                disabled={Boolean(provento)}
              >
                <option value="">
                  Selecione
                </option>
                {investidores.map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.nome}
                  </option>
                ))}
              </select>
            </label>
          ) : null}

          <label>
            <span>
              Ativo
            </span>

            <input
              list="provento-tickers"
              value={
                ticker
              }
              onChange={(
                event,
              ) =>
                setTicker(
                  event.target
                    .value,
                )
              }
              placeholder="VALE3"
            />

            <datalist id="provento-tickers">
              {tickers.map(
                (item) => (
                  <option
                    key={
                      item
                    }
                    value={
                      item
                    }
                  />
                ),
              )}
            </datalist>
          </label>

          <label>
            <span>
              Tipo
            </span>

            <select
              value={
                tipo
              }
              onChange={(
                event,
              ) =>
                setTipo(
                  event.target
                    .value,
                )
              }
            >
              <option value="">
                Selecione
              </option>

              <option value="DIVIDENDO">
                Dividendo
              </option>

              <option value="JCP">
                JUROS
              </option>

              <option value="RENDIMENTO">
                Rendimento
              </option>
            </select>
          </label>

          <label>
            <span>
              Data-Com
            </span>

            <input
              type="date"
              value={
                dataCom
              }
              onChange={(
                event,
              ) =>
                setDataCom(
                  event.target
                    .value,
                )
              }
            />
          </label>

          <label>
            <span>
              Data de pagamento
            </span>

            <input
              type="date"
              value={
                dataPagamento
              }
              onChange={(
                event,
              ) =>
                setDataPagamento(
                  event.target
                    .value,
                )
              }
            />
          </label>

          {modoRateio && !provento ? (
            <label className="provento-field-full">
              <span>
                Valor total do provento
              </span>

              <input
                type="number"
                min="0"
                step="0.01"
                value={valorTotal}
                onChange={(event) =>
                  setValorTotal(event.target.value)
                }
                placeholder="0,00"
              />
            </label>
          ) : (
            <>
          <label>
            <span>
              Quantidade base
            </span>

            <input
              type="number"
              min="0"
              step="1"
              value={
                quantidadeBase
              }
              onChange={(
                event,
              ) =>
                setQuantidadeBase(
                  event.target
                    .value,
                )
              }
            />
          </label>

          <label>
            <span>
              Valor por unidade
            </span>

            <input
              type="number"
              min="0"
              step="0.000001"
              value={
                valorPorUnidade
              }
              onChange={(
                event,
              ) =>
                setValorPorUnidade(
                  event.target
                    .value,
                )
              }
            />
          </label>

          <label className="provento-field-full">
            <span>
              Valor efetivamente recebido
            </span>

            <input
              type="number"
              min="0"
              step="0.01"
              value={
                valorRecebido
              }
              onChange={(
                event,
              ) =>
                setValorRecebido(
                  event.target
                    .value,
                )
              }
            />
          </label>


            </>
          )}

          <label className="provento-field-full">
            <span>
              Descrição
            </span>

            <select
              value={
                descricao
              }
              onChange={(
                event,
              ) =>
                setDescricao(
                  event.target
                    .value,
                )
              }
            >
              <option value="">
                Selecione
              </option>

              <option value="Ação">
                Ação
              </option>

              <option value="FII">
                FII
              </option>
            </select>
          </label>
        </div>

        {!modoRateio || provento ? (
        <div className="provento-calculo">
          <div>
            <span>
              Valor bruto
            </span>

            <strong>
              {formatarMoeda(
                valorBruto,
              )}
            </strong>
          </div>

          <div>
            <span>
              IR retido
            </span>

            <strong>
              {formatarMoeda(
                imposto,
              )}
            </strong>
          </div>

          <div className="provento-calculo-destaque">
            <span>
              Valor recebido
            </span>

            <strong>
              {formatarMoeda(
                recebido,
              )}
            </strong>
          </div>
        </div>
        ) : (
          <div className="provento-calculo">
            <div>
              <span>
                Distribuição
              </span>
              <strong>
                Automática pela posição na Data-Com
              </strong>
            </div>
          </div>
        )}
      </div>
    </Modal>
  )
}