import {
  useMemo,
} from 'react'
import type {
  Dispatch,
  SetStateAction,
} from 'react'
import type {
  Administracao,
  AtivoAdministracao,
} from '../../../types/administracao'
import {
  formatarDataCurta,
  formatarMoeda,
} from '../../../utils/formatters'

interface AtivosTabProps {
  dados: Administracao
  erro: string | null

  filtroInvestidor: string
  filtroAtivo: string
  filtroClasse: string
  filtroTipo: string

  pagina: number

  setFiltroInvestidor:
    Dispatch<SetStateAction<string>>

  setFiltroAtivo:
    Dispatch<SetStateAction<string>>

  setFiltroClasse:
    Dispatch<SetStateAction<string>>

  setFiltroTipo:
    Dispatch<SetStateAction<string>>

  setPagina:
    Dispatch<SetStateAction<number>>

  abrirNovoAtivo: () => void

  abrirEditarAtivo:
    (
      ativo: AtivoAdministracao,
    ) => void

  excluirAtivo:
    (
      ativo: AtivoAdministracao,
    ) => Promise<void>

  excluindo: boolean
}

function ehOutroInvestimento(
  ativo: AtivoAdministracao,
) {
  const ticker =
    ativo.ticker
      .trim()
      .toUpperCase()

  const tipo =
    ativo.tipoAtivoCodigo
      .trim()
      .toUpperCase()

  if (tipo === 'PREVIDENCIA') {
    return true
  }

  if (
    ticker === 'CDB NEON' ||
    ticker === 'CDB BTG' ||
    ticker === 'FMP ELETROBRAS'
  ) {
    return true
  }

  return (
    ticker.includes(
      'FMP ELETROBRAS',
    ) ||
    ticker.includes(
      'ELETROBRAS',
    ) ||
    ticker.includes(
      'CDB',
    ) ||
    tipo === 'FMP'
  )
}

