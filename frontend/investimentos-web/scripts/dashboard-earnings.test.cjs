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


const React = require('react')
const { renderToStaticMarkup } = require('react-dom/server')
const { DashboardView } = loader()('pages/Dashboard/DashboardView.tsx')

function render(dashboard) {
  return renderToStaticMarkup(React.createElement(DashboardView, {
    dashboard, carteiras: [], evolucao: [], apiDisponivel: true, erro: null,
  }))
}

test('o painel exibe os dois rendimentos da API e sinaliza prejuízos', () => {
  const html = render({ rendimentoOpcoes: -125.50, rendimentoAcoesFiis: 230.75,
    posicoes: [], proventos: [], opcoes: [], distribuicaoPorTipo: [] })
  assert.match(html, /Rendimento de opções/)
  assert.match(html, /Rendimento de ações \+ FIIs/)
  assert.match(html, /-R\$[^<]*125,50/)
  assert.match(html, /R\$[^<]*230,75/)
  assert.match(html, /portfolio-earnings-negative/)
  assert.match(html, /portfolio-earnings-positive/)
  assert.doesNotMatch(html, /Entradas|Saídas|Capital aportado|Capital retirado/)
})

test('o painel sem dados apresenta zero e mantém a evolução patrimonial disponível', () => {
  const html = render(null)
  assert.match(html, /Rendimento de opções/)
  assert.match(html, /Rendimento de ações \+ FIIs/)
  assert.doesNotMatch(html, /NaN|Infinity|Entradas|Saídas/)
})
