import { useEffect, useMemo, useState } from 'react'
import {
  atualizarDescontoFiscal,
  criarDescontoFiscal,
  excluirDescontoFiscal,
} from '../../../api/administracaoApi'
import {
  obterFiscal,
  type FiscalResumo,
} from '../../../api/fiscalApi'
import { Modal } from '../../../components/Modal'
import type {
  Administracao,
  DescontoFiscalAdministracao,
} from '../../../types/administracao'
import {
  formatarDataCurta,
  formatarMoeda,
} from '../../../utils/formatters'

interface CarteiraTabProps {
  dados: Administracao
  recarregar?: () => Promise<void>
}

export function CarteiraTab({ dados, recarregar }: CarteiraTabProps) {
  const [editando, setEditando] = useState<DescontoFiscalAdministracao | null>(null)
  const [novo, setNovo] = useState(false)
  const [investidorId, setInvestidorId] = useState('TODOS')
  const [ano, setAno] = useState(new Date().getFullYear())
  const [dataPagamento, setDataPagamento] = useState('')
  const [valor, setValor] = useState('')
  const [descricao, setDescricao] = useState('')
  const [investidorModalId, setInvestidorModalId] = useState('')
  const [salvando, setSalvando] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const [fiscal, setFiscal] = useState<FiscalResumo | null>(null)

  async function carregarFiscal() {
    try {
      setFiscal(await obterFiscal(
        investidorId === 'TODOS' ? undefined : investidorId,
        ano,
      ))
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Não foi possível carregar a apuração fiscal.')
    }
  }

  useEffect(() => {
    void carregarFiscal()
  }, [investidorId, ano, dados.descontosFiscais])

  const descontosFiltrados = useMemo(
    () => dados.descontosFiscais.filter(
      (x) => investidorId === 'TODOS' || x.investidorId === investidorId,
    ),
    [dados.descontosFiscais, investidorId],
  )

  const competenciasMensais = useMemo(() => {
    const grupos = new Map<string, {
      ano: number
      mes: number
      irEstimado: number
      darfPago: number
      diferenca: number
      investidores: NonNullable<FiscalResumo['meses']>
    }>()

    for (const item of fiscal?.meses ?? []) {
      const chave = `${item.ano}-${String(item.mes).padStart(2, '0')}`
      const grupo = grupos.get(chave) ?? {
        ano: item.ano,
        mes: item.mes,
        irEstimado: 0,
        darfPago: 0,
        diferenca: 0,
        investidores: [],
      }

      grupo.irEstimado += item.irEstimadoOpcoes
      grupo.darfPago += item.darfPago
      grupo.diferenca += item.irEstimadoOpcoes - item.darfPago
      grupo.investidores.push(item)
      grupos.set(chave, grupo)
    }

    return [...grupos.values()]
      .sort((a, b) => b.ano - a.ano || b.mes - a.mes)
      .map((grupo) => ({
        ...grupo,
        investidores: [...grupo.investidores]
          .sort((a, b) => b.irEstimadoOpcoes - a.irEstimadoOpcoes),
      }))
  }, [fiscal?.meses])

  function abrirNovo() {
    setNovo(true)
    setEditando(null)
    setDataPagamento(new Date().toISOString().slice(0, 10))
    setValor('')
    setDescricao('')
    setInvestidorModalId(investidorId === 'TODOS' ? '' : investidorId)
    setErro(null)
  }

  function abrirEdicao(desconto: DescontoFiscalAdministracao) {
    setNovo(false)
    setEditando(desconto)
    setDataPagamento(desconto.dataPagamento.slice(0, 10))
    setValor(desconto.valor.toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }))
    setDescricao(desconto.descricao ?? '')
    setInvestidorModalId(desconto.investidorId ?? '')
    setErro(null)
  }

  function paraNumero(texto: string) {
    return Number(texto.replace(/\./g, '').replace(',', '.'))
  }

  function formatarEntradaMoeda(texto: string) {
    const digitos = texto.replace(/\D/g, '')
    if (!digitos) return ''
    return (Number(digitos) / 100).toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    })
  }

  async function salvar() {
    const valorNumerico = paraNumero(valor)
    const investidorSelecionado =
      investidorModalId || null

    if (!dataPagamento || !Number.isFinite(valorNumerico) || valorNumerico <= 0 || !investidorSelecionado) {
      setErro('Informe investidor, data e valor válidos.')
      return
    }

    try {
      setSalvando(true)
      setErro(null)
      const request = {
        dataPagamento,
        valor: valorNumerico,
        descricao: descricao.trim() || null,
        investidorId: investidorSelecionado,
      }

      if (editando) {
        await atualizarDescontoFiscal(editando.id, request)
      } else {
        await criarDescontoFiscal(request)
      }

      setEditando(null)
      setNovo(false)
      await recarregar?.()
      await carregarFiscal()
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Não foi possível salvar o DARF.')
    } finally {
      setSalvando(false)
    }
  }

  async function excluir(desconto: DescontoFiscalAdministracao) {
    if (!window.confirm('Deseja excluir este DARF?')) return
    try {
      setErro(null)
      await excluirDescontoFiscal(desconto.id)
      await recarregar?.()
      await carregarFiscal()
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Não foi possível excluir o DARF.')
    }
  }

  const modalAberto = novo || editando !== null

  return (
    <>
      <article className="panel admin-panel">
        <div className="admin-toolbar">
          <strong>Fiscal / DARF</strong>
          <div className="admin-toolbar-actions">
            <select value={investidorId} onChange={(e) => setInvestidorId(e.target.value)}>
              <option value="TODOS">TODOS</option>
              {dados.investidores.map((x) => (
                <option key={x.id} value={x.id}>{x.nome}</option>
              ))}
            </select>
            <input
              type="number"
              min="2000"
              max="2100"
              value={ano}
              onChange={(e) => setAno(Number(e.target.value))}
              aria-label="Ano fiscal"
            />
            <button type="button" className="admin-primary-button" onClick={abrirNovo}>
              Novo DARF
            </button>
          </div>
        </div>

        {erro ? <div className="admin-inline-error">{erro}</div> : null}

        <div className="admin-summary-grid">
          <div><span>DARF devido</span><strong>{formatarMoeda(fiscal?.irEstimadoOpcoes ?? 0)}</strong></div>
          <div><span>DARF pago</span><strong className="positive">{formatarMoeda(fiscal?.darfPago ?? 0)}</strong></div>
          <div><span>Saldo DARF</span><strong className={(fiscal?.irEstimadoOpcoes ?? 0) - (fiscal?.darfPago ?? 0) > 0 ? 'negative' : 'positive'}>{formatarMoeda(Math.max(0, (fiscal?.irEstimadoOpcoes ?? 0) - (fiscal?.darfPago ?? 0)))}</strong></div>
          <div><span>Pendências</span><strong>{fiscal?.pendencias ?? 0}</strong></div>
        </div>

        {fiscal?.avisos.map((aviso) => (
          <div className="admin-inline-info" key={aviso}>{aviso}</div>
        ))}

        <div className="fiscal-months">
          {competenciasMensais.map((grupo) => (
            <section className="fiscal-month-card" key={`${grupo.ano}-${grupo.mes}`}>
              <div className="fiscal-month-header">
                <strong>{String(grupo.mes).padStart(2, '0')}/{grupo.ano}</strong>
                <div className="fiscal-month-totals">
                  <span>DARF devido <b>{formatarMoeda(grupo.irEstimado)}</b></span>
                  <span>DARF pago <b>{formatarMoeda(grupo.darfPago)}</b></span>
                  <span>Saldo <b className={grupo.diferenca > 0 ? 'negative' : 'positive'}>{formatarMoeda(Math.max(0, grupo.diferenca))}</b></span>
                </div>
              </div>

              <div className="admin-table-wrap fiscal-participation-wrap">
                <table className="data-table admin-table fiscal-participation-table">
                  <thead>
                    <tr>
                      <th>Investidor</th><th>Participação</th><th>Valor devido</th><th>Valor pago</th><th>Saldo DARF</th>
                    </tr>
                  </thead>
                  <tbody>
                    {grupo.investidores.map((x) => {
                      const participacao = grupo.irEstimado > 0
                        ? (x.irEstimadoOpcoes / grupo.irEstimado) * 100
                        : 0
                      return (
                        <tr key={`${x.ano}-${x.mes}-${x.investidorId ?? 'sem'}`}>
                          <td><strong>{x.investidor}</strong></td>
                          <td>
                            <div className="fiscal-share">
                              <strong>{participacao.toLocaleString('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 })}%</strong>
                              <span><i style={{ width: `${Math.min(100, participacao)}%` }} /></span>
                            </div>
                          </td>
                          <td className="fiscal-tax-value">{formatarMoeda(x.irEstimadoOpcoes)}</td>
                          <td className={x.darfPago > 0 ? 'positive' : ''}>{formatarMoeda(x.darfPago)}</td>
                          <td>
                            {(() => {
                              const saldoDarf = Math.max(0, x.irEstimadoOpcoes - x.darfPago)
                              const pagoIntegral = x.irEstimadoOpcoes > 0 && saldoDarf <= 0.009
                              return (
                                <div className="fiscal-darf-due">
                                  <strong className={saldoDarf > 0 ? 'negative' : 'positive'}>
                                    {formatarMoeda(saldoDarf)}
                                  </strong>
                                  <span className={pagoIntegral ? 'fiscal-paid' : saldoDarf > 0 ? 'fiscal-pending' : 'fiscal-neutral'}>
                                    {pagoIntegral ? 'Pago' : saldoDarf > 0 ? 'A pagar' : 'Sem DARF'}
                                  </span>
                                </div>
                              )
                            })()}
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            </section>
          ))}
          {competenciasMensais.length === 0 ? <div className="admin-empty-state">Sem apuração fiscal para o período.</div> : null}
        </div>
      </article>

      <article className="panel admin-panel">
        <div className="admin-toolbar"><strong>DARFs registrados</strong></div>
        <div className="admin-table-wrap">
          <table className="data-table admin-table">
            <thead>
              <tr><th>Pagamento</th><th>Investidor</th><th>Descrição</th><th>Valor</th><th>Status</th><th>Ações</th></tr>
            </thead>
            <tbody>
              {descontosFiltrados.map((desconto) => (
                <tr key={desconto.id}>
                  <td>{formatarDataCurta(desconto.dataPagamento)}</td>
                  <td>{desconto.investidorNome ?? 'Não atribuído'}</td>
                  <td>{desconto.descricao ?? '—'}</td>
                  <td>{formatarMoeda(desconto.valor)}</td>
                  <td><span className="admin-status-paid">✓ Pago</span></td>
                  <td>
                    <div className="admin-row-actions">
                      <button type="button" className="admin-action" onClick={() => abrirEdicao(desconto)}>Editar</button>
                      <button type="button" className="admin-action admin-action-danger" onClick={() => excluir(desconto)}>Excluir</button>
                    </div>
                  </td>
                </tr>
              ))}
              {descontosFiltrados.length === 0 ? <tr><td colSpan={6}>Nenhum DARF cadastrado.</td></tr> : null}
            </tbody>
          </table>
        </div>
      </article>

      {modalAberto ? (
        <Modal
          title={editando ? 'Editar DARF' : 'Novo DARF'}
          subtitle="Imposto"
          onClose={() => { setEditando(null); setNovo(false) }}
          closeDisabled={salvando}
          footer={
            <>
              <button type="button" className="admin-clear-button" disabled={salvando} onClick={() => { setEditando(null); setNovo(false) }}>Cancelar</button>
              <button type="button" className="admin-primary-button" disabled={salvando} onClick={salvar}>{salvando ? 'Salvando...' : 'Salvar'}</button>
            </>
          }
        >
          <div className="admin-modal-grid">
            <label>
              <span>Investidor</span>
              <select
                value={investidorModalId}
                onChange={(e) => setInvestidorModalId(e.target.value)}
              >
                <option value="">Selecione</option>
                {dados.investidores.map((x) => <option key={x.id} value={x.id}>{x.nome}</option>)}
              </select>
            </label>
            <label><span>Tipo</span><input value="Imposto" readOnly /></label>
            <label><span>Data de pagamento</span><input type="date" value={dataPagamento} onChange={(e) => setDataPagamento(e.target.value)} /></label>
            <label><span>Valor</span><input inputMode="decimal" value={valor} onChange={(e) => setValor(formatarEntradaMoeda(e.target.value))} /></label>
            <label><span>Descrição</span><input value={descricao} onChange={(e) => setDescricao(e.target.value)} /></label>
          </div>
        </Modal>
      ) : null}
    </>
  )
}
