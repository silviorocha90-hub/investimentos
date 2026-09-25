import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import type { Investidor } from '../../../types/investidor'
import {
  atualizarMovimentacaoFinanceira,
  criarMovimentacaoFinanceira,
  excluirMovimentacaoFinanceira,
  listarMovimentacoesFinanceiras,
} from '../../../api/movimentacoesFinanceirasApi'
import type {
  MovimentacaoFinanceira,
  TipoMovimentacaoFinanceira,
} from '../../../api/movimentacoesFinanceirasApi'

interface Props {
  investidores: Investidor[]
  onDataChanged?: () => Promise<void>
}

const moeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

export function MovimentacoesFinanceirasTab({
  investidores,
  onDataChanged,
}: Props) {
  const hoje = new Date().toISOString().slice(0, 10)
  const ano = new Date().getFullYear()
  const [itens, setItens] = useState<MovimentacaoFinanceira[]>([])
  const [filtro, setFiltro] = useState('TODOS')
  const [editando, setEditando] = useState<MovimentacaoFinanceira | null>(null)
  const [modal, setModal] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const [salvando, setSalvando] = useState(false)
  const [form, setForm] = useState({
    investidorId: '',
    data: hoje,
    tipo: 'APORTE' as TipoMovimentacaoFinanceira,
    valor: '',
    descricao: '',
  })

  async function carregar() {
    try {
      setErro(null)
      setItens(await listarMovimentacoesFinanceiras(ano))
    } catch (e) {
      setErro(e instanceof Error ? e.message : 'Não foi possível carregar as movimentações.')
    }
  }

  useEffect(() => { void carregar() }, [])

  const exibidos = useMemo(
    () => filtro === 'TODOS'
      ? itens
      : itens.filter((x) => x.investidorId === filtro),
    [itens, filtro],
  )

  const totais = useMemo(() => ({
    entradas: exibidos.filter((x) => x.tipo === 'APORTE').reduce((s, x) => s + x.valor, 0),
    saidas: exibidos.filter((x) => x.tipo === 'RETIRADA').reduce((s, x) => s + x.valor, 0),
  }), [exibidos])

  function novo() {
    setEditando(null)
    setForm({
      investidorId: filtro !== 'TODOS' ? filtro : investidores[0]?.id ?? '',
      data: hoje,
      tipo: 'APORTE',
      valor: '',
      descricao: '',
    })
    setErro(null)
    setModal(true)
  }

  function editar(item: MovimentacaoFinanceira) {
    setEditando(item)
    setForm({
      investidorId: item.investidorId,
      data: item.data.slice(0, 10),
      tipo: item.tipo,
      valor: item.valor.toLocaleString('pt-BR', { minimumFractionDigits: 2 }),
      descricao: item.descricao ?? '',
    })
    setErro(null)
    setModal(true)
  }

  async function salvar(event: FormEvent) {
    event.preventDefault()
    const valor = Number(form.valor.replace(/\./g, '').replace(',', '.'))
    if (!form.investidorId || !form.data || !Number.isFinite(valor) || valor <= 0) {
      setErro('Preencha investidor, data e um valor maior que zero.')
      return
    }

    try {
      setSalvando(true)
      setErro(null)
      if (editando) {
        await atualizarMovimentacaoFinanceira(editando.id, {
          investidorId: form.investidorId,
          data: form.data,
          tipo: form.tipo,
          valor,
          descricao: form.descricao.trim() || null,
        })
      } else {
        await criarMovimentacaoFinanceira({
          investidorId: form.investidorId,
          data: form.data,
          tipo: form.tipo,
          valor,
          descricao: form.descricao.trim() || null,
        })
      }
      setModal(false)
      await carregar()
      await onDataChanged?.()
    } catch (e) {
      setErro(e instanceof Error ? e.message : 'Não foi possível salvar a movimentação.')
    } finally {
      setSalvando(false)
    }
  }

  async function excluir(item: MovimentacaoFinanceira) {
    if (!window.confirm(`Excluir ${item.tipo.toLowerCase()} de ${moeda.format(item.valor)}?`)) return
    try {
      setErro(null)
      await excluirMovimentacaoFinanceira(item.id)
      await carregar()
      await onDataChanged?.()
    } catch (e) {
      setErro(e instanceof Error ? e.message : 'Não foi possível excluir a movimentação.')
    }
  }

  return (
    <div className="card admin-panel">
      <div className="admin-toolbar admin-assets-toolbar">
        <div>
          <strong>Movimentações financeiras — {ano}</strong>
          <span className="admin-movement-summary">
            Entradas: {moeda.format(totais.entradas)} · Saídas: {moeda.format(totais.saidas)}
          </span>
        </div>
        <button className="admin-primary-button" type="button" onClick={novo}>
          Nova movimentação
        </button>
      </div>

      <div className="admin-asset-filters admin-movement-filters">
        <label>
          <span>Investidor</span>
          <select value={filtro} onChange={(e) => setFiltro(e.target.value)}>
            <option value="TODOS">TODOS</option>
            {investidores.map((x) => <option key={x.id} value={x.id}>{x.nome}</option>)}
          </select>
        </label>
      </div>

      {erro ? <div className="admin-inline-error">{erro}</div> : null}

      <div className="admin-table-wrap">
        <table className="admin-table admin-movement-table">
          <thead><tr><th>Data</th><th>Investidor</th><th>Tipo</th><th>Valor</th><th>Descrição</th><th>Ações</th></tr></thead>
          <tbody>
            {exibidos.map((item) => (
              <tr key={item.id}>
                <td>{new Date(`${item.data.slice(0, 10)}T12:00:00`).toLocaleDateString('pt-BR')}</td>
                <td>{item.investidor}</td>
                <td><strong className={item.tipo === 'APORTE' ? 'movement-entry' : 'movement-exit'}>{item.tipo}</strong></td>
                <td>{moeda.format(item.valor)}</td>
                <td>{item.descricao || '—'}</td>
                <td>
                  <div className="admin-row-actions">
                    <button className="admin-action admin-movement-edit" type="button" onClick={() => editar(item)}>Editar</button>
                    <button className="admin-action admin-action-danger admin-movement-delete" type="button" onClick={() => void excluir(item)}>Excluir</button>
                  </div>
                </td>
              </tr>
            ))}
            {exibidos.length === 0 ? <tr><td colSpan={6} className="admin-user-empty">Nenhuma movimentação em {ano}.</td></tr> : null}
          </tbody>
        </table>
      </div>

      {modal ? (
        <div className="admin-modal-backdrop">
          <form className="admin-modal" onSubmit={(e) => void salvar(e)}>
            <div className="admin-modal-header">
              <div><span>Movimentações financeiras</span><strong>{editando ? 'Editar movimentação' : 'Nova movimentação'}</strong></div>
              <button type="button" onClick={() => setModal(false)}>×</button>
            </div>
            <div className="admin-modal-grid">
              <label><span>Investidor</span><select value={form.investidorId} onChange={(e) => setForm({ ...form, investidorId: e.target.value })}>{investidores.map((x) => <option key={x.id} value={x.id}>{x.nome}</option>)}</select></label>
              <label><span>Data</span><input type="date" value={form.data} onChange={(e) => setForm({ ...form, data: e.target.value })} /></label>
              <label><span>Tipo</span><select value={form.tipo} onChange={(e) => setForm({ ...form, tipo: e.target.value as TipoMovimentacaoFinanceira })}><option value="APORTE">APORTE</option><option value="RETIRADA">RETIRADA</option></select></label>
              <label><span>Valor</span><input inputMode="decimal" value={form.valor} onChange={(e) => setForm({ ...form, valor: e.target.value })} placeholder="0,00" /></label>
              <label className="admin-movement-description"><span>Descrição</span><input value={form.descricao} onChange={(e) => setForm({ ...form, descricao: e.target.value })} maxLength={200} /></label>
            </div>
            {erro ? <div className="admin-inline-error">{erro}</div> : null}
            <div className="admin-modal-actions">
              <button className="admin-clear-button" type="button" disabled={salvando} onClick={() => setModal(false)}>Cancelar</button>
              <button className="admin-primary-button" type="submit" disabled={salvando}>{salvando ? 'Salvando...' : 'Salvar'}</button>
            </div>
          </form>
        </div>
      ) : null}
    </div>
  )
}
