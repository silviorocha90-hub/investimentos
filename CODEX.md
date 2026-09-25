# CODEX — Projeto Investimentos

> Documento técnico de referência do projeto.
>
> Objetivo: permitir que o desenvolvimento seja retomado no futuro sem
> necessidade de redescobrir arquitetura, regras de negócio, estrutura do
> banco, formato do Excel, decisões técnicas e estado atual da implementação.
>
> Este arquivo deve ser atualizado sempre que houver uma mudança estrutural
> importante no projeto.

---

# 1. VISÃO GERAL

O projeto Investimentos é um sistema pessoal de gestão de investimentos.

Ele nasceu a partir de uma planilha Excel existente e de um dashboard
Power BI já utilizado para acompanhar a carteira.

O objetivo é transformar essa estrutura em uma aplicação própria,
mantendo inicialmente o Excel como fonte de verdade dos dados históricos.

O sistema também funciona como projeto de atualização técnica em:

- .NET 10
- C#
- Entity Framework Core
- SQL Server 2025
- Clean Architecture
- SOLID
- React
- TypeScript
- Vite

O foco não é apenas reproduzir o Excel.

A intenção é construir uma aplicação estruturada que futuramente possa
substituir Excel e Power BI como ferramenta principal de gestão da carteira.

---

# 2. PRINCÍPIOS DE DESENVOLVIMENTO

## 2.1 Ritmo de desenvolvimento

O desenvolvimento deve ser conduzido passo a passo.

Evitar:

- alterações enormes sem validação intermediária;
- microetapas excessivas de TDD;
- refatorações sem necessidade prática;
- mudanças simultâneas em várias camadas quando não forem necessárias.

Preferir:

1. implementar uma unidade coerente;
2. compilar;
3. testar;
4. validar o resultado;
5. somente então avançar.

---

## 2.2 Arquivos completos

Sempre que um arquivo precisar ser alterado, fornecer o conteúdo COMPLETO
do arquivo.

Nunca instruir apenas:

"localize este trecho e substitua".

Isso reduz erros de edição e facilita acompanhar a evolução do projeto.

---

## 2.3 Tratamento de erros

Quando ocorrer erro:

1. identificar o erro exato;
2. localizar o arquivo/camada responsável;
3. entender a causa;
4. somente depois alterar código.

Não realizar alterações especulativas em vários arquivos.

---

## 2.4 Banco de dados

Não administrar SQL Server através de PowerShell.

Usar:

- Visual Studio / Package Manager Console para migrations;
- SQL Server Management Studio para inspeção e administração;
- aplicação/importador para operações programáticas.

---

# 3. STACK

Backend:

- .NET SDK 10.0.400
- ASP.NET Core 10
- Entity Framework Core 10.0.11
- SQL Server 2025

Frontend:

- React
- TypeScript
- Vite
- Node.js 24.20.0
- npm 11.19.0

Ferramentas:

- Visual Studio
- SSMS 22.9.2
- Git 2.55

Importação Excel:

- ClosedXML 0.105.0

---

# 4. ARQUITETURA

Arquitetura lógica pretendida:

Investimentos
├── src
│   ├── Investimentos.Domain
│   ├── Investimentos.Application
│   ├── Investimentos.Infrastructure
│   └── Investimentos.Api
├── tests
│   ├── Investimentos.Domain.Tests
│   ├── Investimentos.Application.Tests
│   └── Investimentos.IntegrationTests
├── frontend
│   └── investimentos-web
└── Investimentos.Importador

IMPORTANTE:

A estrutura física atual possui alguns caminhos diferentes dessa estrutura
lógica.

Não reorganizar os projetos apenas por estética.

Existem ProjectReferences gerados pelo Visual Studio e já funcionando.

Preservar os caminhos atuais até existir uma razão concreta para
reorganização.

---

# 5. BANCO DE DADOS

Banco principal:

InvestimentosDb

Servidor:

localhost

Autenticação:

Windows Authentication

Connection string atual:

Server=localhost;
Database=InvestimentosDb;
Trusted_Connection=True;
TrustServerCertificate=True;

Banco de integração:

InvestimentosDbTests

---

# 6. SEGURANÇA DO IMPORTADOR

O importador somente pode reconstruir:

InvestimentosDb

Antes de qualquer reset deve verificar:

Database.GetDbConnection().Database

Se o nome não for exatamente:

InvestimentosDb

o reset deve ser bloqueado.

Isso existe para impedir que o importador apague outro banco por engano.

---

# 7. TABELAS DE REFERÊNCIA

Estas tabelas NÃO devem ser apagadas pelo importador:

- ClasseAtivo
- TipoAtivo
- TipoOperacao
- __EFMigrationsHistory

São dados estruturais do sistema.

---

# 8. CLASSES DE ATIVOS

ClasseAtivo:

1. RENDA_FIXA
2. RENDA_VARIAVEL
3. FUNDOS
4. ALTERNATIVOS

Todos ativos.

---

# 9. TIPOS DE ATIVOS

O banco possui atualmente 20 tipos de ativos.

Tipos adicionados posteriormente:

RENDA_FIXA
→ Classe RENDA_FIXA

PREVIDENCIA
→ Classe FUNDOS

Mapeamentos históricos importantes:

LCA
→ LCA

NEON
→ RENDA_FIXA

PREV
→ PREVIDENCIA

---

# 10. ENTIDADES PRINCIPAIS

