import {
  useEffect,
  useMemo,
  useState,
} from 'react'

import {
  gerarRelatorioIa,
  listarRelatoriosIa,
  type RelatorioIa,
} from '../../api/assistenteIaApi'

import { RelatorioFormatado } from './RelatorioFormatado'
import './AssistenteIa.css'

interface AssistenteIaViewProps {
  podeGerar?: boolean
}

function dataRelatorio(data: string) {
  return new Date(data).toLocaleDateString(
    'pt-BR',
    {
      timeZone: 'UTC',
      day: '2-digit',
      month: 'long',
      year: 'numeric',
    },
  )
}

function dataCurta(data: string) {
  return new Date(data).toLocaleDateString(
    'pt-BR',
    {
      timeZone: 'UTC',
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    },
  )
}

function horaGeracao(data: string) {
  return new Date(data).toLocaleString(
    'pt-BR',
    {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    },
  )
}

export function AssistenteIaView({
  podeGerar = false,
}: AssistenteIaViewProps) {
  const [relatorios, setRelatorios] =
    useState<RelatorioIa[]>([])
  const [selecionadoId, setSelecionadoId] =
    useState<string | null>(null)
  const [carregando, setCarregando] =
    useState(true)
  const [gerando, setGerando] =
    useState(false)
  const [erro, setErro] =
    useState<string | null>(null)

  async function carregar() {
    try {
      setErro(null)
      const dados =
        await listarRelatoriosIa()
      setRelatorios(dados)
      setSelecionadoId((atual) =>
        atual &&
        dados.some(
          (item) => item.id === atual,
        )
          ? atual
          : dados[0]?.id ?? null,
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
      const novo =
        await gerarRelatorioIa()
      await carregar()
      setSelecionadoId(novo.id)
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

  const selecionado = useMemo(
    () =>
      relatorios.find(
        (item) =>
          item.id === selecionadoId,
      ) ??
      relatorios[0] ??
      null,
    [relatorios, selecionadoId],
  )

  return (
    <section className="assistant-page">
      <header className="assistant-header">
        <div>
          <span className="assistant-eyebrow">
            Monitoramento autônomo
          </span>
          <h1>Assistente IA</h1>
          <p>
            {podeGerar
              ? 'Visão consolidada de todas as carteiras, notícias, eventos e opções.'
              : 'Análises, notícias, eventos e opções somente da sua carteira.'}
          </p>
        </div>

        {podeGerar ? (
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
        ) : null}
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
      ) : selecionado ? (
        <div className="assistant-workspace">
          <aside className="assistant-days">
            <div className="assistant-days-header">
              <div>
                <span>Análises geradas</span>
                <strong>
                  {relatorios.length}
                </strong>
              </div>
              <small>
                Selecione um dia para consultar
              </small>
            </div>

            <div className="assistant-day-list">
              {relatorios.map(
                (relatorio, indice) => {
                  const ativo =
                    relatorio.id ===
                    selecionado.id

                  return (
                    <button
                      type="button"
                      key={relatorio.id}
                      className={
                        ativo
                          ? 'assistant-day active'
                          : 'assistant-day'
                      }
                      onClick={() =>
                        setSelecionadoId(
                          relatorio.id,
                        )
                      }
                    >
                      <span className="assistant-day-date">
                        {dataCurta(
                          relatorio.dataReferencia,
                        )}
                      </span>
                      <span className="assistant-day-info">
                        <strong>
                          {indice === 0
                            ? 'Mais recente'
                            : 'Análise diária'}
                        </strong>
                        <small>
                          Gerada em{' '}
                          {horaGeracao(
                            relatorio.dataGeracao,
                          )}
                        </small>
                      </span>
                      <span className="assistant-day-arrow">
                        ›
                      </span>
                    </button>
                  )
                },
              )}
            </div>
          </aside>

          <article className="assistant-report">
            <div className="assistant-report-top">
              <div>
                <span className="assistant-report-label">
                  {podeGerar
                    ? 'Análise consolidada'
                    : 'Análise da sua carteira'}
                </span>
                <h2>
                  {dataRelatorio(
                    selecionado.dataReferencia,
                  )}
                </h2>
                <p>
                  {podeGerar
                    ? 'Este relatório considera o conjunto completo das carteiras administradas.'
                    : 'Este relatório considera exclusivamente os investimentos vinculados ao seu acesso.'}
                </p>
              </div>

              <div className="assistant-report-status">
                <span>✓ Gerada</span>
                <small>
                  {selecionado.modelo}
                </small>
              </div>
            </div>

            <div className="assistant-content">
              <RelatorioFormatado conteudo={selecionado.conteudo} />
            </div>
          </article>
        </div>
      ) : (
        <div className="assistant-empty">
          <strong>
            Nenhuma análise gerada ainda.
          </strong>
          <span>
            Quando houver uma análise,
            ela aparecerá aqui organizada
            pela data de referência.
          </span>
        </div>
      )}
    </section>
  )
}
