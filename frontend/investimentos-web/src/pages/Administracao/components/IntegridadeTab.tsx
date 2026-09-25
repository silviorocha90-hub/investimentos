import {
  useEffect,
  useState,
} from 'react'

import {
  verificarIntegridade,
} from '../../../api/administracaoApi'

import type {
  RelatorioIntegridade,
} from '../../../api/administracaoApi'

export function IntegridadeTab() {
  const [relatorio, setRelatorio] =
    useState<RelatorioIntegridade | null>(null)
  const [carregando, setCarregando] =
    useState(true)
  const [erro, setErro] =
    useState<string | null>(null)

  async function carregar() {
    try {
      setCarregando(true)
      setErro(null)
      setRelatorio(
        await verificarIntegridade(),
      )
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível verificar a integridade.',
      )
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    void carregar()
  }, [])

  return (
    <article className="panel admin-panel">
      <div className="admin-toolbar">
        <div>
          <strong>Integridade dos dados</strong>
          <p>
            Reconciliação automática das principais regras
            estruturais da carteira.
          </p>
        </div>

        <button
          type="button"
          className="admin-primary-button"
          disabled={carregando}
          onClick={() => void carregar()}
        >
          {carregando ? 'Verificando...' : 'Verificar novamente'}
        </button>
      </div>

      {erro ? (
        <div className="admin-inline-error">
          {erro}
        </div>
      ) : null}

      {relatorio ? (
        <>
          <div className="admin-summary-grid">
            <div>
              <span>Status</span>
              <strong>
                {relatorio.integro
                  ? 'Íntegro'
                  : 'Requer atenção'}
              </strong>
            </div>

            <div>
              <span>Ocorrências</span>
              <strong>
                {relatorio.totalProblemas}
              </strong>
            </div>
          </div>

          <div className="admin-table-wrap">
            <table className="data-table admin-table">
              <thead>
                <tr>
                  <th>Severidade</th>
                  <th>Entidade</th>
                  <th>Referência</th>
                  <th>Código</th>
                  <th>Descrição</th>
                </tr>
              </thead>
              <tbody>
                {relatorio.problemas.map(
                  (problema, indice) => (
                    <tr
                      key={`${problema.codigo}-${problema.referencia}-${indice}`}
                    >
                      <td>
                        <strong>
                          {problema.severidade}
                        </strong>
                      </td>
                      <td>{problema.entidade}</td>
                      <td>{problema.referencia}</td>
                      <td>{problema.codigo}</td>
                      <td>{problema.mensagem}</td>
                    </tr>
                  ),
                )}

                {relatorio.problemas.length === 0 ? (
                  <tr>
                    <td colSpan={5}>
                      Nenhuma inconsistência encontrada.
                    </td>
                  </tr>
                ) : null}
              </tbody>
            </table>
          </div>
        </>
      ) : null}
    </article>
  )
}
