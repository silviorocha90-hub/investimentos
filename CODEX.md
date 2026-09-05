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

Estamos preparando a PRIMEIRA IMPORTAÇÃO REAL COMPLETA do Excel.

O banco ainda NÃO foi reconstruído com os dados reais.

O Program.cs ainda está em MODO SIMULAÇÃO.

Última atividade:

foi identificado que a linha:

VALOR | Disponível

da planilha Cotação Ativos estava sendo considerada um ativo.

Foi definida alteração no:

LeitorExcelInvestimentos.LerCadastroAtivos()

para ignorar:

Tipo = Disponível
Tipo = Disponivel

O próximo dry-run deve confirmar:

Operações.............. 226
Compras................ 179
Vendas................. 47

Proventos.............. 172

Opções................. 61
PUT.................... 40
CALL................... 21

Cotações............... 19
Duplicadas............. 0

Saldos disponíveis..... 6
Duplicados............. 0

Histórico patrimonial.. 40
Duplicados............. 0

Investidores........... 6

Ativos utilizados...... 50
Classificados.......... 50
Sem classificação...... 0

Ativos-base opções..... 61/61

Encerradas............. 46
Encerradas com data.... 46
Exercidas.............. 15

Histórico de vendas.... válido

A pendência auxiliar da linha 213 pode continuar aparecendo.

Resultado final esperado:

DADOS PRONTOS PARA IMPORTAÇÃO

seguido de:

MODO SIMULAÇÃO

Nenhuma alteração foi realizada no banco.

---

# 57. PRÓXIMOS PASSOS

Executar nesta ordem:

1. Rebuild após correção do leitor.
2. Executar dry-run.
3. Confirmar 50/50 ativos classificados.
4. Confirmar 19 cotações.
5. Confirmar 6 saldos.
6. Confirmar 40 históricos.
7. Criar ImportadorCotacoes.
8. Compilar.
9. Criar ImportadorSaldosDisponiveis.
10. Compilar.
11. Criar ImportadorHistoricosPatrimonio.
12. Compilar.
13. Atualizar ResetBancoDados.
14. Compilar.
15. Atualizar VerificadorBancoDados.
16. Compilar.
17. Atualizar CoordenadorImportacao.
18. Compilar.
19. Validar transaction/reset/load completo.
20. Somente então alterar Program.cs para importação real.
21. Executar primeira reconstrução real do InvestimentosDb.
22. Comparar Excel x SQL.
23. Rodar testes.
24. Retomar API/dashboard/frontend.

---

# 58. CONTAGENS ESPERADAS NA PRIMEIRA IMPORTAÇÃO

Excel:

Investidores:
6

Operações:
226

Proventos:
172

Opções:
61

Cotações:
19

Saldos disponíveis:
6

Histórico patrimonial:
40

Ativos:
aproximadamente 50, considerando o conjunto necessário das fontes e
ativos-base das opções.

O número final de ativos deve ser validado pelo próprio importador,
não hardcoded como regra de domínio.

---

# 59. PRINCÍPIO PARA O FUTURO

O Excel é fonte de verdade APENAS nesta fase.

A arquitetura não deve assumir permanentemente que dados sempre virão
do Excel.

Por isso:

LeitorExcelInvestimentos
→ transforma Excel em dados de importação.

Importadores
→ transformam dados de importação em entidades.

Domain
→ não conhece Excel.

Infrastructure
→ não conhece regras específicas das células do Excel.

Application
→ não deve depender do formato físico da planilha.

Essa separação deve ser preservada.

---

# 60. QUANDO RETOMAR O PROJETO EM OUTRA CONVERSA

Antes de propor qualquer alteração:

1. Ler este CODEX.md.
2. Identificar o "Estado atual exato do desenvolvimento".
3. Não reconstruir decisões já tomadas.
4. Não alterar arquitetura validada sem motivo concreto.
5. Pedir arquivo atual somente quando ele realmente for necessário.
6. Sempre devolver arquivos completos quando houver alteração.
7. Prosseguir a partir do próximo passo registrado neste documento.

---

# 61. REFERÊNCIAS UTILIZADAS PELO FRONTEND

Última atualização deste registro: 2026-09-05.

## Instruções do projeto

- `C:\Silvio\Projetos\Investimentos\CODEX.md`
  - Documento de contexto, regras de arquitetura e etapas do projeto.

## Fontes de dados e referência visual

- `C:\Silvio\Investimentos\Investimentos.xlsx`
  - Fonte dos dados usados no snapshot local do frontend: carteira,
    rentabilidade, saldos disponíveis, cotações, proventos, opções,
    resultado mensal e IRPF.
- `C:\Silvio\Investimentos\CarteiraInvestimentos.pbix`
  - Referência visual e funcional do dashboard original, incluindo os
    indicadores, tabelas e agrupamentos exibidos no Power BI.

