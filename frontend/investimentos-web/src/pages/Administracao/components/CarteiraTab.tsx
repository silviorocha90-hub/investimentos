import type {
  Administracao,
} from '../../../types/administracao'
import {
  formatarDataCurta,
  formatarMoeda,
} from '../../../utils/formatters'

interface CarteiraTabProps {
  dados: Administracao
}

export function CarteiraTab({
  dados,
}: CarteiraTabProps) {
  const totalDescontos =
    dados.descontosFiscais.reduce(
      (
        total,
        desconto,
      ) =>
        total +
        desconto.valor,
      0,
    )

  return (
    <article className="panel admin-panel">
      <div className="admin-toolbar">
        <strong>
          Descontos Fiscais
        </strong>
      </div>

      <div className="admin-summary-grid">
        <div>
          <span>
            Total de descontos fiscais
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
            {
              dados
                .descontosFiscais
                .length
            }
          </strong>
        </div>
      </div>

      <div className="admin-table-wrap">
        <table className="data-table admin-table">
          <thead>
            <tr>
              <th>
                Data de pagamento
              </th>

              <th>
                Tipo
              </th>

              <th>
                Descrição
              </th>

              <th>
                Valor
              </th>

              <th>
                Ações
              </th>
            </tr>
          </thead>

          <tbody>
            {dados
              .descontosFiscais
              .map(
                (desconto) => (
                  <tr
                    key={
                      desconto.id
                    }
                  >
                    <td>
                      {formatarDataCurta(
                        desconto
                          .dataPagamento,
                      )}
                    </td>

                    <td>
                      {
                        desconto.tipo
                      }
                    </td>

                    <td>
                      {desconto
                        .descricao ??
                        '—'}
                    </td>

                    <td>
                      {formatarMoeda(
                        desconto.valor,
                      )}
                    </td>

                    <td>
                      <button
                        type="button"
                        className="admin-action"
                        disabled
                      >
                        Editar
                      </button>
                    </td>
                  </tr>
                ),
              )}

            {dados
              .descontosFiscais
              .length === 0 ? (
              <tr>
                <td colSpan={5}>
                  Nenhum desconto fiscal cadastrado.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </article>
  )
}