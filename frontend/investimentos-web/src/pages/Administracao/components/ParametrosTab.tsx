import {
  useState,
} from 'react'
import {
  atualizarClasseAtivo,
  atualizarTipoAtivoParametro,
  atualizarTipoOperacaoParametro,
  excluirClasseAtivo,
  excluirTipoAtivoParametro,
  excluirTipoOperacaoParametro,
} from '../../../api/administracaoApi'
import {
  Modal,
} from '../../../components/Modal'
import type {
  Administracao,
  ClasseAtivoAdministracao,
  TipoAtivoAdministracao,
  TipoOperacaoAdministracao,
} from '../../../types/administracao'

type AbaParametro =
  | 'classes'
  | 'tipos'
  | 'operacoes'

type ItemParametro =
  | ClasseAtivoAdministracao
  | TipoAtivoAdministracao
  | TipoOperacaoAdministracao

interface ParametrosTabProps {
  dados: Administracao
  recarregar: () => Promise<void>
}

export function ParametrosTab({
  dados,
  recarregar,
}: ParametrosTabProps) {
  const [abaParametro, setAbaParametro] =
    useState<AbaParametro>('classes')
  const [editando, setEditando] =
    useState<ItemParametro | null>(null)
  const [codigo, setCodigo] = useState('')
  const [nome, setNome] = useState('')
  const [ativo, setAtivo] = useState(true)
  const [classeAtivoId, setClasseAtivoId] =
    useState('')
  const [salvando, setSalvando] =
    useState(false)
  const [erro, setErro] =
    useState<string | null>(null)

  function abrirEdicao(
    item: ItemParametro,
  ) {
    setEditando(item)
    setCodigo(item.codigo)
    setNome(item.nome)
    setAtivo(item.ativo)
    setClasseAtivoId(
      'classeAtivoId' in item
        ? String(item.classeAtivoId)
        : '',
    )
    setErro(null)
  }

  async function salvar() {
    if (!editando) return

    if (
      !codigo.trim() ||
      !nome.trim()
    ) {
      setErro(
        'Informe código e nome.',
      )
      return
    }

    try {
      setSalvando(true)
      setErro(null)

      if (abaParametro === 'classes') {
        await atualizarClasseAtivo(
          editando.id,
          {
            codigo,
            nome,
            ativo,
          },
        )
      } else if (
        abaParametro === 'tipos'
      ) {
        const classeId =
          Number(classeAtivoId)

        if (!classeId) {
          setErro(
            'Informe a classe do ativo.',
          )
          return
        }

        await atualizarTipoAtivoParametro(
          editando.id,
          {
            codigo,
            nome,
            ativo,
            classeAtivoId:
              classeId,
          },
        )
      } else {
        await atualizarTipoOperacaoParametro(
          editando.id,
          {
            codigo,
            nome,
            ativo,
          },
        )
      }

      setEditando(null)
      await recarregar()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível salvar o parâmetro.',
      )
    } finally {
      setSalvando(false)
    }
  }

  async function excluir(
    item: ItemParametro,
  ) {
    if (
      !window.confirm(
        `Deseja excluir ${item.codigo}?`,
      )
    ) {
      return
    }

    try {
      setErro(null)

      if (abaParametro === 'classes') {
        await excluirClasseAtivo(
          item.id,
        )
      } else if (
        abaParametro === 'tipos'
      ) {
        await excluirTipoAtivoParametro(
          item.id,
        )
      } else {
        await excluirTipoOperacaoParametro(
          item.id,
        )
      }

      await recarregar()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível excluir o parâmetro.',
      )
    }
  }

  function acoes(
    item: ItemParametro,
  ) {
    return (
      <div className="admin-row-actions">
        <button
          type="button"
          className="admin-action"
          onClick={() =>
            abrirEdicao(item)
          }
        >
          Editar
        </button>

        <button
          type="button"
          className="admin-action admin-action-danger"
          onClick={() =>
            void excluir(item)
          }
        >
          Excluir
        </button>
      </div>
    )
  }

  return (
    <>
      <article className="panel admin-panel">
        <div className="admin-subtabs">
          <button
            type="button"
            className={
              abaParametro === 'classes'
                ? 'active'
                : ''
            }
            onClick={() =>
              setAbaParametro('classes')
            }
          >
            Classes de Ativo
          </button>

          <button
            type="button"
            className={
              abaParametro === 'tipos'
                ? 'active'
                : ''
            }
            onClick={() =>
              setAbaParametro('tipos')
            }
          >
            Tipos de Ativo
          </button>

          <button
            type="button"
            className={
              abaParametro === 'operacoes'
                ? 'active'
                : ''
            }
            onClick={() =>
              setAbaParametro('operacoes')
            }
          >
            Tipos de Operação
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
              {abaParametro === 'tipos' ? (
                <tr>
                  <th>Código</th>
                  <th>Nome</th>
                  <th>Classe</th>
                  <th>Status</th>
                  <th>Ações</th>
                </tr>
              ) : (
                <tr>
                  <th>Código</th>
                  <th>Nome</th>
                  <th>Status</th>
                  <th>Ações</th>
                </tr>
              )}
            </thead>

            <tbody>
              {abaParametro === 'classes'
                ? dados.classesAtivo.map(
                    (item) => (
                      <tr key={item.id}>
                        <td>
                          <span className="ticker">
                            {item.codigo}
                          </span>
                        </td>
                        <td>{item.nome}</td>
                        <td>
                          {item.ativo
                            ? 'Ativo'
                            : 'Inativo'}
                        </td>
                        <td>{acoes(item)}</td>
                      </tr>
                    ),
                  )
                : null}

              {abaParametro === 'tipos'
                ? dados.tiposAtivo.map(
                    (item) => (
                      <tr key={item.id}>
                        <td>
                          <span className="ticker">
                            {item.codigo}
                          </span>
                        </td>
                        <td>{item.nome}</td>
                        <td>
                          {item.classeAtivoNome}
                        </td>
                        <td>
                          {item.ativo
                            ? 'Ativo'
                            : 'Inativo'}
                        </td>
                        <td>{acoes(item)}</td>
                      </tr>
                    ),
                  )
                : null}

              {abaParametro === 'operacoes'
                ? dados.tiposOperacao.map(
                    (item) => (
                      <tr key={item.id}>
                        <td>
                          <span className="ticker">
                            {item.codigo}
                          </span>
                        </td>
                        <td>{item.nome}</td>
                        <td>
                          {item.ativo
                            ? 'Ativo'
                            : 'Inativo'}
                        </td>
                        <td>{acoes(item)}</td>
                      </tr>
                    ),
                  )
                : null}
            </tbody>
          </table>
        </div>
      </article>

      {editando ? (
        <Modal
          title="Editar Parâmetro"
          subtitle={
            abaParametro === 'classes'
              ? 'Classe de Ativo'
              : abaParametro === 'tipos'
                ? 'Tipo de Ativo'
                : 'Tipo de Operação'
          }
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
                onClick={() =>
                  void salvar()
                }
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
              <span>Código</span>
              <input
                value={codigo}
                onChange={(event) =>
                  setCodigo(
                    event.target.value,
                  )
                }
              />
            </label>

            <label>
              <span>Nome</span>
              <input
                value={nome}
                onChange={(event) =>
                  setNome(
                    event.target.value,
                  )
                }
              />
            </label>

            {abaParametro === 'tipos' ? (
              <label>
                <span>Classe</span>
                <select
                  value={classeAtivoId}
                  onChange={(event) =>
                    setClasseAtivoId(
                      event.target.value,
                    )
                  }
                >
                  {dados.classesAtivo.map(
                    (classe) => (
                      <option
                        key={classe.id}
                        value={classe.id}
                      >
                        {classe.nome}
                      </option>
                    ),
                  )}
                </select>
              </label>
            ) : null}

            <label>
              <span>Status</span>
              <select
                value={
                  ativo
                    ? 'ATIVO'
                    : 'INATIVO'
                }
                onChange={(event) =>
                  setAtivo(
                    event.target.value ===
                      'ATIVO',
                  )
                }
              >
                <option value="ATIVO">
                  Ativo
                </option>
                <option value="INATIVO">
                  Inativo
                </option>
              </select>
            </label>
          </div>
        </Modal>
      ) : null}
    </>
  )
}