## Investidor

Campos principais:

- Id
- Nome

O nome é obrigatório.

---

## Ativo

Campos:

- Id
- Ticker
- Nome
- TipoAtivoId
- TipoAtivo

Ticker é Value Object.

Ativo exige TipoAtivo válido.

---

## TipoOperacao

Tipos atualmente relevantes:

- COMPRA
- VENDA

---

## Operacao

Campos:

- Id
- Data
- Sequencia
- InvestidorId
- AtivoId
- TipoOperacaoId
- Quantidade
- PrecoUnitario
- Taxas

ValorBruto:

Quantidade * PrecoUnitario

A sequência é utilizada para ordenar operações ocorridas no mesmo dia.

---

# 11. PROVENTOS

Entidade:

Provento

Campos:

- Id
- Investidor
- Ativo
- Tipo
- Descricao
- DataCom
- DataPagamento
- QuantidadeBase
- ValorPorUnidade
- ValorRecebido

Tipos permitidos:

- DIVIDENDO
- JCP
- RENDIMENTO

No Excel:

JUROS
→ JCP

ValorBruto:

QuantidadeBase * ValorPorUnidade

ImpostoRetido:

max(0, ValorBruto - ValorRecebido)

IMPORTANTE:

ValorRecebido é preservado do Excel.

Não assumir que ValorRecebido = ValorBruto porque JCP e outros proventos
podem possuir retenção.

---

# 12. OPÇÕES

Entidade:

OperacaoOpcao

Dados relevantes:

- Investidor
- Ativo-base
- TickerOpcao
- TipoOpcao
- Natureza
- DataOperacao
- DataFinalizacao
- Vencimento
- Strike
- Contratos
- Quantidade
- PremioUnitario
- Taxas
- ResultadoInformado
- PrecoRecompra
- ValorExecucao
- Situacao

Tipos:

- PUT
- CALL

Natureza:

- COMPRA
- VENDA

Situações:

- ABERTA
- ENCERRADA
- EXERCIDA
- EXPIRADA

---

# 13. REGRA IMPORTANTE DAS OPÇÕES

ENCERRADA:

- exige DataFinalizacao;
- exige PrecoRecompraUnitario.

EXERCIDA:

- NÃO exige DataFinalizacao.

Isso é proposital.

No histórico do Excel as opções exercidas não possuem necessariamente
uma data de finalização registrada.

Portanto:

MarcarExercida()

mantém:

DataFinalizacao = null

Não alterar essa regra sem revisar o histórico real.

---

# 14. RESULTADO DAS OPÇÕES

ResultadoInformado é nullable.

Ele preserva o resultado existente no Excel/MyProfit.

Isso é necessário porque o resultado informado pode conter custos/taxas
que não são reproduzidos apenas por:

prêmio recebido - recompra

ResultadoFinal da entidade:

ABERTA
→ null

ENCERRADA
→ calculado

EXPIRADA
→ prêmio

EXERCIDA
→ null

---

# 15. RESOLUÇÃO DE ATIVO-BASE DAS OPÇÕES

Existe:

ResolvedorAtivoBaseOpcao

Mapeamentos especiais:

PETR → PETR4
CPLE → CPLE3
SAPR → SAPR11
AXIA → AXIA3
SANB → SANB4
ALOS → ALOS3

Última validação:

61 opções
61 resolvidas
0 ambíguas
0 não resolvidas

---

# 16. COTAÇÃO DE ATIVOS

Entidade:

CotacaoAtivo

Campos:

- Id
- AtivoId
- DataReferencia
- Preco

Índice único:

AtivoId + DataReferencia

Preço:

decimal(18,8)

Enquanto o Excel for fonte de verdade, a data de referência da cotação
será o momento da importação.

Todas as cotações de uma mesma importação devem utilizar EXATAMENTE
o mesmo DateTime.

---

# 17. SALDO DISPONÍVEL

Entidade:

SaldoDisponivel

Campos:

- Id
- InvestidorId
- DataReferencia
- Valor

Índice único:

InvestidorId + DataReferencia

Valor:

decimal(18,2)

Saldo negativo é permitido.

Isso é uma decisão de domínio deliberada.

A data de referência será o mesmo snapshot utilizado nas cotações.

---

# 18. HISTÓRICO PATRIMONIAL

Entidade:

HistoricoPatrimonio

Campos:

- Id
- InvestidorId
- DataReferencia
- ValorCarteira

Índice único:

InvestidorId + DataReferencia

ValorCarteira não pode ser negativo.

Diferentemente de CotacaoAtivo e SaldoDisponivel, a data vem diretamente
do histórico existente no Excel.

---

# 19. MIGRATIONS EXISTENTES

Migrations conhecidas:

- CriacaoInicial
- AdicionarInvestidor
- AdicionarOperacoes
- AdicionarUnicidadeInvestidor
- AjustarMapeamentoTicker
- AdicionarSequenciaOperacao
- AdicionarProventosEOpcoes
- AmpliarHistoricoOperacoesOpcoes
- AjustarHistoricoProventos
- CorrigirHistoricoOperacoesOpcoes
- AdicionarTiposRendaFixaEPrevidencia
- PreservarDadosHistoricosExcel
- AdicionarCotacaoAtivo
- AdicionarSaldoDisponivel
- AdicionarHistoricoPatrimonio

Não editar migrations antigas.

Não editar ModelSnapshot manualmente.