export function AtivosTab({
  dados,
  erro,

  filtroInvestidor,
  filtroAtivo,
  filtroClasse,
  filtroTipo,

  pagina,

  setFiltroInvestidor,
  setFiltroAtivo,
  setFiltroClasse,
  setFiltroTipo,
  setPagina,

  abrirNovoAtivo,
  abrirEditarAtivo,
  excluirAtivo,
  excluindo,
}: AtivosTabProps) {
  const investidoresDisponiveis =
    useMemo(
      () =>
        dados.investidores
          .slice()
          .sort(
            (a, b) =>
              a.nome.localeCompare(
                b.nome,
                'pt-BR',
              ),
          ),
      [dados.investidores],
    )

  const ativosDoInvestidor =
    useMemo(
      () =>
        dados.ativos.filter(
          (ativo) =>
            filtroInvestidor ===
              'TODOS'
              ? ativo.quantidade > 0
              : ativo
                  .posicoesInvestidores
                  .some(
                    (posicao) =>
                      posicao.investidorId ===
                        filtroInvestidor &&
                      posicao.quantidade >
                        0,
                  ),
        ),
      [
        dados.ativos,
        filtroInvestidor,
      ],
    )

  const classesDisponiveis =
    useMemo(
      () =>
        dados.classesAtivo
          .filter(
            (classe) =>
              classe.ativo &&
              ativosDoInvestidor.some(
                (ativo) =>
                  ativo.classeAtivoId ===
                  classe.id,
              ),
          )
          .slice()
          .sort(
            (a, b) =>
              a.nome.localeCompare(
                b.nome,
                'pt-BR',
              ),
          ),
      [
        dados.classesAtivo,
        ativosDoInvestidor,
      ],
    )

  const tiposDisponiveis =
    useMemo(() => {
      if (
        filtroClasse ===
        'TODOS'
      ) {
        return []
      }

      return dados.tiposAtivo
        .filter(
          (tipo) =>
            tipo.ativo &&
            String(
              tipo.classeAtivoId,
            ) ===
              filtroClasse &&
            ativosDoInvestidor.some(
              (ativo) =>
                ativo.tipoAtivoId ===
                  tipo.id,
            ),
        )
        .slice()
        .sort(
          (a, b) =>
            a.nome.localeCompare(
              b.nome,
              'pt-BR',
            ),
        )
    }, [
      dados.tiposAtivo,
      ativosDoInvestidor,
      filtroClasse,
    ])

  const ativosDisponiveis =
    useMemo(() => {
      if (
        filtroClasse ===
          'TODOS' ||
        filtroTipo ===
          'TODOS'
      ) {
        return []
      }

      return ativosDoInvestidor
        .filter(
          (ativo) =>
            String(
              ativo.classeAtivoId,
            ) ===
              filtroClasse &&
            String(
              ativo.tipoAtivoId,
            ) ===
              filtroTipo,
        )
        .slice()
        .sort(
          (a, b) =>
            a.ticker.localeCompare(
              b.ticker,
              'pt-BR',
            ),
        )
    }, [
      ativosDoInvestidor,
      filtroClasse,
      filtroTipo,
    ])

  const ativosFiltrados =
    useMemo(
      () =>
        ativosDoInvestidor.filter(
          (ativo) =>
            (
              filtroClasse ===
                'TODOS' ||
              String(
                ativo.classeAtivoId,
              ) ===
                filtroClasse
            ) &&
            (
              filtroTipo ===
                'TODOS' ||
              String(
                ativo.tipoAtivoId,
              ) ===
                filtroTipo
            ) &&
            (
              filtroAtivo ===
                'TODOS' ||
              ativo.ticker ===
                filtroAtivo
            ),
        ),
      [
        ativosDoInvestidor,
        filtroClasse,
        filtroTipo,
        filtroAtivo,
      ],
    )

  const rendaVariavel =
    ativosFiltrados.filter(
      (ativo) =>
        !ehOutroInvestimento(
          ativo,
        ),
    )

  const outrosInvestimentos =
    ativosFiltrados.filter(
      (ativo) =>
        ehOutroInvestimento(
          ativo,
        ),
    )

  function obterValorAtual(
    ativo: AtivoAdministracao,
  ) {
    /*
     * Outros investimentos possuem valor patrimonial manual.
     * Esse é o valor corrente do ativo e deve prevalecer sobre
     * o custo calculado pelas operações, inclusive quando um
     * investidor específico estiver selecionado.
     */
    if (
      ehOutroInvestimento(
        ativo,
      ) &&
      ativo.valorPatrimonialAtual != null
    ) {
      return ativo.valorPatrimonialAtual
    }

    if (
      filtroInvestidor ===
      'TODOS'
    ) {
      return ativo.valorAtual
    }

    return (
      ativo
        .posicoesInvestidores
        .find(
          (posicao) =>
            posicao.investidorId ===
            filtroInvestidor,
        )
        ?.valorAtual ??
      0
    )
  }

  function alterarInvestidor(
    investidorId: string,
  ) {
    setFiltroInvestidor(
      investidorId,
    )

    setFiltroClasse(
      'TODOS',
    )

    setFiltroTipo(
      'TODOS',
    )

    setFiltroAtivo(
      'TODOS',
    )

    setPagina(1)
  }

  function alterarClasse(
    classeId: string,
  ) {
    setFiltroClasse(
      classeId,
    )

    setFiltroTipo(
      'TODOS',
    )

    setFiltroAtivo(
      'TODOS',
    )

    setPagina(1)
  }

  function alterarTipo(
    tipoId: string,
  ) {
    setFiltroTipo(
      tipoId,
    )

    setFiltroAtivo(
      'TODOS',
    )

    setPagina(1)
  }

  function alterarAtivo(
    ticker: string,
  ) {
    setFiltroAtivo(
      ticker,
    )

    setPagina(1)
  }

  function limparFiltros() {
    setFiltroInvestidor(
      'TODOS',
    )

    setFiltroClasse(
      'TODOS',
    )

    setFiltroTipo(
      'TODOS',
    )

    setFiltroAtivo(
      'TODOS',
    )

    setPagina(1)
  }

  function renderAcoes(
    ativo: AtivoAdministracao,
  ) {
    return (
      <div className="admin-row-actions">
        <button
          type="button"
          className="admin-action"
          onClick={() =>
            abrirEditarAtivo(
              ativo,
            )
          }
          disabled={
            excluindo
          }
        >
          Editar
        </button>

        <button
          type="button"
          className="admin-action admin-action-danger"
          onClick={() =>
            void excluirAtivo(
              ativo,
            )
          }
          disabled={
            excluindo
          }
        >
          Excluir
        </button>
      </div>
    )
  }

  return (
    <article className="panel admin-panel">
      <div className="admin-toolbar admin-assets-toolbar">
        <strong>
          Ativos
        </strong>

        <button
          type="button"
          className="admin-primary-button"
          onClick={
            abrirNovoAtivo
          }
        >
          + Novo Ativo
        </button>
      </div>

      <div className="admin-asset-filters">
        <label>
          <span>
            Investidor
          </span>

          <select
            value={
              filtroInvestidor
            }
            onChange={(
              event,
            ) =>
              alterarInvestidor(
                event.target.value,
              )
            }
          >
            <option value="TODOS">
              Todos
            </option>

            {investidoresDisponiveis.map(
              (investidor) => (
                <option
                  key={
                    investidor.id
                  }
                  value={
                    investidor.id
                  }
                >
                  {
                    investidor.nome
                  }
                </option>
              ),
            )}
          </select>
        </label>

        <label>
          <span>
            Classe
          </span>

          <select
            value={
              filtroClasse
            }
            onChange={(
              event,
            ) =>
              alterarClasse(
                event.target.value,
              )
            }
          >
            <option value="TODOS">
              Todas
            </option>

            {classesDisponiveis.map(
              (classe) => (
                <option
                  key={
                    classe.id
                  }
                  value={
                    classe.id
                  }
                >
                  {
                    classe.nome
                  }
                </option>
              ),
            )}
          </select>
        </label>

        <label>
          <span>
            Tipo
          </span>

          <select
            value={
              filtroTipo
            }
            disabled={
              filtroClasse ===
              'TODOS'
            }
            onChange={(
              event,
            ) =>
              alterarTipo(
                event.target.value,
              )
            }
          >
            <option value="TODOS">
              Todos
            </option>

            {tiposDisponiveis.map(
              (tipo) => (
                <option
                  key={
                    tipo.id
                  }
                  value={
                    tipo.id
                  }
                >
                  {
                    tipo.nome
                  }
                </option>
              ),
            )}
          </select>
        </label>

        <label>
          <span>
            Ativo
          </span>

          <select
            value={
              filtroAtivo
            }
            disabled={
              filtroClasse ===
                'TODOS' ||
              filtroTipo ===
                'TODOS'
            }
            onChange={(
              event,
            ) =>
              alterarAtivo(
                event.target.value,
              )
            }
          >
            <option value="TODOS">
              Todos
            </option>

            {ativosDisponiveis.map(
              (ativo) => (
                <option
                  key={
                    ativo.id
                  }
                  value={
                    ativo.ticker
                  }
                >
                  {
                    ativo.ticker
                  }
                </option>
              ),
            )}
          </select>
        </label>

        <button
          type="button"
          className="admin-clear-button"
          onClick={
            limparFiltros
          }
          disabled={
            filtroInvestidor ===
              'TODOS' &&
            filtroClasse ===
              'TODOS' &&
            filtroTipo ===
              'TODOS' &&
            filtroAtivo ===
              'TODOS'
          }
        >
          Limpar filtros
        </button>
      </div>

      {erro ? (
        <div className="admin-inline-error">
          {erro}
        </div>
      ) : null}

      <div className="admin-asset-section">
        <div className="admin-asset-section-title">
          <strong>
            Renda Variável
          </strong>

          <span>
            Ações, FIIs e ativos com cotação de mercado
          </span>
        </div>

        <div className="admin-table-wrap">
          <table className="data-table admin-table admin-assets-table">
            <thead>
              <tr>
                <th>Ativo</th>
                <th>Classe</th>
                <th>Tipo</th>
                <th>Cotação</th>
                <th>Data Cotação</th>
                <th>Ações</th>
              </tr>
            </thead>

            <tbody>
              {rendaVariavel.map(
                (ativo) => (
                  <tr
                    key={
                      ativo.id
                    }
                  >
                    <td>
                      <span className="ticker">
                        {
                          ativo.ticker
                        }
                      </span>
                    </td>

                    <td>
                      {
                        ativo.classeAtivoNome
                      }
                    </td>

                    <td>
                      {
                        ativo.tipoAtivoNome
                      }
                    </td>

                    <td>
                      {ativo.cotacaoAtual ==
                      null
                        ? '—'
                        : formatarMoeda(
                            ativo.cotacaoAtual,
                          )}
                    </td>

                    <td>
                      {ativo.dataCotacao
                        ? formatarDataCurta(
                            ativo.dataCotacao,
                          )
                        : '—'}
                    </td>

                    <td>
                      {renderAcoes(
                        ativo,
                      )}
                    </td>
                  </tr>
                ),
              )}

              {rendaVariavel.length ===
              0 ? (
                <tr>
                  <td colSpan={6}>
                    Nenhum ativo de renda variável encontrado.
                  </td>
                </tr>
              ) : null}
            </tbody>
          </table>
        </div>
      </div>

      <div className="admin-asset-section">
        <div className="admin-asset-section-title">
          <strong>
            Outros Investimentos
          </strong>

          <span>
            Renda fixa, FMP e previdência pelo valor total
          </span>
        </div>

        <div className="admin-table-wrap">
          <table className="data-table admin-table admin-assets-table admin-other-assets-table">
            <thead>
              <tr>
                <th>Investimento</th>
                <th>Classe</th>
                <th>Tipo</th>
                <th>Valor Total</th>
                <th>Ações</th>
              </tr>
            </thead>

            <tbody>
              {outrosInvestimentos.map(
                (ativo) => (
                  <tr
                    key={
                      ativo.id
                    }
                  >
                    <td>
                      <span className="ticker">
                        {
                          ativo.ticker
                        }
                      </span>
                    </td>

                    <td>
                      {
                        ativo.classeAtivoNome
                      }
                    </td>

                    <td>
                      {
                        ativo.tipoAtivoNome
                      }
                    </td>

                    <td>
                      {formatarMoeda(
                        obterValorAtual(
                          ativo,
                        ),
                      )}
                    </td>

                    <td>
                      {renderAcoes(
                        ativo,
                      )}
                    </td>
                  </tr>
                ),
              )}

              {outrosInvestimentos.length ===
              0 ? (
                <tr>
                  <td colSpan={5}>
                    Nenhum outro investimento encontrado.
                  </td>
                </tr>
              ) : null}
            </tbody>
          </table>
        </div>
      </div>


    </article>
  )
}
