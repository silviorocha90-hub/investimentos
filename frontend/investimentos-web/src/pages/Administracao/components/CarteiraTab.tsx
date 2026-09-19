import {
  useState,
} from 'react'
import {
  atualizarDescontoFiscal,
  excluirDescontoFiscal,
} from '../../../api/administracaoApi'
import {
  Modal,
} from '../../../components/Modal'
import type {
  Administracao,
  DescontoFiscalAdministracao,
} from '../../../types/administracao'
import {
  formatarDataCurta,
  formatarMoeda,
} from '../../../utils/formatters'

interface CarteiraTabProps {
  dados: Administracao
  recarregar?: () => Promise<void>
}

export function CarteiraTab({
  dados,
  recarregar,
}: CarteiraTabProps) {
  const [editando, setEditando] =
    useState<DescontoFiscalAdministracao | null>(null)
  const [dataPagamento, setDataPagamento] = useState('')
  const [valor, setValor] = useState('')
  const [descricao, setDescricao] = useState('')
  const [salvando, setSalvando] = useState(false)
  const [erro, setErro] = useState<string | null>(null)

  const totalDescontos =
    dados.descontosFiscais.reduce(
      (total, desconto) =>
        total + desconto.valor,
      0,
    )

  function abrirEdicao(
    desconto: DescontoFiscalAdministracao,
  ) {
    setEditando(desconto)
    setDataPagamento(
      desconto.dataPagamento.slice(0, 10),
    )
    setValor(
      desconto.valor.toLocaleString(
        'pt-BR',
        {
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        },
      ),
    )
    setDescricao(
      desconto.descricao ?? '',
    )
    setErro(null)
  }

  function paraNumero(
    texto: string,
  ) {
    const normalizado =
      texto
        .replace(/\./g, '')
        .replace(',', '.')

    return Number(normalizado)
  }

  function formatarEntradaMoeda(
    texto: string,
  ) {
    const digitos =
      texto.replace(/\D/g, '')

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

  async function salvar() {
    if (!editando) return

    const valorNumerico =
      paraNumero(valor)

    if (
      !dataPagamento ||
      !Number.isFinite(valorNumerico) ||
      valorNumerico <= 0
    ) {
      setErro(
        'Informe data e valor válidos.',
      )
      return
    }

    try {
      setSalvando(true)
      setErro(null)

      await atualizarDescontoFiscal(
        editando.id,
        {
          dataPagamento,
          valor: valorNumerico,
          descricao:
            descricao.trim() || null,
        },
      )

      setEditando(null)
      await recarregar?.()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível atualizar o imposto.',
      )
    } finally {
      setSalvando(false)
    }
  }

  async function excluir(
    desconto: DescontoFiscalAdministracao,
  ) {
    if (
      !window.confirm(
        'Deseja excluir este imposto?',
      )
    ) {
      return
    }

    try {
      setErro(null)
      await excluirDescontoFiscal(
        desconto.id,
      )
      await recarregar?.()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível excluir o imposto.',
      )
    }
  }

  return (
    <>
      <article className="panel admin-panel">
        <div className="admin-toolbar">
          <strong>
            DARF
          </strong>
        </div>

        {erro ? (
          <div className="admin-inline-error">
            {erro}
          </div>
        ) : null}

        <div className="admin-summary-grid">
          <div>
            <span>
              Total de impostos
            </span>

            <strong>
              {formatarMoeda(
                totalDescontos,
              )}
            </strong>
          </div>

          <div>
            <span>
              Registros
            </span>

            <strong>
              {dados.descontosFiscais.length}
            </strong>
          </div>
        </div>

        <div className="admin-table-wrap">
          <table className="data-table admin-table">
            <thead>
              <tr>
                <th>Data de pagamento</th>
                <th>Tipo</th>
                <th>Descrição</th>
                <th>Valor</th>
                <th>Ações</th>
              </tr>
            </thead>

            <tbody>
              {dados.descontosFiscais.map(
                (desconto) => (
                  <tr key={desconto.id}>
                    <td>
                      {formatarDataCurta(
                        desconto.dataPagamento,
                      )}
                    </td>

                    <td>
                      Imposto
                    </td>

                    <td>
                      {desconto.descricao ?? '—'}
                    </td>

                    <td>
                      {formatarMoeda(
                        desconto.valor,
                      )}
                    </td>

                    <td>
                      <div className="admin-row-actions">
                        <button
                          type="button"
                          className="admin-action"
                          onClick={() =>
                            abrirEdicao(desconto)
                          }
                        >
                          Editar
                        </button>

                        <button
                          type="button"
                          className="admin-action admin-action-danger"
                          onClick={() =>
                            excluir(desconto)
                          }
                        >
                          Excluir
                        </button>
                      </div>
                    </td>
                  </tr>
                ),
              )}

              {dados.descontosFiscais.length === 0 ? (
                <tr>
                  <td colSpan={5}>
                    Nenhum imposto cadastrado.
                  </td>
                </tr>
              ) : null}
            </tbody>
          </table>
        </div>
      </article>

      {editando ? (
        <Modal
          title="Editar DARF"
          subtitle="Imposto"
          onClose={() =>
            setEditando(null)
          }
          closeDisabled={salvando}
          footer={
            <>
              <button
                type="button"
                className="admin-clear-button"
                disabled={salvando}
                onClick={() =>
                  setEditando(null)
                }
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
                  : 'Salvar'}
              </button>
            </>
          }
        >
          <div className="admin-modal-grid">
            <label>
              <span>Tipo</span>
              <input
                value="Imposto"
                readOnly
              />
            </label>

            <label>
              <span>Data de pagamento</span>
              <input
                type="date"
                value={dataPagamento}
                onChange={(event) =>
                  setDataPagamento(
                    event.target.value,
                  )
                }
              />
            </label>

            <label>
              <span>Valor</span>
              <input
                inputMode="decimal"
                value={valor}
                onChange={(event) =>
                  setValor(
                    formatarEntradaMoeda(
                      event.target.value,
                    ),
                  )
                }
              />
            </label>

            <label>
              <span>Descrição</span>
              <input
                value={descricao}
                onChange={(event) =>
                  setDescricao(
                    event.target.value,
                  )
                }
              />
            </label>
          </div>
        </Modal>
      ) : null}
    </>
  )
}
