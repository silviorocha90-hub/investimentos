import {
  useMemo,
  useState,
} from 'react'

import {
  criarOperacao,
} from '../../api/dashboardApi'

import { PageHeader } from '../../components/PageHeader'
import { SectionTitle } from '../../components/SectionTitle'

import type {
  OperacaoCarteira,
} from '../../types/dashboard'

import type {
  Investidor,
} from '../../types/investidor'

import {
  formatarDataCurta,
  formatarMoeda,
  formatarNumeroInteiro,
} from '../../utils/formatters'

import {
  OperacaoModal,
} from './components/OperacaoModal'

import type {
  NovaOperacaoForm,
} from './components/OperacaoModal'

import './Operacoes.css'

interface OperacoesViewProps {
  investidores: readonly Investidor[]

  selectedInvestor: string

  onSelectInvestor: (
    nome: string,
  ) => void

  operacoes?: readonly OperacaoCarteira[]

  onSaveOperation: (
    operacao: Pick<
      OperacaoCarteira,
      | 'id'
      | 'data'
      | 'quantidade'
      | 'precoUnitario'
      | 'taxas'
    >,
  ) => Promise<void>
}

function criarFormularioInicial(): NovaOperacaoForm {
  return {
    ticker: '',
    tipoOperacaoCodigo: 'COMPRA',
    quantidade: '',
    precoUnitario: '',
    taxas: '0',
    data: new Date()
      .toISOString()
      .split('T')[0],
  }
}

