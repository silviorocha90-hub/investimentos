# Aportiva no localhost — develop

O código e as migrations de main estão incorporados na develop. O perfil
`localhost-integrado` executa a API em `Local`, com banco SQL Server local,
login HTTPS, cotações, IA, e-mail e histórico de IR. O agendamento fica habilitado
nesse perfil. Os perfis antigos `https`/`http` continuam em Development, onde a
IA automática fica desabilitada.

## 1. Atualizar o código no seu computador

Na pasta do projeto, com suas alterações locais já salvas/commitadas:

```text
git switch develop
git pull --ff-only origin develop
```

Requisitos: .NET SDK 10, Node.js 24, SQL Server local e SSMS. Este documento
não instala nem restaura um banco automaticamente no computador do usuário.

## 2. Preservar e trazer os dados

Branches armazenam código/migrations; não armazenam as operações inseridas no
Azure. A exportação contém dados pessoais e financeiros e deve ficar fora do Git.

Se os dados mais recentes estão no Azure:

1. Regularizar o acesso à assinatura/banco, que estava suspenso em 06/11/2026.
2. Pelo SSMS conectado ao Azure SQL, exportar `aportiva`: Tasks > Export Data-tier
   Application, gerando um `.bacpac`. Evitar alterações simultâneas durante a cópia.
3. No SQL Server LOCAL, usar Import Data-tier Application. A importação exige um
   banco novo: se já existir InvestimentosDb, fazer backup pelo SSMS e importar
   primeiro como `InvestimentosDb_Importacao`. Conferir a cópia antes de substituir
   ou renomear qualquer banco. Ao final, o destino usado pelo app deve chamar-se
   `InvestimentosDb`. Não excluir o banco anterior antes da conferência.
4. Executar `database/local/ConferirDados.sql` na origem e na cópia. Comparar
   datas, contagens e saldos por investidor entre origem e cópia.
   Incluir operações, opções, proventos, caixa, usuários, vínculos, relatórios e IR.

Se o SQL local já contém os dados atualizados, fazer um backup `.bak` pelo SSMS
antes de atualizar a estrutura. Não executar o importador Excel para esta migração:
ele reconstrói dados de negócio e não é uma cópia do Azure.

## 3. Atualizar a estrutura do banco local

No SSMS, conectar-se ao servidor LOCAL, selecionar `InvestimentosDb` e executar:

```text
database/local/AtualizarInvestimentosDb.sql
```

O script é idempotente, gerado pelo EF Core 10.0.11, e aplica apenas migrations
ainda ausentes de `__EFMigrationsHistory`. Cada lote bloqueia execução fora de
InvestimentosDb ou em Azure SQL. Inclui as migrations de estimativa/histórico de IR
que estavam em main. Não copia operações nem substitui um backup.
A API também aplica migrations pendentes ao iniciar, após validar o destino local.

Para regenerar o script base (sem as proteções extras do arquivo versionado):

```text
dotnet tool restore
dotnet ef migrations script --idempotent --project src/Investimentos.Infrastructure --startup-project src/Investimentos.Infrastructure --output schema-base.sql
```

Executar SQL pelo SSMS, não por scripts de PowerShell de administração do banco.

## 4. Configurar as credenciais somente no computador

No Visual Studio: botão direito em `Investimentos.Api` > Manage User Secrets.
O perfil Local lê o mesmo UserSecretsId já usado anteriormente em Development.
Use `docs/local/user-secrets.example.json` apenas como referência; substitua os
marcadores no cofre local, nunca em arquivos versionados.

Manter/reutilizar os valores reais de:

| Chave | Uso |
|---|---|
| `ConnectionStrings:InvestimentosDb` | SQL Server local; padrão localhost / InvestimentosDb / autenticação Windows |
| `OPENAI_API_KEY` | Análises por IA |
| `OPENAI_MODEL` | Mesmo modelo que você já usa; omitir preserva o padrão do código |
| `RESEND_API_KEY` | Envio dos relatórios |
| `EMAIL_FROM` | Mesmo remetente/domínio já verificado no Resend |
| `BRAPI_TOKEN` | Atualização de ativos; cobertura depende do plano da brapi |
| `BRAPI_OPCOES_HABILITADAS` | Ativar opções apenas se o plano permitir |
| `TELEGRAM_BOT_TOKEN` | Opcional, se já usa Telegram |

