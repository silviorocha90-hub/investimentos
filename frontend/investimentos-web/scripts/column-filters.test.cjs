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

test('combina colunas por AND e valores da mesma coluna por OR, preservando os dados', () => {
  const { filterTableRows } = loader()('components/useColumnFilters.ts')
  const rows = [{ tipo: 'PUT', valor: 'R$ 0,00' }, { tipo: 'CALL', valor: 'R$ -5,00' }, { tipo: 'PUT', valor: '—' }]
  const columns = ['tipo', 'valor'].map(id => ({ id, label: id, value: row => row[id] }))
  assert.equal(filterTableRows(rows, columns, { tipo: ['PUT', 'CALL'] }).length, 3)
  assert.deepEqual(filterTableRows(rows, columns, { tipo: ['PUT'], valor: ['R$ 0,00'] }), [rows[0]])
  assert.deepEqual(filterTableRows(rows, columns, { valor: ['—'] }), [rows[2]])
  assert.equal(filterTableRows(rows, columns, { tipo: ['CALL'], valor: ['R$ 0,00'] }).length, 0)
  assert.equal(rows.length, 3)
})

test('limpa filtros e descarta a seleção ao trocar o investidor', () => {
  const runner = hooks()
  const { useColumnFilters } = runner.load('components/useColumnFilters.ts')
  const rows = [{ ticker: 'ITUB4' }, { ticker: 'VALE3' }]
  const columns = [{ id: 'ticker', label: 'Ativo', value: row => row.ticker }]
  let result = runner.render(() => useColumnFilters(rows, columns, 'Lucas'))
  result.setFilter('ticker', ['VALE3'])
  result = runner.render(() => useColumnFilters(rows, columns, 'Lucas'))
  assert.deepEqual(result.filteredRows, [rows[1]])
  result.clear()
  result = runner.render(() => useColumnFilters(rows, columns, 'Lucas'))
  assert.equal(result.filteredRows.length, 2)
  result.setFilter('ticker', ['VALE3'])
  result = runner.render(() => useColumnFilters(rows, columns, 'Simone'))
  assert.equal(result.active, 0)
  assert.equal(result.filteredRows.length, 2)
})

test('abre o filtro com um clique, busca sem acentos e seleciona um valor', () => {
  const runner = hooks()
  const { ColumnFilterHeader } = runner.load('components/ColumnFilter.tsx')
  let selected
  const props = { label: 'Tipo', values: ['Ação', 'FII'], onChange: value => { selected = value } }
  let tree = runner.render(() => ColumnFilterHeader(props))
  const trigger = nodes(tree).find(node => node.props?.['aria-label'] === 'Filtrar por Tipo')
  trigger.props.ref.current = { getBoundingClientRect: () => ({ left: 10, top: 50, bottom: 80 }) }
  const previousWindow = global.window, previousDocument = global.document
  global.window = { innerWidth: 1024, innerHeight: 768 }
  global.document = { body: {} }
  try {
    trigger.props.onClick()
    tree = runner.render(() => ColumnFilterHeader(props))
    assert.ok(nodes(tree).some(node => node.props?.role === 'dialog'))
    const search = nodes(tree).find(node => node.type === 'input')
    search.props.onChange({ target: { value: 'acao' } })
    tree = runner.render(() => ColumnFilterHeader(props))
    const choices = nodes(tree).filter(node => node.props?.className === 'column-filter-value')
    assert.equal(choices.length, 1)
    choices[0].props.onClick()
    assert.deepEqual(selected, ['Ação'])
  } finally { global.window = previousWindow; global.document = previousDocument }
})

test('filtra todas as opções antes de paginar e mantém os cabeçalhos quando não há resultado', () => {
  const runner = hooks()
  const { OpcoesTabela } = runner.load('pages/Opcoes/components/OpcoesTabela.tsx')
  const opcoes = Array.from({ length: 25 }, (_, index) => ({ id: String(index), tickerOpcao: 'OPCAO' + index,
    tipoOpcao: index === 24 ? 'PUT' : 'CALL', situacao: 'ABERTA', vencimento: '2026-11-19', quantidade: 100,
    strike: 20, resultadoBruto: null, resultadoLiquido: null }))
  let tree = runner.render(() => OpcoesTabela({ opcoes }))
  const next = nodes(tree).find(node => node.type === 'button' && node.props.children === '›')
  next.props.onClick()
  tree = runner.render(() => OpcoesTabela({ opcoes }))
  const typeHeader = nodes(tree).find(node => node.props?.label === 'Tipo')
  typeHeader.props.onChange(['PUT'])
  tree = runner.render(() => OpcoesTabela({ opcoes }))
  assert.ok(nodes(tree).some(node => node.props?.className === 'ticker' && node.props.children === 'OPCAO24'))
  assert.equal(nodes(tree).filter(node => node.type === 'tr').length, 2)
  nodes(tree).find(node => node.props?.label === 'Status').props.onChange(['ENCERRADA'])
  tree = runner.render(() => OpcoesTabela({ opcoes }))
  assert.equal(nodes(tree).filter(node => typeof node.type === 'function' && node.type.name === 'ColumnFilterHeader').length, 10)
  assert.ok(nodes(tree).some(node => node.props?.className === 'column-filter-no-results'))
})

test('Carteira oferece filtro nas 12 colunas das duas tabelas e mantém os valores originais', () => {
  const { renderToStaticMarkup } = require('react-dom/server')
  const React = require('react')
  const { CarteiraView } = loader()('pages/Carteira/CarteiraView.tsx')
  const posicao = { ticker: 'ITUB4', tipoAtivoCodigo: 'ACAO', tipoAtivoNome: 'Ação', quantidade: 100, valorAtual: 4000,
    precoMedio: 30, precoAtual: 40, custoTotal: 3000, valorizacao: 1000, rentabilidadeEconomica: 33.3 }
  const dashboard = { posicoes: [posicao, { ...posicao, ticker: 'PREVIDENCIA', tipoAtivoCodigo: 'PREVIDENCIA' }] }
  const html = renderToStaticMarkup(React.createElement(CarteiraView, { investidores: [{ id: '1', nome: 'Lucas' }],
    dashboardConsolidado: dashboard, carteiras: [{ nome: 'Lucas', dashboard }], selectedInvestor: 'Lucas', onSelectInvestor() {}, snapshotSeries: {} }))
  assert.equal((html.match(/aria-label="Filtrar por /g) ?? []).length, 12)
  assert.ok(html.includes('ITUB4') && html.includes('PREVIDENCIA'))
})
