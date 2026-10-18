import { useEffect, useState } from 'react'

// Acompanha apenas chamadas HTTP da API, sem interferir no conteúdo das respostas.
let requisicoesAtivas = 0
let instalado = false
const EVENTO = 'aportiva:requisicoes'

function notificar() {
  window.dispatchEvent(new CustomEvent(EVENTO, { detail: requisicoesAtivas }))
}

export function instalarIndicadorRequisicoes() {
  if (instalado || typeof window === 'undefined') return
  instalado = true
  const fetchOriginal = window.fetch.bind(window)
  window.fetch = async (input: RequestInfo | URL, init?: RequestInit): Promise<Response> => {
    const url = typeof input === 'string' ? input : input instanceof URL ? input.href : input.url
    // Ignora recursos estáticos e consultas de status/polling em segundo plano.
    const monitorar = /\/api\//i.test(url) &&
      !/\/api\/assistente-ia\/(?:status|geracao|relatorios\/status)/i.test(url)
    if (!monitorar) return fetchOriginal(input, init)
    requisicoesAtivas += 1
    notificar()
    try {
      return await fetchOriginal(input, init)
    } finally {
      requisicoesAtivas = Math.max(0, requisicoesAtivas - 1)
      notificar()
    }
  }
}

export function IndicadorCarregamento() {
  const [ativas, setAtivas] = useState(requisicoesAtivas)
  const [visivel, setVisivel] = useState(false)
  const [segundos, setSegundos] = useState(0)

  useEffect(() => {
    const atualizar = (event: Event) => {
      setAtivas((event as CustomEvent<number>).detail)
    }
    window.addEventListener(EVENTO, atualizar)
    return () => window.removeEventListener(EVENTO, atualizar)
  }, [])

  useEffect(() => {
    if (ativas === 0) {
      setVisivel(false)
      setSegundos(0)
      return
    }
    // Não pisca durante requisições rápidas.
    const inicio = Date.now()
    const atraso = window.setTimeout(() => setVisivel(true), 350)
    const intervalo = window.setInterval(() => setSegundos(Math.floor((Date.now() - inicio) / 1000)), 1000)
    return () => { window.clearTimeout(atraso); window.clearInterval(intervalo) }
  }, [ativas > 0])

  if (!visivel || ativas === 0) return null
  return (
    <div className="aportiva-loading-status" role="status" aria-live="polite">
      <span className="aportiva-loading-spinner" aria-hidden="true" />
      <span>Carregando... <strong>{segundos}s</strong></span>
    </div>
  )
}
