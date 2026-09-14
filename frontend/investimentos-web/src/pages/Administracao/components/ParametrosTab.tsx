import {
  useState,
} from 'react'
import type {
  Administracao,
} from '../../../types/administracao'

type AbaParametro =
  | 'classes'
  | 'tipos'
  | 'operacoes'

interface ParametrosTabProps {
  dados: Administracao
}

export function ParametrosTab({
  dados,
}: ParametrosTabProps) {
  const [
    abaParametro,
    setAbaParametro,
  ] = useState<AbaParametro>(
    'classes',
  )

  return (
    <article className="panel admin-panel">
      <div className="admin-subtabs">
        <button
          type="button"
          className={
            abaParametro ===
            'classes'
              ? 'active'
              : ''
          }
          onClick={() =>
            setAbaParametro(
              'classes',
            )
          }
        >
          Classes de Ativo
        </button>

        <button
          type="button"
          className={
            abaParametro ===
            'tipos'
              ? 'active'
              : ''
          }
          onClick={() =>
            setAbaParametro(
              'tipos',
            )
          }
        >
          Tipos de Ativo
        </button>

        <button
          type="button"
          className={
            abaParametro ===
            'operacoes'
              ? 'active'
              : ''
          }
          onClick={() =>
            setAbaParametro(
              'operacoes',
            )
          }
        >
          Tipos de Operação
        </button>
      </div>

      <div className="admin-table-wrap">
        <table className="data-table admin-table">
          <thead>
            {abaParametro ===
            'tipos' ? (
              <tr>
                <th>
                  Código
                </th>

                <th>
                  Nome
                </th>

                <th>
                  Classe
                </th>

                <th>
                  Status
                </th>

                <th>
                  Ações
                </th>
              </tr>
            ) : (
              <tr>
                <th>
                  Código
                </th>

                <th>
                  Nome
                </th>

                <th>
                  Status
                </th>

                <th>
                  Ações
                </th>
              </tr>
            )}
          </thead>

          <tbody>
            {abaParametro ===
            'classes'
              ? dados
                  .classesAtivo
                  .map(
                    (item) => (
                      <tr
                        key={
                          item.id
                        }
                      >
                        <td>
                          <span className="ticker">
                            {
                              item.codigo
                            }
                          </span>
                        </td>

                        <td>
                          {
                            item.nome
                          }
                        </td>

                        <td>
                          {item.ativo
                            ? 'Ativo'
                            : 'Inativo'}
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
                  )
              : null}

            {abaParametro ===
            'tipos'
              ? dados
                  .tiposAtivo
                  .map(
                    (item) => (
                      <tr
                        key={
                          item.id
                        }
                      >
                        <td>
                          <span className="ticker">
                            {
                              item.codigo
                            }
                          </span>
                        </td>

                        <td>
                          {
                            item.nome
                          }
                        </td>

                        <td>
                          {
                            item
                              .classeAtivoNome
                          }
                        </td>

                        <td>
                          {item.ativo
                            ? 'Ativo'
                            : 'Inativo'}
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
                  )
              : null}

            {abaParametro ===
            'operacoes'
              ? dados
                  .tiposOperacao
                  .map(
                    (item) => (
                      <tr
                        key={
                          item.id
                        }
                      >
                        <td>
                          <span className="ticker">
                            {
                              item.codigo
                            }
                          </span>
                        </td>

                        <td>
                          {
                            item.nome
                          }
                        </td>

                        <td>
                          {item.ativo
                            ? 'Ativo'
                            : 'Inativo'}
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
                  )
              : null}
          </tbody>
        </table>
      </div>
    </article>
  )
}