Criar novas migrations quando houver alteração real de modelo.

---

# 20. EXCEL FONTE DE VERDADE

Arquivo:

Investimentos.xlsx

Enquanto o processo de migração não estiver concluído, o Excel continua
sendo a fonte de verdade.

O importador deve reconstruir os dados de negócio a partir dele.

---

# 21. PLANILHAS DO EXCEL

## Qtde Ativos (bkp)

FONTE PRINCIPAL DO HISTÓRICO DE OPERAÇÕES.

Não usar "Qtde Ativos" como histórico completo.

Colunas:

- Data
- Ativo
- Pessoa
- Transação
- Qtde
- Preço Médio
- Taxa Bolsa
- Custo Total (R$)
- Valor Recebido (R$)

Última leitura validada:

226 operações

179 compras
47 vendas

Período aproximado:

10/07/2025 a 03/09/2026

Todas as vendas possuem posição anterior suficiente.

Existe uma linha auxiliar conhecida:

Linha 213

Data:
28/08/2026

Ativo:
Valor

Investidor:
-

Transação:
COMPRA

Essa linha NÃO representa uma operação real.

Ela pode gerar Pendencia no relatório, mas não bloqueia a importação.

---

# 22. QTDE ATIVOS

A aba:

Qtde Ativos

NÃO representa o histórico completo.

Possui aproximadamente 44 registros recentes.

Não utilizar como fonte primária das operações.

---

# 23. PROVENTOS ATIVOS

Fonte dos proventos.

Colunas:

- Data
- Ativo
- Descrição
- Pessoa
- Cotas
- Valor por Cota
- Valor Recebido
- Tipo Provento

Última validação:

172 registros

Tipos:

- Rendimento
- Juros
- Dividendo

Mapeamento:

Juros → JCP

---

# 24. OPÇÕES ATIVOS

Fonte das operações com opções.

Colunas relevantes:

1 Data
2 Finalizada
3 Pessoa
4 Ativo
5 Tipo
6 Direção
7 Qtde
8 Strike
9 Valor Ação
10 Venc
11 Venda
12 Recompra/Atual
13 MyProfit
14 Status
15 Venc Dias
16 Execução
17 Recebido
18 Resultado Final
19 %

IMPORTANTE:

Recompra/Atual é PREÇO UNITÁRIO.

MyProfit é preservado como ResultadoInformado.

Última validação:

61 opções

40 PUT
21 CALL

46 ENCERRADA
15 EXERCIDA

46 encerradas possuem DataFinalizacao.

Exercidas não exigem DataFinalizacao.

---

# 25. COTAÇÃO ATIVOS

Possui:

- Ativo
- Tipo
- Preço Atual

Última leitura:

19 cotações válidas
0 duplicadas

A mesma planilha também é usada como fonte de classificação de ativos.

Existe uma linha auxiliar com:

VALOR
Disponível

Essa linha NÃO representa ativo financeiro.

O leitor deve ignorar linhas cujo Tipo seja:

Disponível
ou
Disponivel

antes de adicionar o ticker em:

- CadastroAtivos
- Ativos
- Cotacoes

---

# 26. VALORES DISPONÍVEIS

Fonte do saldo disponível por pessoa.

Última leitura:

6 registros
0 duplicados

Investidores:

- Anna
- Lucas
- Mãe
- Silene
- Silvio
- Simone

O valor pode ser negativo.

A planilha não possui data explícita.

DataReferencia será o snapshot da importação.

---

# 27. LINHA DO TEMPO

Fonte do histórico patrimonial.

Estrutura:

Pessoa
Data
Carteira

Última leitura:

40 registros
0 duplicados

Distribuição histórica conhecida:

Silvio 19
Silene 7
Mãe 6
Lucas 4
Simone 2
Anna 2

Esses registros alimentam:

HistoricoPatrimonio

---

# 28. PLANILHAS DERIVADAS / REFERÊNCIA

Não importar diretamente como fonte de verdade:

- Carteira Silvio
- Análise
- Rentabilidade
- Resultado Mensal
- IRPF 2025

Motivo:

são dashboards, cálculos, consolidações ou referências.

A aplicação deve futuramente reproduzir os cálculos necessários a partir
dos dados estruturados.

---

# 29. DADOSIMPORTACAO

DadosImportacao contém atualmente:

- Operacoes
- Proventos
- Opcoes
- CadastroAtivos
- Cotacoes
- SaldosDisponiveis
- HistoricosPatrimonio
- Pendencias
- Ativos
- Investidores

Ativos:

HashSet case-insensitive.

Investidores:

HashSet case-insensitive.

---

# 30. RECORDS DE IMPORTAÇÃO

## OperacaoImportacao

- LinhaExcel
- Data
- Ativo
- Investidor
- TipoOperacao
- Quantidade
- PrecoUnitario
- Taxas
- CustoTotalInformado
- ValorRecebidoInformado

IMPORTANTE:

A propriedade chama-se:

Ativo

e NÃO Ticker.

---

## ProventoImportacao

- LinhaExcel
- DataPagamento
- Ativo
- Descricao
- Investidor
- QuantidadeBase
- ValorPorUnidade
- ValorRecebido
- Tipo

---

## OpcaoImportacao

- LinhaExcel
- DataOperacao
- DataFinalizacao
- Investidor
- TickerOpcao
- TipoOpcao
- Natureza
- Quantidade
- Strike
- Vencimento
- PremioUnitario
- PrecoRecompraUnitario
- ResultadoInformado
- ValorExecucao
- Situacao

