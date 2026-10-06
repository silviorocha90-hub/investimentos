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

## Telegram opcional

```
TELEGRAM_BOT_TOKEN=<token do bot>
TELEGRAM_CHAT_ID=<chat id>
```

Sem essas duas variáveis o relatório continua sendo gerado e salvo, apenas
não é enviado pelo Telegram.

## Frontend

No build do frontend:

```
VITE_API_URL=https://<dominio-da-api>
```

## Comportamento autônomo

O worker verifica periodicamente se já existe relatório para a data local de
São Paulo. Depois da hora configurada, gera no máximo um relatório por dia.
Se o container reiniciar, a existência do relatório no banco impede geração
duplicada.

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
