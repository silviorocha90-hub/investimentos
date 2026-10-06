import {
  useEffect,
  useState,
} from 'react'

import {
  gerarRelatorioIa,
  listarRelatoriosIa,
  type RelatorioIa,
} from '../../api/assistenteIaApi'

import './AssistenteIa.css'

export function AssistenteIaView() {
  const [
    relatorios,
    setRelatorios,
  ] = useState<RelatorioIa[]>([])

  const [
    carregando,
    setCarregando,
  ] = useState(true)

  const [
    gerando,
    setGerando,
  ] = useState(false)

  const [
    erro,
    setErro,
  ] = useState<string | null>(null)

  async function carregar() {
    try {
      setErro(null)
      setRelatorios(
        await listarRelatoriosIa(),
      )
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Falha ao carregar relatórios.',
      )
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    void carregar()
  }, [])

  async function gerarAgora() {
    try {
      setGerando(true)
      setErro(null)
      await gerarRelatorioIa()
      await carregar()
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Falha ao gerar análise.',
      )
    } finally {
      setGerando(false)
    }
  }

  const atual =
    relatorios[0] ?? null

  return (
    <section className="assistant-page">
      <header className="assistant-header">
        <div>
          <span className="assistant-eyebrow">
            Monitoramento autônomo
          </span>
          <h1>Assistente IA</h1>
          <p>
            Notícias, eventos e opções analisados
            no contexto da sua carteira.
          </p>
        </div>

        <button
          type="button"
          className="assistant-generate"
          disabled={gerando}
          onClick={() =>
            void gerarAgora()
          }
        >
          {gerando
            ? 'Analisando...'
            : 'Gerar análise agora'}
        </button>
      </header>

      {erro ? (
        <div className="assistant-error">
          {erro}
        </div>
      ) : null}

      {carregando ? (
        <div className="assistant-empty">
          Carregando análises...
        </div>
      ) : atual ? (
        <>
          <article className="assistant-report">
            <div className="assistant-report-meta">
              <strong>
                Resumo de{' '}
                {new Date(
                  atual.dataReferencia,
                ).toLocaleDateString(
                  'pt-BR',
                  { timeZone: 'UTC' },
                )}
              </strong>
              <span>
                {atual.modelo}
              </span>
            </div>

            <div className="assistant-content">
              {atual.conteudo}
            </div>
          </article>

          {relatorios.length > 1 ? (
            <section className="assistant-history">
              <h2>Histórico</h2>
              {relatorios
                .slice(1)
                .map((relatorio) => (
                  <details
                    key={relatorio.id}
                  >
                    <summary>
                      {new Date(
                        relatorio.dataReferencia,
                      ).toLocaleDateString(
                        'pt-BR',
                        { timeZone: 'UTC' },
                      )}
                    </summary>
                    <div className="assistant-content">
                      {relatorio.conteudo}
                    </div>
                  </details>
                ))}
            </section>
          ) : null}
        </>
      ) : (
        <div className="assistant-empty">
          Nenhuma análise gerada ainda.
        </div>
      )}
    </section>
  )
}