---

## CotacaoAtivoImportacao

- LinhaExcel
- Ativo
- Preco

Não possui DataReferencia.

A data será definida pelo coordenador da importação.

---

## SaldoDisponivelImportacao

- LinhaExcel
- Investidor
- Valor

Não possui DataReferencia.

A data será definida pelo coordenador da importação.

---

## HistoricoPatrimonioImportacao

- LinhaExcel
- Investidor
- DataReferencia
- ValorCarteira

---

# 31. LEITOR DO EXCEL

Classe:

LeitorExcelInvestimentos

Responsabilidades:

- abrir Excel;
- extrair dados;
- normalizar textos;
- converter datas;
- converter decimais;
- transformar linhas em records de importação;
- registrar pendências;
- alimentar conjuntos globais de ativos/investidores.

Métodos principais:

- LerOperacoes
- LerProventos
- LerOpcoes
- LerCadastroAtivos
- LerSaldosDisponiveis
- LerHistoricoPatrimonio

---

# 32. VALIDAÇÃO DO HISTÓRICO

Classe:

ValidadorHistoricoOperacoes

Ordenação:

Data
LinhaExcel

A validação mantém posição acumulada por investidor/ativo.

Venda somente é válida quando existe posição anterior suficiente.

Última execução:

Todas as vendas possuem posição anterior suficiente.

---

# 33. ÚLTIMO DRY-RUN VALIDADO

Antes da correção da linha auxiliar VALOR:

Operações:
226

Compras:
179

Vendas:
47

Proventos:
172

Opções:
61

PUT:
40

CALL:
21

Cotações:
19

Cotações duplicadas:
0

Saldos:
6

Saldos duplicados:
0

Histórico patrimonial:
40

Históricos duplicados:
0

Investidores:
6

Opções encerradas:
46

Opções exercidas:
15

Encerradas com data:
46

Encerradas sem data:
0

Ativos-base resolvidos:
61/61

Histórico:
válido

O dry-run mostrou temporariamente:

51 ativos
50 classificados
1 sem classificação

O item indevido era:

VALOR
Tipo Excel = Disponível

A causa foi identificada:

a linha auxiliar da aba Cotação Ativos estava sendo adicionada ao conjunto
dados.Ativos.

A correção definida foi ignorar Tipo = Disponível/Disponivel no
LerCadastroAtivos.

Resultado esperado após correção:

50 ativos
50 classificados
0 sem classificação

Cotações devem continuar:

19

---

# 34. IMPORTAÇÃO: REGRA FUNDAMENTAL

Enquanto o Excel for fonte de verdade:

CADA EXECUÇÃO REAL DO IMPORTADOR DEVE RECONSTRUIR TODOS OS DADOS DE NEGÓCIO.

Não realizar importação incremental neste estágio.

Fluxo:

1. Ler Excel inteiro.
2. Validar Excel inteiro.
3. Se houver erro bloqueante, NÃO alterar banco.
4. Confirmar banco InvestimentosDb.
5. Abrir transação SQL.
6. Apagar dados importáveis.
7. Recriar investidores.
8. Recriar ativos.
9. Importar operações.
10. Importar proventos.
11. Importar opções.
12. Importar cotações.
13. Importar saldos.
14. Importar histórico patrimonial.
15. Validar quantidades Excel x SQL.
16. Commit.

Qualquer falha:

ROLLBACK.

Nunca deixar o banco parcialmente reconstruído.

---

# 35. DADOS QUE DEVEM SER APAGADOS

Durante reset:

1. OperacaoOpcao
2. Provento
3. Operacao
4. CotacaoAtivo
5. SaldoDisponivel
6. HistoricoPatrimonio
7. Ativo
8. Investidor

A ordem deve respeitar as FKs.

---

# 36. DADOS QUE DEVEM SER PRESERVADOS

Nunca apagar no reset:

- ClasseAtivo
- TipoAtivo
- TipoOperacao
- __EFMigrationsHistory

---

# 37. RESETBANCO DADOS

Classe existente:

ResetBancoDados

Possui proteção:

BancoPermitido = "InvestimentosDb"

Ainda precisa ser atualizado para incluir:

- CotacoesAtivos
- SaldosDisponiveis
- HistoricosPatrimonio

antes de apagar Ativo/Investidor.

---

# 38. IMPORTADORCADASTROS

Responsável por:

- carregar TipoAtivo;
- criar Investidor;
- determinar ativos necessários;
- resolver ativos-base das opções;
- classificar ativos;
- criar Ativo;
- salvar cadastros.

IMPORTANTE:

TiposAtivos NÃO devem ser carregados com AsNoTracking quando forem usados
como entidades de referência dos novos Ativos.

Isso já foi corrigido.

---

# 39. IMPORTADOROPERACOES

Carrega TipoOperacao do banco.

IMPORTANTE:

TiposOperacoes também NÃO utiliza AsNoTracking porque as entidades são
utilizadas no novo grafo.

Sequência é calculada por:

Investidor + Data

As operações são ordenadas por:

Data
LinhaExcel

---

# 40. IMPORTADORPROVENTOS

Já existe.

Converte ProventoImportacao para Provento.

DataCom atualmente:

null

Descrição do Excel é preservada.

---

# 41. IMPORTADOROPCOES

Já existe.

Resolve o ativo-base.

