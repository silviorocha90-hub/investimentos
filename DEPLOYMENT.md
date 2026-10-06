# Deploy econômico — Investimentos + Assistente IA

A aplicação está preparada para executar sem depender do computador local.

## Serviços

- API .NET: container em `Investimentos.Api/Dockerfile`.
- Frontend React: container em `frontend/investimentos-web/Dockerfile`.
- Banco: SQL Server/Azure SQL compatível com a connection string atual.
- Assistente IA: executado dentro da API por `AssistenteIaWorker`.

## Variáveis obrigatórias da API

```
ConnectionStrings__InvestimentosDb=<connection string SQL Server>
OPENAI_API_KEY=<chave da OpenAI>
OPENAI_MODEL=gpt-6-luna
ASSISTENTE_IA_ATIVO=true
ASSISTENTE_IA_HORA=19
FRONTEND_ORIGINS=https://<dominio-do-frontend>
```

## Envio de relatórios por e-mail e Telegram

O e-mail é enviado pelo Resend para o e-mail cadastrado do usuário. O Telegram
é enviado quando o usuário possui um Chat ID configurado.

```
RESEND_API_KEY=<chave da Resend>
EMAIL_FROM=Investimentos <relatorios@seu-dominio.com>
TELEGRAM_BOT_TOKEN=<token do bot>
```

Não existe configuração do WhatsApp. O administrador recebe a visão consolidada
`TODOS`; usuários comuns recebem apenas os relatórios `INVESTIDOR` das
carteiras às quais possuem acesso.

## Frontend

No build do frontend:

```
VITE_API_URL=https://<dominio-da-api>
```

## Comportamento autônomo

O worker verifica periodicamente os relatórios pela data local de São Paulo. Depois da hora configurada, gera a visão consolidada `TODOS` para o administrador e processa os investidores habilitados conforme a frequência cadastrada: diário, semanal, quinzenal ou mensal. O histórico no banco impede envios duplicados antes do próximo período.

O botão "Gerar análise agora" força nova geração e substitui o relatório do
dia.

## Segurança

Nunca commitar chaves da OpenAI, Telegram ou connection strings de produção.
Configure-as como secrets/variables no provedor de nuvem.

## Hospedagem sugerida

Para manter o custo baixo, hospedar API e frontend em containers e usar um
SQL Server/Azure SQL gerenciado. O projeto não depende de tarefas agendadas
do provedor: o próprio worker da API executa a rotina diária, desde que o
serviço permaneça ativo.


## Dados de mercado (brapi)

O Assistente IA atualiza as cotações dos ativos da carteira antes de gerar o
relatório diário.

Configuração opcional/recomendada:

- `BRAPI_TOKEN`: token da brapi.dev.

Sem token, somente os tickers liberados pelo sandbox da brapi serão
atualizados. A cotação EOD de opções exige plano Pro para séries fora do
sandbox. Quando uma cotação de opção não estiver disponível, o relatório
continua usando preço do ativo-base, strike e vencimento e não inventa o
preço da opção.