## Implementação frontend

- `C:\Silvio\Projetos\Investimentos\frontend\investimentos-web\src\App.tsx`
  - Composição da tela, filtros, integração com a API e fallback para o
    snapshot local.
- `C:\Silvio\Projetos\Investimentos\frontend\investimentos-web\src\App.css`
  - Estilos e layout responsivo do dashboard.
- `C:\Silvio\Projetos\Investimentos\frontend\investimentos-web\src\index.css`
  - Estilos globais e tipografia.
- `C:\Silvio\Projetos\Investimentos\frontend\investimentos-web\src\data\dashboardSnapshot.ts`
  - Snapshot legado mantido apenas para referência durante a transição; não
    é fonte dos indicadores consolidados quando a API está disponível.

## Integração consolidada com o banco

- `C:\Silvio\Projetos\Investimentos\Investimentos.Api\Program.cs`
  - Expõe `GET /api/dashboard` para o painel consolidado.
- `C:\Silvio\Projetos\Investimentos\Investimentos.Application\Dashboard\ConsultarDashboardConsolidadoHandler.cs`
  - Consolida carteira, proventos e opções de todos os investidores
    cadastrados no banco.
- `C:\Silvio\Projetos\Investimentos\Investimentos.Application\Interfaces\ISaldoDisponivelRepository.cs`
- `C:\Silvio\Projetos\Investimentos\Investimentos.Application\Interfaces\IHistoricoPatrimonioRepository.cs`
  - Contratos para consultar os últimos saldos e patrimônios por investidor.
- `C:\Silvio\Projetos\Investimentos\src\Investimentos.Infrastructure\Persistence\Repositories\SaldoDisponivelRepository.cs`
- `C:\Silvio\Projetos\Investimentos\src\Investimentos.Infrastructure\Persistence\Repositories\HistoricoPatrimonioRepository.cs`
  - Consultas dos dados consolidados nas tabelas do banco.
- `C:\Silvio\Projetos\Investimentos\src\Investimentos.Infrastructure\Persistence\Repositories\CotacaoAtivoRepository.cs`
  - Consulta a última cotação por ativo para calcular o valor aplicado.
- `C:\Silvio\Projetos\Investimentos\Investimentos.Application\Interfaces\IHistoricoPatrimonioRepository.cs`
  - Contrato do histórico usado pelo endpoint de evolução.
- `GET /api/dashboard/evolucao`
  - Retorna uma série histórica do banco para cada investidor.

## Normalização de opções

- `EXECUTADA` substitui `EXERCIDA` como status de execução.
- `ENCERRADA` permanece como status de fechamento por recompra.
- `C:\Silvio\Projetos\Investimentos\src\Investimentos.Infrastructure\Migrations\20260905160000_NormalizarStatusOpcoes.cs`
  - Migration criada para normalizar registros antigos.
- A correção foi aplicada diretamente no banco `InvestimentosDb`, resultando
  em 15 opções `EXECUTADA` e 46 opções `ENCERRADA`.

## Regra do valor aplicado


Para cada posição consolidada com quantidade positiva:

`Valor aplicado = Quantidade atual × Última cotação do ativo`

O total é a soma dessa operação para todos os investidores e ativos.
O ativo `PREV` fica fora deste indicador e aparece somente na carteira.

## Regra do resultado realizado do painel

- Resultado consolidado: premio liquido de opcoes + proventos e dividendos
  + valorizacao das posicoes ativas contra o custo medio remanescente.
- A valorizacao somente considera ativos cuja primeira compra ocorreu ha mais
  de um mes; compras recentes continuam no valor aplicado, mas nao geram
  valorizacao no resultado.
- `PREV` fica restrito a carteira e nao entra na valorizacao ou no resultado.

- O premio liquido de opcoes usa o `ResultadoInformado`/`MyProfit` das opcoes
  encerradas, abatido pelos lancamentos persistidos em `DescontoFiscal` dos
  tipos `DARF`, `SPRAD` e `DARF_SPRAD`. Os valores agregados da aba Resultado
  Mensal foram carregados como `DARF_SPRAD` para o investidor Silvio.

- O painel exibe o premio liquido de opcoes no bloco superior, com barras
  verticais por investidor e o total consolidado vindo da API.

- Opções: soma de `ResultadoInformado` (origem `MyProfit`) somente para
  opções com situação `ENCERRADA`.
- Ativos: soma de `Quantidade líquida × Última cotação - Custo médio
  remanescente` apenas para posições com quantidade líquida maior que zero.
- O banco possui 46 opções encerradas com `ResultadoInformado` preenchido.

## Regra de manutenção

Sempre que uma nova fonte for usada ou um arquivo relevante do frontend for
alterado, atualizar esta seção com o caminho, a finalidade e a data da
alteração. As referências não substituem as instruções técnicas deste
documento.
