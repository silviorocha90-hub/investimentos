import { useEffect, useId, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import './ColumnFilter.css'

function normalize(value: string) {
  return value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase('pt-BR')
}

interface HeaderProps {
  label: string
  values: readonly string[]
  selected?: readonly string[]
  onChange: (values: readonly string[]) => void
}

export function ColumnFilterHeader({ label, values, selected, onChange }: HeaderProps) {
  const [open, setOpen] = useState(false)
  const [search, setSearch] = useState('')
  const [position, setPosition] = useState({ top: 0, left: 0, maxHeight: 400 })
  const trigger = useRef<HTMLButtonElement>(null)
  const panel = useRef<HTMLDivElement>(null)
  const searchInput = useRef<HTMLInputElement>(null)
  const id = useId()
  const active = !!selected?.length

  useEffect(() => {
    if (!open) return
    searchInput.current?.focus()
    function closeOutside(event: PointerEvent) {
      const target = event.target as Node
      if (!panel.current?.contains(target) && !trigger.current?.contains(target)) setOpen(false)
    }
    function escape(event: KeyboardEvent) {
      if (event.key === 'Escape') { setOpen(false); trigger.current?.focus() }
    }
    function closeOnScroll(event: Event) {
      if (!(event.target instanceof Node) || !panel.current?.contains(event.target)) setOpen(false)
    }
    document.addEventListener('pointerdown', closeOutside)
    document.addEventListener('keydown', escape)
    window.addEventListener('resize', closeOnScroll)
    window.addEventListener('scroll', closeOnScroll, true)
    return () => {
      document.removeEventListener('pointerdown', closeOutside)
      document.removeEventListener('keydown', escape)
      window.removeEventListener('resize', closeOnScroll)
      window.removeEventListener('scroll', closeOnScroll, true)
    }
  }, [open])

  function toggle() {
    if (open) { setOpen(false); return }
    const bounds = trigger.current?.getBoundingClientRect()
    if (!bounds) return
    const availableBelow = window.innerHeight - bounds.bottom - 16
    const height = Math.min(400, window.innerHeight - 24)
    setPosition({
      left: Math.max(8, Math.min(bounds.left, window.innerWidth - 296)),
      top: availableBelow >= Math.min(260, height) ? bounds.bottom + 6 : Math.max(8, bounds.top - height - 6),
      maxHeight: availableBelow >= Math.min(260, height) ? Math.min(height, availableBelow) : height,
    })
    setSearch('')
    setOpen(true)
  }

  function select(value: string) {
    onChange(selected?.includes(value) ? selected.filter(item => item !== value) : [...(selected ?? []), value])
  }

  const visible = values.filter(value => normalize(value).includes(normalize(search)))

  return <th scope="col" className="column-filter-heading">
    <button ref={trigger} type="button" className={`column-filter-trigger${active ? ' is-filtered' : ''}`}
      onClick={toggle} aria-label={`Filtrar por ${label}${active ? ' (filtro ativo)' : ''}`}
      aria-expanded={open} aria-controls={open ? id : undefined} aria-haspopup="dialog">
      <span>{label}</span>
      <svg viewBox="0 0 16 16" width="12" height="12" aria-hidden="true"><path d="M2 3h12L9 9v4l-2-1V9Z" fill="none" stroke="currentColor" strokeWidth="1.4" /></svg>
    </button>
    {open && createPortal(<div ref={panel} id={id} role="dialog" aria-label={`Filtro de ${label}`} className="column-filter-popover" style={position}
      onBlur={event => { if (event.relatedTarget && !event.currentTarget.contains(event.relatedTarget as Node) && event.relatedTarget !== trigger.current) setOpen(false) }}>
      <div className="column-filter-title"><strong>{label}</strong><button type="button" aria-label="Fechar filtro" onClick={() => { setOpen(false); trigger.current?.focus() }}>×</button></div>
      <input ref={searchInput} type="search" value={search} onChange={event => setSearch(event.target.value)} placeholder="Buscar valor…" aria-label={`Buscar valores de ${label}`} />
      <small>Selecione um ou mais valores</small>
      <button type="button" className="column-filter-all" aria-pressed={!active} onClick={() => onChange([])}>Todos os valores / limpar</button>
      <div className="column-filter-values">
        {visible.map(value => <button key={value} type="button" className="column-filter-value" aria-pressed={selected?.includes(value) ?? false} onClick={() => select(value)}>
          <span aria-hidden="true">{selected?.includes(value) ? '☑' : '☐'}</span><span>{value}</span>
        </button>)}
        {!visible.length && <p>Nenhum valor encontrado.</p>}
      </div>
      <button type="button" className="column-filter-done" onClick={() => { setOpen(false); trigger.current?.focus() }}>Concluir</button>
    </div>, document.body)}
  </th>
}

export function TableFilterSummary({ shown, total, active, onClear }: { shown: number; total: number; active: number; onClear: () => void }) {
  return active > 0 ? <div className="table-filter-summary">
    <span role="status">{shown} de {total} registros · {active} {active === 1 ? 'coluna filtrada' : 'colunas filtradas'}</span>
    <button type="button" onClick={onClear}>Limpar filtros das colunas</button>
  </div> : null
}