Exemplo para instância SQL Express (somente se for a sua instância):

```json
{
  "ConnectionStrings": {
    "InvestimentosDb": "Server=.\\SQLEXPRESS;Database=InvestimentosDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

O perfil bloqueia conexões remotas/Azure herdadas antes de aplicar migrations.
O launcher define ambos DOTNET_ENVIRONMENT e ASPNETCORE_ENVIRONMENT como Local
para não herdar Production da máquina e permite as origens locais do frontend.
Variáveis do processo e argumentos de linha de comando têm prioridade sobre User
Secrets; corrigir também qualquer ConnectionStrings__InvestimentosDb herdada.

Se restaurou o banco, os usuários e permissões vêm com ele. Para banco novo,
configurar `BootstrapAdmin:Nome`, `BootstrapAdmin:Email`, `BootstrapAdmin:Senha`
no User Secrets. O bootstrap cria o administrador somente se ele ainda não existir.

## 5. Iniciar

Confiar no certificado local uma vez:

```text
dotnet dev-certs https --trust
```

Abrir `iniciar-local.cmd` no Windows. Ele inicia duas janelas:

- API: `https://localhost:7237` com perfil `localhost-integrado`.
- Frontend: `http://localhost:5173` apontando para essa API local.

Alternativa em dois terminais:

```text
dotnet run --project Investimentos.Api --launch-profile localhost-integrado
```

```text
cd frontend/investimentos-web
npm ci
npm run dev -- --host localhost --port 5173 --strictPort
```

O login usa cookie Secure/HttpOnly/Path=/; por isso, use a API HTTPS. As chaves de
proteção são salvas por padrão em `%LOCALAPPDATA%\Aportiva\DataProtection-Keys`,
para manter os cookies após reiniciar a API nesta máquina. Nunca versionar essa pasta.

## 6. Integrações e agenda

Não é necessário domínio público para as chamadas de saída ao Resend, brapi,
OpenAI ou Telegram. É preciso internet, credenciais e plano/cobertura válidos.

- Cotações: ciclo a cada 15 minutos; dias úteis depois das 17h (São Paulo).
- Relatório: sexta depois das 19h; recuperação sábado/domingo depois das 10h,
  mantendo a referência da sexta. Último envio é controlado no banco.
- Histórico de IR: preservação automática em ciclos de 15 minutos.
- Geração/atualização manual: continuam disponíveis na Administração.

Deixar o computador ligado, sem suspensão, com a API em execução nessas janelas.
Fechar o navegador não encerra a API, mas fechar o terminal da API encerra as rotinas.
A geração de IA continua consumindo a API paga. O perfil Local não cria uma nova
cobrança Azure, mas os recursos Azure existentes continuam sujeitos ao contrato.
Executar um único agendador usando este banco para evitar envios em duplicidade.

## 7. Validar antes de encerrar o Azure

- Login e recarregamento da página; reiniciar API e conferir persistência da sessão.
- Conferir patrimônio, caixa e última operação de cada investidor.
- Abrir Administração > Estimativa de IR e consultar anos e histórico.
- Atualizar cotações manualmente e conferir data/status das fontes.
- Gerar/enviar o relatório pelo fluxo de administrador e confirmar entrega no Resend.
- Conferir logs se faltar credencial ou ocorrer erro de conexão.
- Guardar exportação `.bacpac` e backup local fora do repositório.

O localhost só é acessível no próprio computador. Acesso por outros investidores
exige uma solução de rede/acesso remoto, que não é configurada por este perfil.
A mudança de código não exclui nem desativa recursos Azure.

Referências: Microsoft Learn, Export a Database to a BACPAC File; Import/Export
Data from SQL Server and Azure SQL Database; Safe storage of app secrets in development.
