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
import {
  Pagination,
} from '../../../components/Pagination'

interface AtivosTabProps {
  dados: Administracao
  erro: string | null

  filtroAtivo: string
  filtroClasse: string
  filtroTipo: string

  pagina: number

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
}

export function AtivosTab({
  dados,
  erro,

  filtroAtivo,
  filtroClasse,
  filtroTipo,

  pagina,

  setFiltroAtivo,
  setFiltroClasse,
  setFiltroTipo,
  setPagina,

  abrirNovoAtivo,
  abrirEditarAtivo,
}: AtivosTabProps) {
  const itensPorPagina = 12

  /*
   * Somente ativos que fazem parte
   * efetivamente da carteira.
   */
  const ativosComPosicao =
    useMemo(
      () =>
        dados.ativos.filter(
          (ativo) =>
            ativo
              .posicoesInvestidores
              .some(
                (posicao) =>
                  posicao.quantidade >
                  0,
              ),
        ),
      [dados.ativos],
    )

  /*
   * CLASSES
   *
   * Primeiro nível do filtro.
   *
   * Exibimos somente classes ativas
   * que possuem pelo menos um ativo
   * presente na carteira.
   */
  const classesDisponiveis =
    useMemo(
      () =>
        dados.classesAtivo
          .filter(
            (classe) =>
              classe.ativo &&
              ativosComPosicao.some(
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
        ativosComPosicao,
      ],
    )

  /*
   * TIPOS
   *
   * Segundo nível.
   *
   * Só existem opções quando uma
   * classe foi selecionada.
   *
   * Também eliminamos tipos que não
   * possuem nenhum ativo na carteira.
   */
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
            ativosComPosicao.some(
              (ativo) =>
                ativo.tipoAtivoId ===
                tipo.id &&
                String(
                  ativo.classeAtivoId,
                ) ===
                  filtroClasse,
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
      ativosComPosicao,
      filtroClasse,
    ])

  /*
   * ATIVOS
   *
   * Terceiro nível.
   *
   * Somente ativos pertencentes
   * simultaneamente à Classe e ao
   * Tipo selecionados.
   */
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

      return ativosComPosicao
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
      ativosComPosicao,
      filtroClasse,
      filtroTipo,
    ])

  /*
   * Resultado apresentado na tabela.
   *
   * Os filtros são aplicados
   * hierarquicamente:
   *
   * Classe -> Tipo -> Ativo
   */
  const ativosFiltrados =
    useMemo(
      () =>
        ativosComPosicao.filter(
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
        ativosComPosicao,
        filtroClasse,
        filtroTipo,
        filtroAtivo,
      ],
    )

  const totalPaginas =
    Math.max(
      1,
      Math.ceil(
        ativosFiltrados.length /
          itensPorPagina,
      ),
    )

  const paginaNormalizada =
    Math.min(
      pagina,
      totalPaginas,
    )

  const ativosPagina =
    ativosFiltrados.slice(
      (
        paginaNormalizada -
        1
      ) * itensPorPagina,

      paginaNormalizada *
        itensPorPagina,
    )

  /*
   * Mudança de Classe.
   *
   * Tipo e Ativo deixam de ser
   * válidos e precisam ser zerados.
   */
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

  /*
   * Mudança de Tipo.
   *
   * O ativo anteriormente escolhido
   * pode não pertencer ao novo tipo.
   */
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
        {/* CLASSE */}
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

        {/* TIPO */}
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

        {/* ATIVO */}
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

      <div className="admin-table-wrap">
        <table className="data-table admin-table admin-assets-table">
          <thead>
            <tr>
              <th>
                Ativo
              </th>

              <th>
                Classe
              </th>

              <th>
                Tipo
              </th>

              <th>
                Cotação
              </th>

              <th>
                Data Cotação
              </th>

              <th>
                Ações
              </th>
            </tr>
          </thead>

          <tbody>
            {ativosPagina.map(
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
                      ativo
                        .classeAtivoNome
                    }
                  </td>

                  <td>
                    {
                      ativo
                        .tipoAtivoNome
                    }
                  </td>

                  <td>
                    {ativo
                      .cotacaoAtual ==
                    null
                      ? '—'
                      : formatarMoeda(
                          ativo
                            .cotacaoAtual,
                        )}
                  </td>

                  <td>
                    {ativo
                      .dataCotacao
                      ? formatarDataCurta(
                          ativo
                            .dataCotacao,
                        )
                      : '—'}
                  </td>

                  <td>
                    <button
                      type="button"
                      className="admin-action"
                      onClick={() =>
                        abrirEditarAtivo(
                          ativo,
                        )
                      }
                    >
                      Editar
                    </button>
                  </td>
                </tr>
              ),
            )}

            {ativosPagina.length ===
            0 ? (
              <tr>
                <td colSpan={6}>
                  Nenhum ativo encontrado
                  para os filtros
                  selecionados.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>

      <Pagination
        page={
          paginaNormalizada
        }
        totalPages={
          totalPaginas
        }
        totalItems={
          ativosFiltrados.length
        }
        itemLabel="ativos"
        onPageChange={
          setPagina
        }
      />
    </article>
  )
}