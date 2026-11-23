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
    const source = fs.readFileSync(file, 'utf8').replaceAll('import.meta.env', "({ VITE_API_URL: 'https://api.test' })")
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

const investidores = [{ id: 'origem', nome: 'Origem' }, { id: 'destino', nome: 'Destino' }]
const operacao = { id: 'antiga', investidorId: 'origem', ativoId: 'ativo', ticker: 'ITUB4', nome: 'Itaú', tipoOperacao: 'COMPRA', quantidade: 100, precoUnitario: 10, taxas: 0, data: '2025-08-01T00:00:00', sequencia: 1 }
function editar(runner, View, onSaveOperation) {
  const props = { investidores, selectedInvestor: 'TOTAL', onSelectInvestor() {}, operacoes: [{ ...operacao, investidorId: 'ORIGEM' }], onSaveOperation }
  let tree = runner.render(() => View(props))
  nodes(tree).find(node => node.type === 'button' && node.props.children === 'Editar').props.onClick()
  tree = runner.render(() => View(props))
  return { props, tree }
}
function eventoSalvar() {
  const inputs = { data: '2025-08-01', quantidade: '100', precoUnitario: '10', taxas: '0', investidorId: 'destino' }
  return { currentTarget: { closest: () => ({ querySelectorAll: () => Object.entries(inputs).map(([name, value]) => ({ name, value })) }) } }
}

test('a edição de uma operação antiga oferece o investidor atual e envia a nova escolha com o mesmo ID', async () => {
  const runner = hooks()
  const { OperacoesView } = runner.load('pages/Operacoes/OperacoesView.tsx')
  let salvo
  const { tree } = editar(runner, OperacoesView, async value => { salvo = value })
  const select = nodes(tree).find(node => node.type === 'select' && node.props.name === 'investidorId')
  assert.equal(select.props.defaultValue, 'origem')
  assert.ok(nodes(select).some(node => node.type === 'option' && node.props.value === 'destino'))
  await nodes(tree).find(node => node.type === 'button' && node.props.children === 'Salvar').props.onClick(eventoSalvar())
  assert.deepEqual(salvo, { id: 'antiga', investidorId: 'destino', data: '2025-08-01', quantidade: 100, precoUnitario: 10, taxas: 0 })
})

test('uma transferência bloqueada mostra a mensagem e mantém a operação em edição', async () => {
  const runner = hooks()
  const { OperacoesView } = runner.load('pages/Operacoes/OperacoesView.tsx')
  const { tree, props } = editar(runner, OperacoesView, async () => { throw new Error('Venda sem quantidade suficiente na origem.') })
  await nodes(tree).find(node => node.type === 'button' && node.props.children === 'Salvar').props.onClick(eventoSalvar())
  const atual = runner.render(() => OperacoesView(props))
  assert.ok(nodes(atual).some(node => node.props?.role === 'alert' && node.props.children.includes('origem')))
  assert.ok(nodes(atual).some(node => node.type === 'select' && node.props.name === 'investidorId'))
})

test('o cliente identifica o titular na listagem e inclui a escolha no PUT autenticado', async () => {
  const api = loader()('api/dashboardApi.ts')
  const previous = global.fetch
  let request
  global.fetch = async (url, options) => {
    request = { url, options }
    return { ok: true, json: async () => [{ ...operacao, investidorId: undefined }] }
  }
  try {
    const lista = await api.obterOperacoes('origem')
    assert.equal(lista[0].investidorId, 'origem')
    await api.atualizarOperacao('antiga', { ...operacao, investidorId: 'destino' })
    assert.equal(request.options.method, 'PUT')
    assert.equal(request.options.credentials, 'include')
    assert.equal(JSON.parse(request.options.body).investidorId, 'destino')
  } finally { global.fetch = previous }
})