Quantidade precisa ser múltipla de 100.

Contratos:

Quantidade / 100

Aplica situação:

ABERTA
→ nenhuma finalização

ENCERRADA
→ Encerrar()

EXERCIDA
→ MarcarExercida()

EXPIRADA
→ MarcarExpirada()

---

# 42. IMPORTADORES AINDA NECESSÁRIOS

Criar:

ImportadorCotacoes

Entrada:

- CotacaoAtivoImportacao
- ativos
- DateTime dataReferencia

Criar CotacaoAtivo.

---

Criar:

ImportadorSaldosDisponiveis

Entrada:

- SaldoDisponivelImportacao
- investidores
- DateTime dataReferencia

Criar SaldoDisponivel.

---

Criar:

ImportadorHistoricosPatrimonio

Entrada:

- HistoricoPatrimonioImportacao
- investidores

Utilizar DataReferencia existente no Excel.

---

# 43. COORDENADOR DA IMPORTAÇÃO

Classe existente:

CoordenadorImportacao

Já possui:

- verificação do banco;
- transação;
- reset;
- importação de cadastros;
- operações;
- proventos;
- opções;
- validação;
- commit;
- rollback.

AINDA NÃO ESTÁ LIGADO AO PROGRAM.CS.

NÃO executar importação real até terminar todos os novos importadores.

Precisa ser ampliado para:

- Cotacoes
- SaldosDisponiveis
- HistoricosPatrimonio

Criar uma única:

DateTime dataReferencia

por execução.

Essa mesma data deve ser usada para:

CotacaoAtivo
SaldoDisponivel

---

# 44. VERIFICADORBANCO DADOS

Classe existente:

VerificadorBancoDados

Atualmente verifica:

- Investidores
- Ativos
- Operacoes
- Proventos
- Opcoes

Precisa futuramente incluir:

- Cotacoes
- SaldosDisponiveis
- HistoricoPatrimonio

---

# 45. PROGRAM.CS DO IMPORTADOR

Estado atual:

MODO SIMULAÇÃO.

Ele:

- recebe caminho do Excel;
- lê Excel;
- valida operações;
- valida classificação;
- valida opções;
- resolve ativo-base;
- mostra pendências;
- verifica banco;
- NÃO altera banco.

Mensagem final:

========================================
 MODO SIMULAÇÃO

 Nenhuma alteração foi realizada no banco.
========================================

Não ligar o CoordenadorImportacao ainda.

---

# 46. VALIDAÇÕES BLOQUEANTES

Atualmente a preparação para importação considera:

- inconsistências históricas;
- ativos sem classificação;
- opções encerradas sem data;
- opções com ativo-base ambíguo;
- opções com ativo-base não resolvido;
- cotações duplicadas;
- saldos duplicados;
- históricos patrimoniais duplicados.

Pendências auxiliares conhecidas do Excel podem ser exibidas sem
necessariamente bloquear a importação.

---

# 47. CARTEIRA / APPLICATION

Existe:

CalcularCarteiraService

Responsabilidade:

calcular posição a partir das operações.

Ordenação:

Data
Sequencia

Agrupamento:

Ativo

Calcula:

- Quantidade
- PrecoMedio
- CustoTotal
- ResultadoRealizado

Venda acima da posição disponível deve ser rejeitada.

---

# 48. DTO DE OPERAÇÃO DA CARTEIRA

OperacaoCarteiraDto:

- AtivoId
- Ticker
- Nome
- TipoOperacao
- Quantidade
- PrecoUnitario
- Taxas
- Data
- Sequencia

---

# 49. DTO DE POSIÇÃO

PosicaoAtivoDto:

- Ticker
- Nome
- Quantidade
- PrecoMedio
- CustoTotal
- ResultadoRealizado

---

# 50. RESPONSABILIDADES DA CARTEIRA

CarteiraRepository:

buscar/projetar dados.

Não deve conter regra de cálculo de carteira.

CalcularCarteiraService:

regra de negócio.

ConsultarCarteiraHandler:

orquestração.

Essa separação é deliberada.

---

# 51. FRONTEND

Stack:

React + TypeScript + Vite.

Objetivo visual:

aproximar gradualmente o sistema do dashboard existente no Power BI.

O frontend já possui uma estrutura visual inicial.

Entretanto, a prioridade atual foi alterada:

ANTES de continuar o frontend, importar corretamente todos os dados reais
do Excel para o SQL Server.

Depois da importação definitiva:

retomar dashboard/frontend.

---

# 52. ERRO FRONTEND JÁ RESOLVIDO

Houve anteriormente aviso/erro relacionado a setDashboard(null)
sincronamente dentro de effect.

O código atual já não possui esse problema.

npm run lint finalizou sem erros.

Visual Studio chegou a mostrar erro antigo na Error List, mas o Build Output
confirmou validação concluída.

Não revisitar isso sem novo erro concreto.

---

# 53. TESTES

Existem projetos:

- Domain.Tests
- Application.Tests
- IntegrationTests

Os testes foram atualizados após inclusão de:

Provento.Descricao

Integration tests ficaram verdes após aplicação das migrations:

- CotacaoAtivo
- SaldoDisponivel
- HistoricoPatrimonio

Um PendingModelChangesWarning ocorreu quando o modelo de CotacaoAtivo
existia mas a migration ainda não havia sido aplicada.

Isso era comportamento correto.

Não suprimir esse warning.

