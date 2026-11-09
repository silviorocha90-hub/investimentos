const { test } = require('node:test')
const assert = require('node:assert/strict')
const fs = require('node:fs')
const path = require('node:path')
const vm = require('node:vm')
const ts = require('typescript')

function loader(react) {
  const cache = new Map()
  function load(file) {
    if (cache.has(file)) return cache.get(file).exports
    const module = { exports: {} }
    cache.set(file, module)
    const source = fs.readFileSync(file, 'utf8')
    const code = ts.transpileModule(source, { compilerOptions: {
      module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022, jsx: ts.JsxEmit.ReactJSX,
    } }).outputText
    function localRequire(name) {
      if (name.endsWith('.css')) return {}
      if (react && name === 'react') return react
      if (react && name === 'react-dom') return { createPortal: node => node }
      if (!name.startsWith('.')) return require(name)
      const base = path.resolve(path.dirname(file), name)
      const resolved = [base, base + '.ts', base + '.tsx'].find(candidate => fs.existsSync(candidate) && fs.statSync(candidate).isFile())
      return load(resolved)
    }
    vm.runInThisContext('(function(require,module,exports){' + code + '\n})', { filename: file })(localRequire, module, module.exports)
    return module.exports
  }
  return name => load(path.resolve(__dirname, '../src', name))
}

function hooks() {
  let cursor = 0
  const slots = []
  const react = {
    useState(initial) {
      const index = cursor++
      if (!(index in slots)) slots[index] = typeof initial === 'function' ? initial() : initial
      return [slots[index], value => { slots[index] = typeof value === 'function' ? value(slots[index]) : value }]
    },
    useMemo: fn => fn(), useEffect: () => {}, useId: () => 'test-filter',
    useRef(initial) { return react.useState({ current: initial })[0] },
  }
  return { load: loader(react), render: fn => { cursor = 0; return fn() } }
}

function nodes(node) {
  if (Array.isArray(node)) return node.flatMap(nodes)
  if (!node || typeof node !== 'object') return []
  return [node, ...nodes(node.props?.children)]
}

test('ordena valores numéricos reais, inclusive negativos e zero, sem alterar a origem', () => {
  const { sortTableRows } = loader()('components/useTableSort.ts')
  const rows = [100, 2, -5, 0, 10, null, undefined].map((valor, id) => ({ id, valor }))
  const columns = [{ id: 'valor', label: 'Valor', value: row => row.valor }]
  assert.deepEqual(sortTableRows(rows, columns, { column: 'valor', direction: 'asc' }).map(row => row.valor), [-5, 0, 2, 10, 100, null, undefined])
  assert.deepEqual(sortTableRows(rows, columns, { column: 'valor', direction: 'desc' }).map(row => row.valor), [100, 10, 2, 0, -5, null, undefined])
  assert.deepEqual(rows.map(row => row.valor), [100, 2, -5, 0, 10, null, undefined])
})

test('ordena datas cronologicamente entre meses e anos, com ausências no fim', () => {
  const { sortTableRows, dateSortValue } = loader()('components/useTableSort.ts')
  const rows = ['2027-01-02', '2026-12-30', null, '2026-11-19']
  const columns = [{ id: 'data', label: 'Data', value: dateSortValue }]
  assert.deepEqual(sortTableRows(rows, columns, { column: 'data', direction: 'asc' }), ['2026-11-19', '2026-12-30', '2027-01-02', null])
  assert.equal(dateSortValue('data inválida'), null)
})

test('ordena textos em português e preserva a sequência dos empates', () => {
  const { sortTableRows } = loader()('components/useTableSort.ts')
  const rows = [{ id: 1, nome: 'VALE3' }, { id: 2, nome: 'AÇÃO10' }, { id: 3, nome: 'acao2' }, { id: 4, nome: 'VALE3' }]
  const columns = [{ id: 'nome', label: 'Nome', value: row => row.nome }]
  assert.deepEqual(sortTableRows(rows, columns, { column: 'nome', direction: 'asc' }).map(row => row.id), [3, 2, 1, 4])
})

