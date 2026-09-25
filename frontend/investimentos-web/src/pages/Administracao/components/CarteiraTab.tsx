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

  const competenciasAtuaisEFuturas = useMemo(() => {
    const hoje = new Date()
    const anoAtual = hoje.getFullYear()
    const mesAtual = hoje.getMonth() + 1

    return (fiscal?.meses ?? []).filter(
      (x) => x.ano > anoAtual || (x.ano === anoAtual && x.mes >= mesAtual),
    )
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
          <div><span>Resultado comum - opções</span><strong>{formatarMoeda(fiscal?.resultadoComumOpcoes ?? 0)}</strong></div>
          <div><span>Resultado day trade - opções</span><strong>{formatarMoeda(fiscal?.resultadoDayTradeOpcoes ?? 0)}</strong></div>
          <div><span>IR estimado - opções</span><strong>{formatarMoeda(fiscal?.irEstimadoOpcoes ?? 0)}</strong></div>
          <div><span>DARF pago</span><strong>{formatarMoeda(fiscal?.darfPago ?? 0)}</strong></div>
          <div><span>Diferença estimado x pago</span><strong>{formatarMoeda(fiscal?.diferencaEstimadoPago ?? 0)}</strong></div>
          <div><span>Pendências fiscais</span><strong>{fiscal?.pendencias ?? 0}</strong></div>
        </div>

        {fiscal?.avisos.map((aviso) => (
          <div className="admin-inline-info" key={aviso}>{aviso}</div>
        ))}

        <div className="admin-table-wrap">
          <table className="data-table admin-table">
            <thead>
              <tr>
                <th>Competência</th><th>Investidor</th><th>Comum</th><th>Day trade</th>
                <th>IR estimado</th><th>DARF pago</th><th>Diferença</th><th>Operações</th>
              </tr>
            </thead>
            <tbody>
              {competenciasAtuaisEFuturas.map((x) => (
                <tr key={`${x.ano}-${x.mes}-${x.investidorId ?? 'sem'}`}>
                  <td>{String(x.mes).padStart(2, '0')}/{x.ano}</td>
                  <td>{x.investidor}</td>
                  <td>{formatarMoeda(x.resultadoComumOpcoes)}</td>
                  <td>{formatarMoeda(x.resultadoDayTradeOpcoes)}</td>
                  <td>{formatarMoeda(x.irEstimadoOpcoes)}</td>
                  <td>{formatarMoeda(x.darfPago)}</td>
                  <td>{formatarMoeda(x.diferencaEstimadoPago)}</td>
                  <td>{x.operacoesConsideradas}</td>
                </tr>
              ))}
              {competenciasAtuaisEFuturas.length === 0 ? <tr><td colSpan={8}>Sem competências atuais ou futuras.</td></tr> : null}
            </tbody>
          </table>
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