Aplicar a migration correspondente.

---

# 54. BANCO ANTES DA IMPORTAÇÃO REAL

Antes da reconstrução real, o banco possuía dados artificiais de
desenvolvimento.

Investidor:

Silvio

Ativo:

ITUB4

Operações artificiais:

2026-09-04
seq 1
ITUB4
COMPRA
100 @ 40

2026-09-04
seq 2
ITUB4
COMPRA
100 @ 42

2026-09-04
seq 3
ITUB4
VENDA
50 @ 45

Esses dados devem desaparecer quando a importação real reconstruir o banco.

---

# 55. API

Swagger conhecido:

https://localhost:7237/swagger/index.html

---

# 56. ESTADO ATUAL EXATO DO DESENVOLVIMENTO

Última atualização: 2026-09-25.

A primeira fase de importação já foi superada. O sistema atual opera com os
dados persistidos no SQL Server e possui API e frontend funcionais.

A prioridade técnica atual é manter uma única interpretação financeira entre
backend e frontend e garantir consistência após qualquer manutenção dos dados.

Entregas estruturais concluídas nesta fase:

1. Motor financeiro único no backend.
2. Remoção dos principais cálculos financeiros duplicados do frontend.
3. Cobertura de testes para a regra econômica central.
4. Refresh/invalidação global após CRUDs relevantes.
5. Relatório administrativo de reconciliação e integridade.
6. Atualização deste CODEX para refletir o sistema real.

O frontend continua mantendo snapshot local somente como fallback/referência.
Quando a API está disponível, os indicadores financeiros devem vir da API.

---

# 57. REGRA FINANCEIRA PADRÃO

A regra econômica oficial da carteira é:

Resultado da carteira =
Valorização dos ativos em carteira
+ Proventos/dividendos líquidos recebidos
+ Prêmio líquido (ganho) de opções.

A classe central é:

`Investimentos.Application/Dashboard/CalculadoraResultadoCarteira.cs`

Ela concentra:

- cálculo do resultado econômico;
- cálculo do capital-base;
- cálculo da rentabilidade.

Resultado realizado na venda de ações continua sendo calculado e exposto
separadamente em `ResultadoRealizadoAcoes`.

IMPORTANTE:

`ResultadoRealizadoAcoes` NÃO deve ser somado novamente ao Resultado da
Carteira.

Isso evita dupla contagem entre valorização, posição remanescente e resultado
realizado.

No dashboard individual, o resultado operacional de opções atualmente é
utilizado sem rateio de desconto fiscal global.

No consolidado:

`PremioLiquidoOpcoes = OpcoesBrutas - DescontosFiscais`

Os descontos fiscais são globais e devem ser abatidos exatamente uma vez.

O IR estimado de opções é informativo e não substitui os descontos fiscais
efetivamente persistidos.

---

# 58. INDICADORES POR ATIVO

`PosicaoAtivoDto` expõe, além da posição contábil:

- PrecoAtual
- ValorAtual
- Valorizacao
- Proventos
- ResultadoOpcoes
- ResultadoEconomico
- RentabilidadeEconomica
- YieldProventos

As telas não devem reconstruir essas fórmulas.

Para cada ativo:

`ResultadoEconomico = Valorizacao + Proventos + ResultadoOpcoes`

`RentabilidadeEconomica = ResultadoEconomico / CustoTotal * 100`

`YieldProventos = Proventos / CustoTotal * 100`

quando `CustoTotal > 0`.

O consolidado recompõe os componentes por ticker a partir dos dashboards
individuais e mantém a mesma semântica.

Observação importante:

descontos fiscais de opções são globais. Como não existe regra de rateio por
ativo, a soma dos resultados econômicos individuais por ativo pode diferir do
resultado consolidado pelo valor desses descontos. Não criar rateio implícito
sem decisão explícita de negócio.

---

# 59. VALOR ATUAL E ATIVOS SEM COTAÇÃO DE MERCADO

Ativos negociados com cotação usam a última cotação disponível.

Alguns investimentos são marcados por valor patrimonial informado, e não por
cotação:

- PREVIDENCIA / label visual PREV
- CDB NEON
- CDB BTG
- FMP ELETROBRAS

Esses ativos usam o último valor patrimonial persistido por
ativo/investidor.

PREVIDENCIA permanece fora do indicador `ValorAplicado` do Painel, mas
integra o patrimônio estimado.

---

# 60. REFRESH / INVALIDAÇÃO GLOBAL

`frontend/investimentos-web/src/App.tsx` possui o fluxo central:

- `carregarDadosAtuais()`
- `atualizarTudo()`

Após alterações relevantes, o frontend deve invalidar/recarregar os dados
globais através desse fluxo, em vez de manter versões financeiras paralelas.

CRUDs conectados ao refresh global incluem:

- Ativos
- Operações
- Opções
- Proventos
- Entradas/Saídas
- Investidores
- DARF/descontos fiscais

Não reintroduzir:

- `window.location.reload()`;
- contador artificial `versaoDados`;
- recargas parciais que atualizem apenas um dashboard e deixem os demais
  estados financeiros defasados;
- remount forçado de telas como mecanismo de sincronização.

O botão lateral `Atualizar` executa a mesma recarga global contra a API.

---

# 61. RECONCILIAÇÃO E INTEGRIDADE

Existe relatório administrativo em:

`Administração -> Integridade`

Endpoint:

`GET /api/admin/integridade`

