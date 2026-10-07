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

## Estado real da publicação — 07/10/2026

Arquitetura de produção definida:

```text
aportiva.com.br
      |
      v
Azure Static Web Apps Free (aportiva-web)
      |
      v
Azure Container Apps Consumption (aportiva-api)
      |
      v
Azure SQL (aportiva)
```

Recursos:

- Resource Group: `rg-aportiva-prod`
- Frontend: `aportiva-web`
- Frontend Azure: `https://ambitious-grass-0c596d70f.5.azurestaticapps.net`
- API: `aportiva-api`
- Container Apps Environment: `managedEnvironment-rgaportivaprod-a57a`
- Região da API: Brazil South
- SQL Server: `sql-aportiva-prod.database.windows.net`
- Banco: `aportiva`

O frontend está publicado e o workflow do Static Web Apps está funcional.

A API ainda não está publicada corretamente. A revisão inicial usa uma imagem
temporária da Microsoft e falhou por incompatibilidade de porta. O workflow
automático criado pelo portal também falhou e precisa ser corrigido para usar o
Dockerfile real da API.

O log do GitHub Actions mostrou entradas inválidas geradas no YAML do portal e
uso de Cloud Build a partir da raiz do repositório. Não tratar esse workflow
como configuração final.

### Ordem para continuar

1. Corrigir o GitHub Actions do `aportiva-api`.
2. Usar explicitamente o Dockerfile `Investimentos.Api/Dockerfile`.
3. Publicar uma nova revisão saudável.
4. Confirmar a URL pública e um endpoint simples da API.
5. Configurar `ConnectionStrings__InvestimentosDb` e demais secrets no
   Container App.
6. Aplicar/validar migrations no Azure SQL.
7. Configurar `FRONTEND_ORIGINS` com o endereço do Static Web App e domínio.
8. Configurar `VITE_API_URL` no frontend e republicar.
9. Finalizar o domínio `aportiva.com.br`.
10. Fazer teste ponta a ponta de login, carteira, opções, proventos e Assistente IA.

### Domínio

A propriedade de `aportiva.com.br` foi iniciada no Azure Static Web Apps por
TXT no Registro.br. O DNS está em modo avançado e não aceita `@` como nome.

Não criar CNAME de apex com `@` no Registro.br. Depois da validação de
propriedade, resolver o roteamento do domínio raiz separadamente. Se necessário,
usar `www` com CNAME e redirecionamento do apex, ou um DNS com CNAME flattening.

### Segurança

Nunca colocar em GitHub:

- senha do Azure SQL;
- connection string de produção com senha;
- `OPENAI_API_KEY`;
- `RESEND_API_KEY`;
- `BRAPI_TOKEN`;
- senha de BootstrapAdmin;
- token de Telegram ou outra mensageria.

Esses valores devem ser configurados como secrets/variáveis do ambiente Azure.

