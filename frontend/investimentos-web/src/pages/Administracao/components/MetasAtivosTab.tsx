import {
  useEffect,
  useMemo,
  useState,
} from 'react'
import type {
  Administracao,
} from '../../../types/administracao'
import {
  excluirMetaAtivo,
  listarMetasAtivos,
  salvarMetaAtivo,
} from '../../../api/metasApi'
import type {
  MetaAtivo,
} from '../../../api/metasApi'

interface Props {
  dados: Administracao
}

const numero =
  new Intl.NumberFormat('pt-BR', {
    maximumFractionDigits: 2,
  })

const percentual =
  new Intl.NumberFormat('pt-BR', {
    minimumFractionDigits: 1,
    maximumFractionDigits: 1,
  })

export function MetasAtivosTab({
  dados,
}: Props) {
  const [metas, setMetas] =
    useState<MetaAtivo[]>([])
  const [investidorId, setInvestidorId] =
    useState('')
  const [ativoId, setAtivoId] =
    useState('')
  const [quantidade, setQuantidade] =
    useState('1000')
  const [salvando, setSalvando] =
    useState(false)
  const [erro, setErro] =
    useState<string | null>(null)

  async function carregar() {
    try {
      setErro(null)
      setMetas(
        await listarMetasAtivos(),
      )
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível carregar as metas.',
      )
    }
  }

  useEffect(() => {
    void carregar()
  }, [])

  useEffect(() => {
    if (
      !investidorId &&
      dados.investidores.length > 0
    ) {
      setInvestidorId(
        dados.investidores[0].id,
      )
    }
  }, [
    dados.investidores,
    investidorId,
  ])

  const ativosElegiveis =
    useMemo(
      () =>
        dados.ativos
          .filter(
            (ativo) =>
              ativo.tipoAtivoCodigo ===
                'ACAO' ||
              ativo.tipoAtivoCodigo ===
                'FII',
          )
          .slice()
          .sort(
            (a, b) =>
              a.ticker.localeCompare(
                b.ticker,
                'pt-BR',
              ),
          ),
      [dados.ativos],
    )

  useEffect(() => {
    if (
      !ativoId &&
      ativosElegiveis.length > 0
    ) {
      setAtivoId(
        ativosElegiveis[0].id,
      )
    }
  }, [
    ativoId,
    ativosElegiveis,
  ])

  async function salvar() {
    const valor =
      Number(
        quantidade
          .replace(/./g, '')
          .replace(',', '.'),
      )

    if (
      !investidorId ||
      !ativoId ||
      !Number.isFinite(valor) ||
      valor <= 0
    ) {
      setErro(
        'Informe investidor, ativo e uma quantidade desejada maior que zero.',
      )
      return
    }

    try {
      setSalvando(true)
      setErro(null)
      await salvarMetaAtivo(
        investidorId,
        ativoId,
        valor,
      )
      await carregar()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível salvar a meta.',
      )
    } finally {
      setSalvando(false)
    }
  }

  async function excluir(
    meta: MetaAtivo,
  ) {
    if (
      !window.confirm(
        `Excluir a meta de ${meta.ticker} para ${meta.investidor}?`,
      )
    ) {
      return
    }

    try {
      setSalvando(true)
      setErro(null)
      await excluirMetaAtivo(
        meta.id,
      )
      await carregar()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível excluir a meta.',
      )
    } finally {
      setSalvando(false)
    }
  }

  return (
    <article className="panel admin-panel">
      <div className="admin-toolbar">
        <div>
          <strong>Metas por Ativo</strong>
          <p>
            Quantidade-alvo configurável por investidor e ativo.
          </p>
        </div>
      </div>

      <div className="admin-asset-filters">
        <label>
          <span>Investidor</span>
          <select
            value={investidorId}
            onChange={(event) =>
              setInvestidorId(
                event.target.value,
              )
            }
          >
            {dados.investidores.map(
              (investidor) => (
                <option
                  key={investidor.id}
                  value={investidor.id}
                >
                  {investidor.nome}
                </option>
              ),
            )}
          </select>
        </label>

        <label>
          <span>Ativo</span>
          <select
            value={ativoId}
            onChange={(event) =>
              setAtivoId(
                event.target.value,
              )
            }
          >
            {ativosElegiveis.map(
              (ativo) => (
                <option
                  key={ativo.id}
                  value={ativo.id}
                >
                  {ativo.ticker}
                </option>
              ),
            )}
          </select>
        </label>

        <label>
          <span>Quantidade desejada</span>
          <input
            type="number"
            min="1"
            step="1"
            value={quantidade}
            onChange={(event) =>
              setQuantidade(
                event.target.value,
              )
            }
          />
        </label>

        <button
          type="button"
          className="admin-primary-button"
          disabled={salvando}
          onClick={() =>
            void salvar()
          }
        >
          {salvando
            ? 'Salvando...'
            : 'Salvar Meta'}
        </button>
      </div>

      {erro ? (
        <div className="admin-inline-error">
          {erro}
        </div>
      ) : null}

      <div className="admin-table-wrap">
        <table className="data-table admin-table">
          <thead>
            <tr>
              <th>Investidor</th>
              <th>Ativo</th>
              <th>Atual</th>
              <th>Meta</th>
              <th>Faltam</th>
              <th>Progresso</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            {metas.map((meta) => {
              const progresso =
                Math.max(
                  0,
                  Math.min(
                    meta.percentualAtingido,
                    100,
                  ),
                )

              return (
                <tr key={meta.id}>
                  <td>{meta.investidor}</td>
                  <td>
                    <span className="ticker">
                      {meta.ticker}
                    </span>
                  </td>
                  <td>
                    {numero.format(
                      meta.quantidadeAtual,
                    )}
                  </td>
                  <td>
                    {numero.format(
                      meta.quantidadeDesejada,
                    )}
                  </td>
                  <td>
                    {numero.format(
                      meta.quantidadeFaltante,
                    )}
                  </td>
                  <td>
                    <div className="admin-goal-progress">
                      <div>
                        <i
                          style={{
                            width: `${progresso}%`,
                          }}
                        />
                      </div>
                      <strong>
                        {percentual.format(
                          meta.percentualAtingido,
                        )}%
                      </strong>
                    </div>
                  </td>
                  <td>
                    <div className="admin-row-actions">
                      <button
                        type="button"
                        className="admin-action"
                        onClick={() => {
                          setInvestidorId(
                            meta.investidorId,
                          )
                          setAtivoId(
                            meta.ativoId,
                          )
                          setQuantidade(
                            String(
                              meta.quantidadeDesejada,
                            ),
                          )
                        }}
                      >
                        Editar
                      </button>
                      <button
                        type="button"
                        className="admin-action admin-action-danger"
                        disabled={salvando}
                        onClick={() =>
                          void excluir(
                            meta,
                          )
                        }
                      >
                        Excluir
                      </button>
                    </div>
                  </td>
                </tr>
              )
            })}

            {metas.length === 0 ? (
              <tr>
                <td colSpan={7}>
                  Nenhuma meta cadastrada.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </article>
  )
}