Contrato:

`Investimentos.Application/Administracao/Integridade`

Implementação:

`src/Investimentos.Infrastructure/Persistence/Repositories/IntegridadeRepository.cs`

A verificação atual procura:

- venda superior à posição disponível;
- tipo de operação não reconhecido;
- opção com quantidade, strike ou datas incompatíveis;
- opção ENCERRADA sem data de finalização ou preço de recompra;
- finalização de opção anterior à operação;
- provento com quantidade, valor ou datas incompatíveis;
- posição atual sem cotação, quando o ativo depende de cotação;
- cotação duplicada por ativo/data;
- saldo disponível duplicado por investidor/data.

Severidades:

- ERRO
- AVISO

O relatório é diagnóstico. Não deve corrigir dados automaticamente.

Ao encontrar inconsistência, identificar primeiro a origem e corrigir pelo
fluxo de domínio/CRUD apropriado.

---

# 62. OPÇÕES — SEMÂNTICA ATUAL

Status persistidos atualmente relevantes:

- ABERTA
- EXECUTADA
- ENCERRADA
- EXPIRADA

A interface atual cadastra novas opções e as marca imediatamente como
`EXECUTADA`.

No contexto atual da aplicação, `EXECUTADA` representa a operação de opção
registrada/ativa. Não interpretar o nome isoladamente como prova de exercício
da ação-base.

`ENCERRADA` representa fechamento por recompra e exige:

- DataFinalizacao
- PrecoRecompraUnitario

`ValorExecucao > 0` é o sinal histórico utilizado quando existe liquidação
por exercício/atribuição que deve impactar a operação da ação-base.

Na manutenção administrativa:

- dados originais de uma EXECUTADA são preservados;
- EXECUTADA pode ser finalizada como ENCERRADA com os dados de fechamento;
- ENCERRADA mantém edição restrita aos dados de finalização/resultado.

Não voltar a usar `EXERCIDA` como status persistido sem migration e revisão
das regras atuais.

---

# 63. EXERCÍCIO DE OPÇÕES E PREÇO MÉDIO

`CalcularCarteiraService` calcula posição e preço médio a partir das
operações.

Regras principais:

- COMPRA aumenta quantidade e custo;
- VENDA realiza resultado usando o preço médio corrente;
- venda reduz o custo proporcionalmente;
- o preço médio da posição remanescente não é alterado por uma venda;
- venda acima da posição disponível é inválida.

Para opções com `ValorExecucao > 0`, a carteira procura a operação normal de
ação correspondente por ativo, natureza derivada, quantidade e data.

Mapeamento:

- PUT VENDA -> COMPRA
- PUT COMPRA -> VENDA
- CALL VENDA -> VENDA
- CALL COMPRA -> COMPRA

Quando a operação correspondente existe, o preço unitário utilizado no
cálculo da carteira é substituído pelo valor de execução da opção.

Não criar uma segunda operação automaticamente quando não houver
correspondência segura.

---

# 64. PROVENTOS

Tipos internos permitidos:

- DIVIDENDO
- JCP
- RENDIMENTO

Na interface, JCP pode ser apresentado ao usuário como `JUROS`, mas o valor
interno/domínio permanece `JCP`.

`ValorRecebido` é o valor líquido efetivamente recebido e deve ser
preservado.

No cadastro total/rateado, os registros pertencem aos investidores conforme
a participação utilizada no rateio. O consolidado não deve duplicar o valor
original ao somar os registros individuais.

---

# 65. ADMINISTRAÇÃO E NAVEGAÇÃO

Telas de consulta principais:

- Painel
- Carteira
- Ativos/Análise
- Opções
- Proventos

Manutenção fica concentrada em Administração.

Abas administrativas relevantes:

- Ativos
- Operações
- Opções
- Proventos
- Entradas / Saídas
- Investidores
- DARF
- Integridade
- Parâmetros
- Usuários

Padrões visuais já adotados:

- indicadores positivos verdes e negativos vermelhos;
- labels financeiros principais verdes;
- botões Editar/Excluir padronizados;
- evitar scroll horizontal;
- filtro TODOS quando aplicável;
- label visual PREV para previdência.

---

# 66. TESTES E VALIDAÇÃO

A regra econômica central possui testes em
`Investimentos.Application.Tests`.

Coberturas importantes:

- `CalculadoraResultadoCarteiraTests`;
- dashboard individual;
- dashboard consolidado;
- cálculo da carteira e resultado realizado;
- regras atuais de atualização/exclusão de opções;
- YieldProventos calculado no backend.

Após alteração financeira ou estrutural relevante, executar:

```bash
dotnet test Investimentos.Application.Tests/Investimentos.Application.Tests.csproj
```

Frontend:

```bash
cd frontend/investimentos-web
npm run build
```

O build da solução completa pode apresentar o problema histórico MSB4249
relacionado ao projeto Web/Solution. Para validar a API isoladamente:

```bash
cd Investimentos.Api
dotnet build Investimentos.Api.csproj
```

Não tratar MSB4249 como erro da regra financeira sem confirmar o projeto que
falhou.

---

# 67. EXECUÇÃO LOCAL

API:

```bash
cd Investimentos.Api
dotnet run
```

Perfil HTTPS conhecido:

`https://localhost:7237`

HTTP alternativo:

`http://localhost:5001`

Frontend:

```bash
cd frontend/investimentos-web
npm run dev
```