export function OperacoesView({
  investidores,
  selectedInvestor,
  onSelectInvestor,
  operacoes = [],
  onSaveOperation,
}: OperacoesViewProps) {
  const [
    operacaoEmEdicao,
    setOperacaoEmEdicao,
  ] = useState<string | null>(
    null,
  )

  const [
    salvandoOperacao,
    setSalvandoOperacao,
  ] = useState(false)

  const [
    deletandoOperacao,
    setDeletandoOperacao,
  ] = useState(false)

  const [
    ativoSelecionado,
    setAtivoSelecionado,
  ] = useState('TODOS')

  const [
    adicionandoOperacao,
    setAdicionandoOperacao,
  ] = useState(false)

  const [
    criandoOperacao,
    setCriandoOperacao,
  ] = useState(false)

  const [
    erroOperacao,
    setErroOperacao,
  ] = useState<string | null>(
    null,
  )

  const [
    formNovaOperacao,
    setFormNovaOperacao,
  ] = useState<NovaOperacaoForm>(
    criarFormularioInicial,
  )

  const ativosDisponiveis =
    useMemo(
      () =>
        Array.from(
          new Set(
            operacoes
              .map(
                (operacao) =>
                  operacao.ticker,
              )
              .filter(
                (ticker) =>
                  ticker
                    .trim()
                    .length > 0,
              ),
          ),
        ).sort(
          (a, b) =>
            a.localeCompare(
              b,
              'pt-BR',
            ),
        ),
      [operacoes],
    )

  const operacoesFiltradas =
    useMemo(
      () =>
        ativoSelecionado ===
        'TODOS'
          ? operacoes
          : operacoes.filter(
              (operacao) =>
                operacao.ticker ===
                ativoSelecionado,
            ),
      [
        ativoSelecionado,
        operacoes,
      ],
    )

  const operacoesOrdenadas =
    useMemo(
      () =>
        operacoesFiltradas
          .slice()
          .sort(
            (a, b) => {
              const porData =
                b.data.localeCompare(
                  a.data,
                )

              if (porData !== 0) {
                return porData
              }

              return (
                (b.sequencia ?? 0) -
                (a.sequencia ?? 0)
              )
            },
          ),
      [operacoesFiltradas],
    )

  const abrirNovaOperacao =
    () => {
      setOperacaoEmEdicao(
        null,
      )

      setFormNovaOperacao(
        criarFormularioInicial(),
      )

      setErroOperacao(
        null,
      )

      setAdicionandoOperacao(
        true,
      )
    }

  const cancelarNovaOperacao =
    () => {
      if (criandoOperacao) {
        return
      }

      setAdicionandoOperacao(
        false,
      )

      setErroOperacao(
        null,
      )

      setFormNovaOperacao(
        criarFormularioInicial(),
      )
    }

  const handleDeleteOperation =
    async (
      operacaoId: string,
    ) => {
      if (
        !window.confirm(
          'Tem certeza que deseja deletar esta operação? Esta ação não pode ser desfeita.',
        )
      ) {
        return
      }

      try {
        setDeletandoOperacao(
          true,
        )

        setErroOperacao(
          null,
        )

        const apiUrl =
          import.meta.env
            .VITE_API_URL ??
          'https://localhost:7237'

        const response =
          await fetch(
            `${apiUrl}/api/operacoes/${operacaoId}`,
            {
              method: 'DELETE',
            },
          )

        if (!response.ok) {
          const detalhe =
            await response.text()

          throw new Error(
            detalhe.trim() ||
              `Não foi possível deletar a operação. HTTP ${response.status}.`,
          )
        }

        window.location.reload()
      } catch (error) {
        console.error(
          'Erro ao deletar operação:',
          error,
        )

        const mensagem =
          error instanceof Error
            ? error.message
            : 'Não foi possível deletar a operação.'

        setErroOperacao(
          mensagem,
        )

        alert(mensagem)
      } finally {
        setDeletandoOperacao(
          false,
        )
      }
    }

  const handleAdicionarOperacao =
    async (
      event: React.FormEvent<HTMLFormElement>,
    ) => {
      event.preventDefault()

      setErroOperacao(
        null,
      )

      if (
        !formNovaOperacao.ticker.trim()
      ) {
        setErroOperacao(
          'Ticker é obrigatório.',
        )

        return
      }

      if (
        !formNovaOperacao.quantidade ||
        Number(
          formNovaOperacao.quantidade,
        ) <= 0
      ) {
        setErroOperacao(
          'Quantidade deve ser maior que 0.',
        )

        return
      }

      if (
        !formNovaOperacao.precoUnitario ||
        Number(
          formNovaOperacao.precoUnitario,
        ) <= 0
      ) {
        setErroOperacao(
          'Preço unitário deve ser maior que 0.',
        )

        return
      }

      try {
        setCriandoOperacao(
          true,
        )

        const investidor =
          investidores.find(
            (item) =>
              item.nome ===
              selectedInvestor,
          )

        if (!investidor) {
          setErroOperacao(
            'Investidor não selecionado.',
          )

          return
        }

        await criarOperacao(
          investidor.id,
          {
            data:
              formNovaOperacao.data,

            ticker:
              formNovaOperacao.ticker
                .trim()
                .toUpperCase(),

            tipoOperacaoCodigo:
              formNovaOperacao
                .tipoOperacaoCodigo,

            quantidade:
              Number(
                formNovaOperacao.quantidade,
              ),

            precoUnitario:
              Number(
                formNovaOperacao.precoUnitario,
              ),

            taxas:
              Number(
                formNovaOperacao.taxas,
              ) || 0,
          },
        )

        window.location.reload()
      } catch (error) {
        console.error(
          'Erro ao criar operação:',
          error,
        )

        setErroOperacao(
          error instanceof Error
            ? error.message
            : 'Erro ao criar operação.',
        )
      } finally {
        setCriandoOperacao(
          false,
        )
      }
    }

  return (
    <section className="portfolio-view operacoes-view">
      <PageHeader
        titulo="Operações"
        investidores={
          investidores
        }
        selectedInvestor={
          selectedInvestor
        }
        onSelectInvestor={
          onSelectInvestor
        }
      />

      <article className="panel portfolio-operations">
        <div className="operations-table-header">
          <SectionTitle
            title={`Operações de ${selectedInvestor}`}
            badge={`${operacoesFiltradas.length} operações`}
          />

          <div className="operations-table-tools">
            <label className="operations-asset-filter">
              <span>
                Ativo
              </span>

              <select
                value={
                  ativoSelecionado
                }
                onChange={(
                  event,
                ) =>
                  setAtivoSelecionado(
                    event.target.value,
                  )
                }
              >
                <option value="TODOS">
                  Todos
                </option>

                {ativosDisponiveis.map(
                  (ticker) => (
                    <option
                      key={ticker}
                      value={ticker}
                    >
                      {ticker}
                    </option>
                  ),
                )}
              </select>
            </label>

            <button
              type="button"
              className="operations-new-button"
              disabled={
                criandoOperacao
              }
              onClick={
                abrirNovaOperacao
              }
            >
              + Operação
            </button>
          </div>
        </div>

        {operacoesOrdenadas.length >
        0 ? (
          <div className="table-wrap compact operations-table-wrap">
            <table className="data-table positions-table operations-table">
              <thead>
                <tr>
                  <th>Data</th>

                  <th>Ativo</th>

                  <th>Tipo</th>

                  <th className="align-right">
                    Quantidade
                  </th>

                  <th className="align-right">
                    Preço
                  </th>

                  <th className="align-right">
                    Taxas
                  </th>

                  <th>Ações</th>
                </tr>
              </thead>

              <tbody>
                {operacoesOrdenadas.map(
                  (operacao) => {
                    const editando =
                      operacaoEmEdicao ===
                      operacao.id

                    return (
                      <tr
                        key={
                          operacao.id
                        }
                      >
                        {editando ? (
                          <>
                            <td>
                              <input
                                name="data"
                                type="date"
                                defaultValue={
                                  operacao.data.slice(
                                    0,
                                    10,
                                  )
                                }
                              />
                            </td>

                            <td>
                              <span className="ticker">
                                {
                                  operacao.ticker
                                }
                              </span>
                            </td>

                            <td>
                              {
                                operacao.tipoOperacao
                              }
                            </td>

                            <td className="align-right">
                              <input
                                name="quantidade"
                                type="number"
                                min="0.0001"
                                step="any"
                                defaultValue={
                                  operacao.quantidade
                                }
                              />
                            </td>

                            <td className="align-right">
                              <input
                                name="precoUnitario"
                                type="number"
                                min="0"
                                step="0.01"
                                defaultValue={
                                  operacao.precoUnitario
                                }
                              />
                            </td>

                            <td className="align-right">
                              <input
                                name="taxas"
                                type="number"
                                min="0"
                                step="0.01"
                                defaultValue={
                                  operacao.taxas
                                }
                              />
                            </td>

                            <td className="operation-actions">
                              <button
                                className="table-action primary"
                                type="button"
                                disabled={
                                  salvandoOperacao
                                }
                                onClick={async (
                                  event,
                                ) => {
                                  const row =
                                    (
                                      event.currentTarget as HTMLElement
                                    ).closest(
                                      'tr',
                                    )

                                  const formData =
                                    new FormData()

                                  row
                                    ?.querySelectorAll<HTMLInputElement>(
                                      'input',
                                    )
                                    .forEach(
                                      (
                                        input,
                                      ) =>
                                        formData.set(
                                          input.name,
                                          input.value,
                                        ),
                                    )

                                  setSalvandoOperacao(
                                    true,
                                  )

                                  try {
                                    await onSaveOperation(
                                      {
                                        id:
                                          operacao.id,

                                        data:
                                          String(
                                            formData.get(
                                              'data',
                                            ),
                                          ),

                                        quantidade:
                                          Number(
                                            formData.get(
                                              'quantidade',
                                            ),
                                          ),

                                        precoUnitario:
                                          Number(
                                            formData.get(
                                              'precoUnitario',
                                            ),
                                          ),

                                        taxas:
                                          Number(
                                            formData.get(
                                              'taxas',
                                            ),
                                          ),
                                      },
                                    )

                                    setOperacaoEmEdicao(
                                      null,
                                    )
                                  } finally {
                                    setSalvandoOperacao(
                                      false,
                                    )
                                  }
                                }}
                              >
                                Salvar
                              </button>

                              <button
                                className="table-action"
                                type="button"
                                disabled={
                                  salvandoOperacao
                                }
                                onClick={() =>
                                  setOperacaoEmEdicao(
                                    null,
                                  )
                                }
                              >
                                Cancelar
                              </button>
                            </td>
                          </>
                        ) : (
                          <>
                            <td>
                              {formatarDataCurta(
                                operacao.data,
                              )}
                            </td>

                            <td>
                              <span className="ticker">
                                {
                                  operacao.ticker
                                }
                              </span>
                            </td>

                            <td>
                              {
                                operacao.tipoOperacao
                              }
                            </td>

                            <td className="align-right">
                              {formatarNumeroInteiro(
                                operacao.quantidade,
                              )}
                            </td>

                            <td className="align-right">
                              {formatarMoeda(
                                operacao.precoUnitario,
                              )}
                            </td>

                            <td className="align-right">
                              {formatarMoeda(
                                operacao.taxas,
                              )}
                            </td>

                            <td className="operation-actions">
                              <button
                                className="table-action"
                                type="button"
                                onClick={() => {
                                  setAdicionandoOperacao(
                                    false,
                                  )

                                  setOperacaoEmEdicao(
                                    operacao.id,
                                  )
                                }}
                              >
                                Editar
                              </button>

                              <button
                                className="table-action operation-delete-button"
                                type="button"
                                disabled={
                                  deletandoOperacao
                                }
                                onClick={() =>
                                  handleDeleteOperation(
                                    operacao.id,
                                  )
                                }
                              >
                                Deletar
                              </button>
                            </td>
                          </>
                        )}
                      </tr>
                    )
                  },
                )}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="empty-state operations-empty-state">
            <strong>
              {ativoSelecionado ===
              'TODOS'
                ? 'Nenhuma operação disponível'
                : `Nenhuma operação encontrada para ${ativoSelecionado}`}
            </strong>
          </div>
        )}
      </article>

      {adicionandoOperacao ? (
        <OperacaoModal
          form={
            formNovaOperacao
          }
          salvando={
            criandoOperacao
          }
          erro={
            erroOperacao
          }
          onChange={
            setFormNovaOperacao
          }
          onClose={
            cancelarNovaOperacao
          }
          onSubmit={
            handleAdicionarOperacao
          }
        />
      ) : null}
    </section>
  )
}