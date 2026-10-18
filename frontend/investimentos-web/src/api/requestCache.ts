// Cache somente em memória e por sessão; evita duplicação de requisições concorrentes.
const cache = new Map<string, { expires: number; promise: Promise<unknown> }>()
export function cachedRequest<T>(key: string, load: () => Promise<T>, ttlMs = 15000): Promise<T> {
  const current = cache.get(key)
  if (current && current.expires > Date.now()) return current.promise as Promise<T>
  const promise = load().catch(error => { if (cache.get(key)?.promise === promise) cache.delete(key); throw error })
  cache.set(key, { expires: Date.now() + ttlMs, promise })
  return promise
}
export function invalidateRequests() { cache.clear() }