test('um clique ordena crescente, outro decrescente e uma nova coluna inicia crescente', () => {
  const runner = hooks()
  const { useTableSort } = runner.load('components/useTableSort.ts')
  const rows = [{ nome: 'B', valor: 10 }, { nome: 'A', valor: 2 }]
  const columns = [{ id: 'valor', label: 'Valor', value: row => row.valor }, { id: 'nome', label: 'Nome', value: row => row.nome }]
  let result = runner.render(() => useTableSort(rows, columns, 'Lucas'))
  result.toggle('valor')
  result = runner.render(() => useTableSort(rows, columns, 'Lucas'))
  assert.equal(result.sort.direction, 'asc'); assert.equal(result.sortedRows[0].valor, 2)
  result.toggle('valor')
  result = runner.render(() => useTableSort(rows, columns, 'Lucas'))
  assert.equal(result.sort.direction, 'desc'); assert.equal(result.sortedRows[0].valor, 10)
  result.toggle('nome')
  result = runner.render(() => useTableSort(rows, columns, 'Lucas'))
  assert.equal(result.sort.direction, 'asc'); assert.equal(result.sortedRows[0].nome, 'A')
  result = runner.render(() => useTableSort(rows, columns, 'Simone'))
  assert.equal(result.sort, null)
})

test('cabeçalho indica direção e oferece um botão acessível, sem painel de filtro', () => {
  const { SortableColumnHeader } = loader()('components/SortableColumnHeader.tsx')
  let clicked = 0
  const tree = SortableColumnHeader({ label: 'Strike', direction: 'asc', onSort: () => clicked++ })
  assert.equal(tree.props['aria-sort'], 'ascending')
  const button = nodes(tree).find(node => node.type === 'button')
  assert.match(button.props['aria-label'], /ordem decrescente/)
  button.props.onClick(); assert.equal(clicked, 1)
  assert.ok(!nodes(tree).some(node => node.props?.role === 'dialog'))
})

test('Lista de Opções ordena todos os registros antes de paginar e volta à primeira página', () => {
  const runner = hooks()
  const { OpcoesTabela } = runner.load('pages/Opcoes/components/OpcoesTabela.tsx')
  const opcoes = Array.from({ length: 25 }, (_, index) => ({ id: String(index), tickerOpcao: 'OPCAO' + index,
    tipoOpcao: 'PUT', situacao: 'ABERTA', vencimento: '2026-11-19', quantidade: 100,
    strike: 25 - index, resultadoBruto: null, resultadoLiquido: null }))
  let tree = runner.render(() => OpcoesTabela({ opcoes }))
  nodes(tree).find(node => node.type === 'button' && node.props.children === '›').props.onClick()
  tree = runner.render(() => OpcoesTabela({ opcoes }))
  nodes(tree).find(node => node.props?.label === 'Strike').props.onSort()
  tree = runner.render(() => OpcoesTabela({ opcoes }))
  let tickers = nodes(tree).filter(node => node.props?.className === 'ticker').map(node => node.props.children)
  assert.equal(tickers.length, 10); assert.equal(tickers[0], 'OPCAO24')
  nodes(tree).find(node => node.props?.label === 'Strike').props.onSort()
  tree = runner.render(() => OpcoesTabela({ opcoes }))
  tickers = nodes(tree).filter(node => node.props?.className === 'ticker').map(node => node.props.children)
  assert.equal(tickers[0], 'OPCAO0')
})

test('Carteira oferece ordenação nas 12 colunas e conserva a ordem inicial por valor atual', () => {
  const { renderToStaticMarkup } = require('react-dom/server')
  const React = require('react')
  const { CarteiraView } = loader()('pages/Carteira/CarteiraView.tsx')
  const posicao = { ticker: 'ITUB4', tipoAtivoCodigo: 'ACAO', tipoAtivoNome: 'Ação', quantidade: 100, valorAtual: 4000,
    precoMedio: 30, precoAtual: 40, custoTotal: 3000, valorizacao: 1000, rentabilidadeEconomica: 33.3 }
  const dashboard = { posicoes: [posicao, { ...posicao, ticker: 'PREVIDENCIA', tipoAtivoCodigo: 'PREVIDENCIA' }] }
  const html = renderToStaticMarkup(React.createElement(CarteiraView, { investidores: [{ id: '1', nome: 'Lucas' }],
    dashboardConsolidado: dashboard, carteiras: [{ nome: 'Lucas', dashboard }], selectedInvestor: 'Lucas', onSelectInvestor() {}, snapshotSeries: {} }))
  assert.equal((html.match(/aria-label="Ordenar por /g) ?? []).length, 12)
  assert.equal((html.match(/aria-sort="descending"/g) ?? []).length, 2)
  assert.ok(!html.includes('Filtrar por') && html.includes('ITUB4'))
})