`VITE_API_URL` deve apontar para a API correta. O fallback atual do frontend
é `https://localhost:7237`.

Se necessário:

```bash
dotnet dev-certs https --trust
```

---

# 68. MIGRATIONS

Regras permanentes:

- não editar migrations antigas;
- não editar ModelSnapshot manualmente;
- criar migration nova para alteração real de modelo;
- aplicar migration antes de concluir que o modelo está inconsistente.

O sistema executa `Database.MigrateAsync()` na inicialização da API.

Migrations posteriores às primeiras versões incluem normalização de opções,
descontos fiscais, movimentações financeiras e valor patrimonial por ativo.

Consultar a pasta `src/Investimentos.Infrastructure/Migrations` para a lista
canônica atual, em vez de confiar em uma lista histórica fixa neste documento.

---

# 69. LIMITAÇÕES CONHECIDAS / DECISÕES PENDENTES

1. `RentabilidadeAno` é um nome legado. O cálculo atual representa a
   rentabilidade econômica acumulada com base no resultado e capital-base;
   não é ainda uma performance temporal anual rigorosa.

2. Entradas e saídas são expostas no dashboard, mas ainda não compõem uma
   metodologia temporal como Modified Dietz/TWR/XIRR.

3. O campo/indicador `ValorAplicado` do Painel representa atualmente valor
   de mercado das posições elegíveis, e o nome é legado.

4. Descontos fiscais globais de opções não são rateados por ativo.

5. Histórico/auditoria completa da carteira e performance temporal pertencem
   à próxima fase funcional.

Essas limitações devem ser tratadas explicitamente. Não alterar a semântica
silenciosamente.

---

# 70. FASE FUNCIONAL

## Dashboard avançado de Opções — concluído

A tela de consulta de Opções possui agora indicadores calculados a partir das
métricas fornecidas pelo backend:

- capital comprometido em PUT de venda ativa;
- ações comprometidas em CALL de venda ativa;
- prêmio recebido nas posições ativas, líquido das taxas da operação;
- retorno dos prêmios ativos sobre o capital comprometido em PUT;
- próximos vencimentos;
- exposição a exercício com base na relação entre cotação atual e strike.

Uma opção é considerada ativa para esses indicadores quando a situação é
ABERTA ou EXECUTADA.

Para PUT de venda:

`CapitalComprometidoPut = Strike * Quantidade`

Para CALL de venda:

`AcoesComprometidasCall = Quantidade`

A exposição a exercício é sinalizada quando:

- PUT: preço atual do ativo-base <= strike;
- CALL: preço atual do ativo-base >= strike.

Essa sinalização representa exposição objetiva pelo preço/strike e não uma
previsão de exercício.

As métricas por operação ficam em `OperacaoOpcaoDto`; o React agrega e
apresenta os valores, sem reconstruir a regra de classificação da exposição.

## Próximos itens

1. Metas por ativo.
2. Histórico/auditoria da carteira e metodologia adequada de performance.
3. Evolução fiscal/DARF.

A ordem pode ser revista por decisão explícita de produto.

---

# 71. REFERÊNCIAS PRINCIPAIS DO CÓDIGO

Motor financeiro:

- `Investimentos.Application/Dashboard/CalculadoraResultadoCarteira.cs`
- `Investimentos.Application/Dashboard/ConsultarDashboardHandler.cs`
- `Investimentos.Application/Dashboard/ConsultarDashboardConsolidadoHandler.cs`
- `Investimentos.Application/Carteira/ConsultarCarteira/CalcularCarteiraService.cs`
- `Investimentos.Application/Carteira/ConsultarCarteira/PosicaoAtivoDto.cs`

Refresh frontend:

- `frontend/investimentos-web/src/App.tsx`

Administração:

- `frontend/investimentos-web/src/pages/Administracao/AdministracaoView.tsx`

Integridade:

- `Investimentos.Application/Administracao/Integridade/IntegridadeDto.cs`
- `Investimentos.Application/Administracao/Integridade/IIntegridadeRepository.cs`
- `src/Investimentos.Infrastructure/Persistence/Repositories/IntegridadeRepository.cs`
- `frontend/investimentos-web/src/pages/Administracao/components/IntegridadeTab.tsx`

API:

- `Investimentos.Api/Program.cs`

---

# 72. QUANDO RETOMAR O PROJETO

Antes de propor mudança estrutural:

1. Ler este `CODEX.md`.
2. Conferir o estado atual e as limitações conhecidas.
3. Não reimplementar cálculos no frontend quando o backend já os fornece.
4. Preservar o motor financeiro único.
5. Usar o refresh global após CRUD financeiro.
6. Executar Integridade quando houver suspeita de divergência nos dados.
7. Não alterar migrations antigas nem ModelSnapshot manualmente.
8. Compilar/testar uma unidade coerente antes de avançar.
9. Atualizar este documento quando uma decisão estrutural mudar.

---

# 73. REGRA DE MANUTENÇÃO DO CODEX

Este documento é a referência técnica viva do projeto.

Atualizar a data e as seções afetadas sempre que houver mudança em:

- regra financeira;
- arquitetura de refresh;
- semântica de opções;
- persistência/modelo;
- reconciliação;
- fluxo principal de navegação;
- metodologia de performance;
- roadmap funcional.

Não manter instruções de \"próximo passo\" que já tenham sido concluídas.
O estado atual deve prevalecer sobre registros históricos.
