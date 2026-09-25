using Investimentos.Api.Endpoints;
using Investimentos.Api.ExceptionHandling;
using Investimentos.Api.Startup;
using Investimentos.Application.Administracao;
using Investimentos.Application.Administracao.Integridade;
using Investimentos.Application.Metas;
using Investimentos.Application.Ativos.CadastrarAtivo;
using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Dashboard;
using Investimentos.Application.Interfaces;
using Investimentos.Application.Fiscal;
using Investimentos.Application.Investidores.CadastrarInvestidor;
using Investimentos.Application.Investidores.ListarInvestidores;
using Investimentos.Application.Performance;
using Investimentos.Application.Opcoes.AtualizarOperacaoOpcao;
using Investimentos.Application.Opcoes.CadastrarOperacaoOpcao;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Opcoes.ExcluirOperacaoOpcao;
using Investimentos.Application.Operacoes.AtualizarOperacao;
using Investimentos.Application.Operacoes.CadastrarOperacao;
using Investimentos.Application.Operacoes.ExcluirOperacao;
using Investimentos.Application.Proventos.CadastrarProvento;
using Investimentos.Application.Proventos.ConsultarProventos;
using Investimentos.Application.Usuarios.Administracao;
using Investimentos.Application.Usuarios.Autenticacao;
using Investimentos.Domain.Entities;
using Investimentos.Infrastructure;
using Investimentos.Infrastructure.Persistence;
using Investimentos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddScoped<
    AutenticacaoService>();

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults
            .AuthenticationScheme)
    .AddCookie(
        options =>
        {
            options.Cookie.Name =
                "investimentos.auth";

            options.Cookie.HttpOnly =
                true;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.Cookie.SameSite =
                SameSiteMode.None;

            options.ExpireTimeSpan =
                TimeSpan.FromHours(8);

            options.SlidingExpiration =
                true;

            options.Events.OnRedirectToLogin =
                context =>
                {
                    context.Response.StatusCode =
                        StatusCodes
                            .Status401Unauthorized;

                    return Task.CompletedTask;
                };

            options.Events.OnRedirectToAccessDenied =
                context =>
                {
                    context.Response.StatusCode =
                        StatusCodes
                            .Status403Forbidden;

                    return Task.CompletedTask;
                };
        });

builder.Services.AddAuthorization();

builder.Services.AddScoped<CadastrarInvestidorHandler>();
builder.Services.AddScoped<ListarInvestidoresHandler>();
builder.Services.AddScoped<CadastrarAtivoHandler>();

builder.Services.AddScoped<CadastrarOperacaoHandler>();
builder.Services.AddScoped<AtualizarOperacaoHandler>();
builder.Services.AddScoped<ExcluirOperacaoHandler>();

builder.Services.AddScoped<ConsultarCarteiraHandler>();
builder.Services.AddScoped<CalcularCarteiraService>();

builder.Services.AddScoped<CadastrarProventoHandler>();
builder.Services.AddScoped<ConsultarProventosHandler>();

builder.Services.AddScoped<CadastrarOperacaoOpcaoHandler>();
builder.Services.AddScoped<ConsultarOpcoesHandler>();
builder.Services.AddScoped<AtualizarOperacaoOpcaoHandler>();
builder.Services.AddScoped<ExcluirOperacaoOpcaoHandler>();

builder.Services.AddScoped<
    IOperacaoOpcaoCrudRepository,
    OperacaoOpcaoCrudRepository>();

builder.Services.AddScoped<IMovimentacaoFinanceiraRepository, MovimentacaoFinanceiraRepository>();

builder.Services.AddScoped<ConsultarDashboardHandler>();
builder.Services.AddScoped<ConsultarDashboardConsolidadoHandler>();
builder.Services.AddScoped<ConsultarPerformanceCarteiraService>();
builder.Services.AddScoped<IFiscalRepository, FiscalRepository>();

builder.Services.AddScoped<AdministrarUsuariosService>();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:5174")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<
    IAdministracaoRepository,
    AdministracaoRepository>();

builder.Services.AddScoped<
    IInvestidorAdministracaoRepository,
    InvestidorAdministracaoRepository>();

builder.Services.AddScoped<
    IIntegridadeRepository,
    IntegridadeRepository>();

builder.Services.AddScoped<
    IMetaAtivoRepository,
    MetaAtivoRepository>();

var app = builder.Build();

/*
 * Mantém o banco local sincronizado com as migrations antes
 * de qualquer serviço consultar as novas estruturas.
 *
 * Isso evita a API iniciar com o modelo novo apontando para
 * um banco ainda sem a tabela correspondente.
 */
using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<
            InvestimentosDbContext>();

    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapPost(
    "/api/investidores",
    async (
        CadastrarInvestidorRequest request,
        CadastrarInvestidorHandler handler,
        CancellationToken cancellationToken) =>
    {
        var command =
            new CadastrarInvestidorCommand(
                request.Nome);

        var id =
            await handler.HandleAsync(
                command,
                cancellationToken);

        return Results.Created(
            $"/api/investidores/{id}",
            new { id });
    });

app.MapGet(
    "/api/investidores",
    async (
        ListarInvestidoresHandler handler,
        CancellationToken cancellationToken) =>
    {
        var resultado =
            await handler.HandleAsync(
                cancellationToken);

        return Results.Ok(resultado);
    });

app.MapGet(
    "/api/movimentacoes-financeiras",
    async (
        InvestimentosDbContext context,
        int? ano,
        Guid? investidorId,
        CancellationToken cancellationToken) =>
    {
        var anoConsulta = ano ?? DateTime.Today.Year;
        var inicio = new DateTime(anoConsulta, 1, 1);
        var fim = inicio.AddYears(1);

        var query = context.MovimentacoesFinanceiras
            .AsNoTracking()
            .Include(x => x.Investidor)
            .Where(x => x.Data >= inicio && x.Data < fim);

        if (investidorId.HasValue)
            query = query.Where(x => x.InvestidorId == investidorId.Value);

        var itens = await query
            .OrderByDescending(x => x.Data)
            .Select(x => new
            {
                x.Id,
                x.InvestidorId,
                Investidor = x.Investidor.Nome,
                x.Data,
                x.Tipo,
                x.Valor,
                x.Descricao
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(itens);
    });

app.MapPost(
    "/api/movimentacoes-financeiras",
    async (
        CriarMovimentacaoFinanceiraRequest request,
        InvestimentosDbContext context,
        CancellationToken cancellationToken) =>
    {
        var investidor = await context.Investidores
            .FirstOrDefaultAsync(
                x => x.Id == request.InvestidorId,
                cancellationToken);

        if (investidor is null)
            return Results.NotFound(new { detail = "Investidor não encontrado." });

        try
        {
            var movimentacao = new MovimentacaoFinanceira(
                investidor,
                request.Data,
                request.Tipo.Trim().ToUpperInvariant(),
                request.Valor,
                request.Descricao);

            context.MovimentacoesFinanceiras.Add(movimentacao);
            await context.SaveChangesAsync(cancellationToken);

            return Results.Created(
                $"/api/movimentacoes-financeiras/{movimentacao.Id}",
                new { movimentacao.Id });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { detail = ex.Message });
        }
    });

app.MapPut(
    "/api/movimentacoes-financeiras/{id:guid}",
    async (
        Guid id,
        AtualizarMovimentacaoFinanceiraRequest request,
        InvestimentosDbContext context,
        CancellationToken cancellationToken) =>
    {
        var movimentacao = await context.MovimentacoesFinanceiras
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (movimentacao is null)
            return Results.NotFound(new { detail = "Movimentação não encontrada." });

        var investidor = await context.Investidores
            .FirstOrDefaultAsync(
                x => x.Id == request.InvestidorId,
                cancellationToken);

        if (investidor is null)
            return Results.NotFound(new { detail = "Investidor não encontrado." });

        try
        {
            movimentacao.Atualizar(
                investidor,
                request.Data,
                request.Tipo,
                request.Valor,
                request.Descricao);

            await context.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { detail = ex.Message });
        }
    });

app.MapDelete(
    "/api/movimentacoes-financeiras/{id:guid}",
    async (
        Guid id,
        InvestimentosDbContext context,
        CancellationToken cancellationToken) =>
    {
        var movimentacao = await context.MovimentacoesFinanceiras
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (movimentacao is null)
            return Results.NotFound();

        context.MovimentacoesFinanceiras.Remove(movimentacao);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    });

app.MapPost(
    "/api/ativos",
    async (
        CadastrarAtivoRequest request,
        CadastrarAtivoHandler handler,
        CancellationToken cancellationToken) =>
    {
        var command =
            new CadastrarAtivoCommand(
                request.Ticker,
                request.Nome,
                request.TipoAtivoCodigo);

        var id =
            await handler.HandleAsync(
                command,
                cancellationToken);

        return Results.Created(
            $"/api/ativos/{id}",
            new { id });
    });

app.MapPost(
    "/api/operacoes",
    async (
        CadastrarOperacaoRequest request,
        CadastrarOperacaoHandler handler,
        CancellationToken cancellationToken) =>
    {
        var command =
            new CadastrarOperacaoCommand(
                request.Data,
                request.InvestidorId,
                request.Ticker,
                request.TipoOperacaoCodigo,
                request.Quantidade,
                request.PrecoUnitario,
                request.Taxas);

        var id =
            await handler.HandleAsync(
                command,
                cancellationToken);

        return Results.Created(
            $"/api/operacoes/{id}",
            new { id });
    });

app.MapGet(
    "/api/operacoes/{investidorId:guid}",
    async (
        Guid investidorId,
        ICarteiraRepository repository,
        CancellationToken cancellationToken) =>
    {
        var resultado =
            await repository.ObterOperacoesAsync(
                investidorId,
                cancellationToken);

        return Results.Ok(resultado);
    });

app.MapPut(
    "/api/operacoes/{id:guid}",
    async (
        Guid id,
        AtualizarOperacaoRequest request,
        AtualizarOperacaoHandler handler,
        CancellationToken cancellationToken) =>
    {
        await handler.HandleAsync(
            new AtualizarOperacaoCommand(
                id,
                request.Data,
                request.Quantidade,
                request.PrecoUnitario,
                request.Taxas),
            cancellationToken);

        return Results.NoContent();
    });

app.MapDelete(
    "/api/operacoes/{id:guid}",
    async (
        Guid id,
        ExcluirOperacaoHandler handler,
        CancellationToken cancellationToken) =>
    {
        await handler.HandleAsync(
            id,
            cancellationToken);

        return Results.NoContent();
    });

app.MapGet(
    "/api/carteira/{investidorId:guid}",
    async (
        Guid investidorId,
        ConsultarCarteiraHandler handler,
        CancellationToken cancellationToken) =>
    {
        var resultado =
            await handler.HandleAsync(
                investidorId,
                cancellationToken);

        return Results.Ok(resultado);
    });

app.MapPost(
    "/api/proventos",
    async (
        CadastrarProventoRequest request,
        CadastrarProventoHandler handler,
        CancellationToken cancellationToken) =>
    {
        var command =
            new CadastrarProventoCommand(
                request.InvestidorId,
                request.Ticker,
                request.Tipo,
                request.DataCom,
                request.DataPagamento,
                request.QuantidadeBase,
                request.ValorPorUnidade,
                request.ValorRecebido,
                request.Descricao);

        var id =
            await handler.HandleAsync(
                command,
                cancellationToken);

        return Results.Created(
            $"/api/proventos/{id}",
            new { id });
    });

app.MapGet(
    "/api/proventos/{investidorId:guid}",
    async (
        Guid investidorId,
        ConsultarProventosHandler handler,
        CancellationToken cancellationToken) =>
    {
        var resultado =
            await handler.HandleAsync(
                investidorId,
                cancellationToken);

        return Results.Ok(resultado);
    });


app.MapPost(
    "/api/proventos/ratear",
    async (
        SalvarProventoTotalRequest request,
        IProventoRepository repository,
        InvestimentosDbContext context,
        CancellationToken cancellationToken) =>
    {
        if (request.ValorTotal <= 0)
        {
            return Results.BadRequest(
                new { mensagem = "O valor total deve ser maior que zero." });
        }

        var ativo =
            await repository.ObterAtivoPorTickerAsync(
                request.Ticker,
                cancellationToken);

        if (ativo is null)
        {
            return Results.NotFound(
                new { mensagem = "Ativo não encontrado." });
        }

        var dataBase =
            (request.DataCom ?? request.DataPagamento).Date;

        var operacoesAtivo =
            await context.Operacoes
                .AsNoTracking()
                .Where(x =>
                    x.AtivoId == ativo.Id &&
                    x.Data.Date <= dataBase)
                .Select(x => new
                {
                    x.InvestidorId,
                    Tipo = x.TipoOperacao.Codigo,
                    x.Quantidade
                })
                .ToListAsync(cancellationToken);

        var posicoes =
            operacoesAtivo
                .GroupBy(x => x.InvestidorId)
                .Select(grupo => new
                {
                    InvestidorId = grupo.Key,
                    Quantidade = grupo.Sum(x =>
                        x.Tipo == "COMPRA"
                            ? x.Quantidade
                            : x.Tipo == "VENDA"
                                ? -x.Quantidade
                                : 0)
                })
                .Where(x => x.Quantidade > 0)
                .ToList();

        var quantidadeTotal =
            posicoes.Sum(x => x.Quantidade);

        if (quantidadeTotal <= 0)
        {
            return Results.BadRequest(
                new
                {
                    mensagem =
                        $"Não existem posições de {request.Ticker.ToUpperInvariant()} na data-base {dataBase:dd/MM/yyyy}."
                });
        }

        var investidoresRateio =
            await context.Investidores
                .Where(x =>
                    posicoes.Select(p => p.InvestidorId)
                        .Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    cancellationToken);

        var valorUnitario =
            request.ValorTotal /
            quantidadeTotal;

        decimal distribuido = 0;

        for (var indice = 0;
             indice < posicoes.Count;
             indice++)
        {
            var posicao =
                posicoes[indice];

            var valorRecebido =
                indice == posicoes.Count - 1
                    ? request.ValorTotal - distribuido
                    : Math.Round(
                        request.ValorTotal *
                        posicao.Quantidade /
                        quantidadeTotal,
                        2,
                        MidpointRounding.AwayFromZero);

            distribuido +=
                valorRecebido;

            context.Proventos.Add(
                new Provento(
                    investidoresRateio[posicao.InvestidorId],
                    ativo,
                    request.Tipo,
                    request.Descricao,
                    request.DataCom,
                    request.DataPagamento,
                    posicao.Quantidade,
                    valorUnitario,
                    valorRecebido));
        }

        await context.SaveChangesAsync(
            cancellationToken);

        return Results.Ok(
            new
            {
                quantidadeTotal,
                valorUnitario,
                investidores =
                    posicoes.Count,
                valorTotal =
                    request.ValorTotal
            });
    });

app.MapPut(
    "/api/proventos/{id:guid}",
    async (
        Guid id,
        AtualizarProventoRequest request,
        IProventoRepository repository,
        CancellationToken cancellationToken) =>
    {
        var provento =
            await repository.ObterPorIdAsync(
                id,
                cancellationToken);

        if (provento is null)
        {
            return Results.NotFound(
                new { mensagem = "Provento não encontrado." });
        }

        var ativo =
            await repository.ObterAtivoPorTickerAsync(
                request.Ticker,
                cancellationToken);

        if (ativo is null)
        {
            return Results.NotFound(
                new { mensagem = "Ativo não encontrado." });
        }

        provento.Atualizar(
            ativo,
            request.Tipo,
            request.Descricao,
            request.DataCom,
            request.DataPagamento,
            request.QuantidadeBase,
            request.ValorPorUnidade,
            request.ValorRecebido);

        await repository.SalvarAlteracoesAsync(
            cancellationToken);

        return Results.NoContent();
    });

app.MapDelete(
    "/api/proventos/{id:guid}",
    async (
        Guid id,
        IProventoRepository repository,
        CancellationToken cancellationToken) =>
    {
        var provento =
            await repository.ObterPorIdAsync(
                id,
                cancellationToken);

        if (provento is null)
        {
            return Results.NotFound(
                new { mensagem = "Provento não encontrado." });
        }

        repository.Excluir(
            provento);

        await repository.SalvarAlteracoesAsync(
            cancellationToken);

        return Results.NoContent();
    });

/*
 * OPÇÕES
 *
 * A interface trabalha somente com o ticker da opção.
 *
 * O ativo-base é identificado automaticamente utilizando
 * as associações já existentes no histórico do investidor.
 *
 * Toda nova opção cadastrada pela interface nasce
 * diretamente como EXECUTADA.
 */
app.MapPost(
    "/api/opcoes",
    async (
        CadastrarOperacaoOpcaoRequest request,
        CadastrarOperacaoOpcaoHandler cadastrarHandler,
        IOperacaoOpcaoRepository repository,
        IOperacaoOpcaoCrudRepository crudRepository,
        CancellationToken cancellationToken) =>
    {
        if (request.InvestidorId == Guid.Empty)
        {
            return Results.BadRequest(
                new
                {
                    detail =
                        "O investidor é obrigatório."
                });
        }

        if (string.IsNullOrWhiteSpace(
                request.Ticker))
        {
            return Results.BadRequest(
                new
                {
                    detail =
                        "O ticker da opção é obrigatório."
                });
        }

        var tickerOpcao =
            request.Ticker
                .Trim()
                .ToUpperInvariant();

        var raizOpcao =
            ObterRaizTickerOpcao(
                tickerOpcao);

        if (string.IsNullOrWhiteSpace(
                raizOpcao))
        {
            return Results.BadRequest(
                new
                {
                    detail =
                        "Não foi possível identificar a raiz do ticker da opção."
                });
        }

        var historico =
            await repository.ListarAsync(
                request.InvestidorId,
                cancellationToken);

        /*
         * Primeiro procuramos opções anteriores cuja raiz
         * seja igual à nova opção.
         *
         * Exemplos:
         *
         * ITUBU407 -> ITUB
         * ITUBW13  -> ITUB
         * VALEM631 -> VALE
         */
        var ativosAssociados =
            historico
                .Where(
                    x =>
                        string.Equals(
                            ObterRaizTickerOpcao(
                                x.TickerOpcao),
                            raizOpcao,
                            StringComparison.OrdinalIgnoreCase))
                .Select(
                    x =>
                        x.Ativo.Ticker.Codigo)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        if (ativosAssociados.Count == 0)
        {
            return Results.BadRequest(
                new
                {
                    detail =
                        $"Não foi possível identificar automaticamente o ativo-base da opção {tickerOpcao}. Não existe associação histórica para a raiz {raizOpcao}."
                });
        }

        if (ativosAssociados.Count > 1)
        {
            return Results.BadRequest(
                new
                {
                    detail =
                        $"A raiz {raizOpcao} possui mais de um ativo-base associado. Não é possível escolher automaticamente com segurança."
                });
        }

        var tickerAtivo =
            ativosAssociados[0];

        /*
         * Contratos deixa de ser um dado solicitado
         * pela interface.
         *
         * Mantemos 1 para compatibilidade com o modelo
         * persistido atual. Todos os cálculos financeiros
         * utilizam Quantidade.
         *
         * Taxas também deixam de ser informadas na nova
         * interface e passam a zero nas novas inclusões.
         */
        const int contratos = 1;
        const decimal taxas = 0m;

        var command =
            new CadastrarOperacaoOpcaoCommand(
                request.InvestidorId,
                tickerAtivo,
                tickerOpcao,
                request.TipoOpcao,
                request.Natureza,
                request.DataOperacao,
                request.Vencimento,
                request.Strike,
                contratos,
                request.Quantidade,
                request.PremioUnitario,
                taxas);

        var id =
            await cadastrarHandler.HandleAsync(
                command,
                cancellationToken);

        /*
         * O domínio mantém o construtor original criando
         * a entidade como ABERTA para compatibilidade.
         *
         * A regra da nova interface determina que toda
         * inclusão seja imediatamente EXECUTADA.
         */
        var operacao =
            await crudRepository.ObterPorIdAsync(
                id,
                cancellationToken);

        if (operacao is null)
        {
            throw new InvalidOperationException(
                "A opção foi cadastrada, mas não pôde ser recuperada para execução.");
        }

        operacao.MarcarExercida();

        await crudRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return Results.Created(
            $"/api/opcoes/{id}",
            new
            {
                id,
                situacao = "EXECUTADA"
            });
    });

app.MapGet(
    "/api/opcoes/{investidorId:guid}",
    async (
        Guid investidorId,
        ConsultarOpcoesHandler handler,
        CancellationToken cancellationToken) =>
    {
        var resultado =
            await handler.HandleAsync(
                investidorId,
                cancellationToken);

        return Results.Ok(resultado);
    });

app.MapPut(
    "/api/opcoes/{id:guid}",
    async (
        Guid id,
        AtualizarOperacaoOpcaoRequest request,
        AtualizarOperacaoOpcaoHandler handler,
        CancellationToken cancellationToken) =>
    {
        var command =
            new AtualizarOperacaoOpcaoCommand(
                id,
                request.DataOperacao,
                request.Vencimento,
                request.Strike,
                request.Contratos,
                request.Quantidade,
                request.PremioUnitario,
                0m,
                request.Situacao,
                request.DataFinalizacao,
                request.PrecoRecompraUnitario,
                request.ValorExecucao,
                request.ResultadoInformado);

        await handler.HandleAsync(
            command,
            cancellationToken);

        return Results.NoContent();
    });

app.MapDelete(
    "/api/opcoes/{id:guid}",
    async (
        Guid id,
        ExcluirOperacaoOpcaoHandler handler,
        CancellationToken cancellationToken) =>
    {
        await handler.HandleAsync(
            id,
            cancellationToken);

        return Results.NoContent();
    });

app.MapGet(
    "/api/dashboard/evolucao",
    async (
        IHistoricoPatrimonioRepository repository,
        CancellationToken cancellationToken) =>
    {
        var resultado =
            await repository.ListarEvolucaoAsync(
                cancellationToken);

        return Results.Ok(resultado);
    });

app.MapGet(
    "/api/performance",
    async (
        Guid? investidorId,
        ConsultarPerformanceCarteiraService service,
        CancellationToken cancellationToken) =>
    {
        var resultado = await service.ConsultarAsync(
            investidorId,
            cancellationToken);

        return Results.Ok(resultado);
    });

app.MapGet(
    "/api/dashboard",
    async (
        ConsultarDashboardConsolidadoHandler handler,
        CancellationToken cancellationToken) =>
    {
        var resultado =
            await handler.HandleAsync(
                cancellationToken);

        return Results.Ok(resultado);
    });

app.MapGet(
    "/api/dashboard/{investidorId:guid}",
    async (
        Guid investidorId,
        ConsultarDashboardHandler handler,
        CancellationToken cancellationToken) =>
    {
        var resultado =
            await handler.HandleAsync(
                investidorId,
                cancellationToken);

        return Results.Ok(resultado);
    });

app.MapGet(
    "/api/fiscal",
    async Task<IResult> (
        Guid? investidorId,
        int? ano,
        IFiscalRepository repository,
        CancellationToken cancellationToken) =>
    {
        var resultado =
            await repository.ConsultarAsync(
                investidorId,
                ano,
                cancellationToken);

        return Results.Ok(resultado);
    });

app.MapGet(
    "/api/descontos-fiscais",
    async (
        IDescontoFiscalRepository repository,
        CancellationToken cancellationToken) =>
    {
        var descontos =
            await repository.ListarAsync(
                cancellationToken);

        return Results.Ok(
            descontos.Select(
                x => new
                {
                    x.Id,
                    x.Tipo,
                    x.DataPagamento,
                    x.Valor,
                    x.Descricao,
                    x.InvestidorId,
                    InvestidorNome = x.Investidor != null
                        ? x.Investidor.Nome
                        : null
                }));
    });

app.MapPost(
    "/api/descontos-fiscais",
    async (
        CadastrarDescontoFiscalRequest request,
        IDescontoFiscalRepository repository,
        InvestimentosDbContext dbContext,
        CancellationToken cancellationToken) =>
    {
        var investidor = request.InvestidorId.HasValue
            ? await dbContext.Investidores.FirstOrDefaultAsync(
                x => x.Id == request.InvestidorId.Value,
                cancellationToken)
            : null;

        if (request.InvestidorId.HasValue && investidor is null)
            return Results.NotFound(new { detail = "Investidor não encontrado." });

        var desconto =
            new DescontoFiscal(
                request.Tipo
                    .Trim()
                    .ToUpperInvariant(),
                request.DataPagamento,
                request.Valor,
                request.Descricao,
                investidor);

        await repository.AdicionarAsync(
            desconto,
            cancellationToken);

        return Results.Created(
            $"/api/descontos-fiscais/{desconto.Id}",
            new { desconto.Id });
    });

app.MapPut(
    "/api/descontos-fiscais/{id:guid}",
    async (
        Guid id,
        AtualizarDescontoFiscalRequest request,
        IDescontoFiscalRepository repository,
        InvestimentosDbContext dbContext,
        CancellationToken cancellationToken) =>
    {
        var investidor = request.InvestidorId.HasValue
            ? await dbContext.Investidores.FirstOrDefaultAsync(
                x => x.Id == request.InvestidorId.Value,
                cancellationToken)
            : null;

        if (request.InvestidorId.HasValue && investidor is null)
            return Results.NotFound(new { detail = "Investidor não encontrado." });

        var desconto =
            await repository.ObterPorIdAsync(
                id,
                cancellationToken);

        if (desconto is null)
        {
            return Results.NotFound(
                new { detail = "Imposto não encontrado." });
        }

        desconto.Atualizar(
            "DARF",
            request.DataPagamento,
            request.Valor,
            request.Descricao,
            investidor);

        await repository.SalvarAlteracoesAsync(
            cancellationToken);

        return Results.NoContent();
    });

app.MapDelete(
    "/api/descontos-fiscais/{id:guid}",
    async (
        Guid id,
        IDescontoFiscalRepository repository,
        CancellationToken cancellationToken) =>
    {
        var desconto =
            await repository.ObterPorIdAsync(
                id,
                cancellationToken);

        if (desconto is null)
        {
            return Results.NotFound(
                new { detail = "Imposto não encontrado." });
        }

        repository.Excluir(
            desconto);

        await repository.SalvarAlteracoesAsync(
            cancellationToken);

        return Results.NoContent();
    });

app.MapGet(
    "/api/metas-ativos",
    async (
        Guid? investidorId,
        IMetaAtivoRepository repository,
        CancellationToken cancellationToken) =>
    {
        return Results.Ok(
            await repository.ListarAsync(
                investidorId,
                cancellationToken));
    });

app.MapPut(
    "/api/metas-ativos",
    async (
        SalvarMetaAtivoRequest request,
        IMetaAtivoRepository repository,
        CancellationToken cancellationToken) =>
    {
        return Results.Ok(
            await repository.SalvarAsync(
                request,
                cancellationToken));
    });

app.MapDelete(
    "/api/metas-ativos/{id:guid}",
    async (
        Guid id,
        IMetaAtivoRepository repository,
        CancellationToken cancellationToken) =>
    {
        return await repository.ExcluirAsync(
            id,
            cancellationToken)
                ? Results.NoContent()
                : Results.NotFound();
    });

app.MapGet(
    "/api/admin/integridade",
    async (
        IIntegridadeRepository repository,
        CancellationToken cancellationToken) =>
    {
        var resultado =
            await repository.VerificarAsync(
                cancellationToken);

        return Results.Ok(resultado);
    });

app.MapGet(
    "/api/admin",
    async (
        IAdministracaoRepository repository,
        CancellationToken cancellationToken) =>
    {
        var resultado =
            await repository.ObterAsync(
                cancellationToken);

        return Results.Ok(resultado);
    });

app.MapPost(
    "/api/admin/ativos",
    async (
        CriarAtivoAdministracaoRequest request,
        IAdministracaoRepository repository,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var resultado =
                await repository.CriarAtivoAsync(
                    request,
                    cancellationToken);

            return Results.Created(
                $"/api/admin/ativos/{resultado.Id}",
                resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(
                new
                {
                    detail = ex.Message
                });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(
                new
                {
                    detail = ex.Message
                });
        }
    });

app.MapPut(
    "/api/admin/ativos/{id:guid}",
    async (
        Guid id,
        AtualizarAtivoAdministracaoRequest request,
        IAdministracaoRepository repository,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var resultado =
                await repository.AtualizarAtivoAsync(
                    id,
                    request,
                    cancellationToken);

            if (resultado is null)
            {
                return Results.NotFound(
                    new
                    {
                        detail =
                            "Ativo não encontrado."
                    });
            }

            return Results.Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(
                new
                {
                    detail = ex.Message
                });
        }
    });

app.MapDelete(
    "/api/admin/ativos/{id:guid}",
    async (
        Guid id,
        IAdministracaoRepository repository,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var excluido =
                await repository.ExcluirAtivoAsync(
                    id,
                    cancellationToken);

            if (!excluido)
            {
                return Results.NotFound(
                    new
                    {
                        detail =
                            "Ativo não encontrado."
                    });
            }

            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(
                new
                {
                    detail = ex.Message
                });
        }
    });

app.MapPut(
    "/api/admin/classes-ativo/{id:int}",
    async (
        int id,
        AtualizarParametroRequest request,
        IAdministracaoRepository repository,
        CancellationToken cancellationToken) =>
    {
        var atualizado =
            await repository.AtualizarClasseAtivoAsync(
                id,
                request.Codigo,
                request.Nome,
                request.Ativo,
                cancellationToken);

        return atualizado
            ? Results.NoContent()
            : Results.NotFound();
    });

app.MapDelete(
    "/api/admin/classes-ativo/{id:int}",
    async (
        int id,
        IAdministracaoRepository repository,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var excluido =
                await repository.ExcluirClasseAtivoAsync(
                    id,
                    cancellationToken);

            return excluido
                ? Results.NoContent()
                : Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(
                new { detail = ex.Message });
        }
    });

app.MapPut(
    "/api/admin/tipos-ativo/{id:int}",
    async (
        int id,
        AtualizarTipoAtivoParametroRequest request,
        IAdministracaoRepository repository,
        CancellationToken cancellationToken) =>
    {
        var atualizado =
            await repository.AtualizarTipoAtivoAsync(
                id,
                request.Codigo,
                request.Nome,
                request.ClasseAtivoId,
                request.Ativo,
                cancellationToken);

        return atualizado
            ? Results.NoContent()
            : Results.NotFound();
    });

app.MapDelete(
    "/api/admin/tipos-ativo/{id:int}",
    async (
        int id,
        IAdministracaoRepository repository,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var excluido =
                await repository.ExcluirTipoAtivoAsync(
                    id,
                    cancellationToken);

            return excluido
                ? Results.NoContent()
                : Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(
                new { detail = ex.Message });
        }
    });

app.MapPut(
    "/api/admin/tipos-operacao/{id:int}",
    async (
        int id,
        AtualizarParametroRequest request,
        IAdministracaoRepository repository,
        CancellationToken cancellationToken) =>
    {
        var atualizado =
            await repository.AtualizarTipoOperacaoAsync(
                id,
                request.Codigo,
                request.Nome,
                request.Ativo,
                cancellationToken);

        return atualizado
            ? Results.NoContent()
            : Results.NotFound();
    });

app.MapDelete(
    "/api/admin/tipos-operacao/{id:int}",
    async (
        int id,
        IAdministracaoRepository repository,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var excluido =
                await repository.ExcluirTipoOperacaoAsync(
                    id,
                    cancellationToken);

            return excluido
                ? Results.NoContent()
                : Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(
                new { detail = ex.Message });
        }
    });

app.MapAdministracaoInvestidoresEndpoints();

app.MapAuthEndpoints();
app.MapUsuariosEndpoints();

await BootstrapAdminService.ExecutarAsync(
    app.Services,
    app.Configuration);

app.Run();

/*
 * Retorna a raiz do ticker da opção.
 *
 * O código alfabético imediatamente anterior aos
 * números contém a letra de vencimento.
 *
 * ITUBU407 -> ITUBU -> ITUB
 * VALEM631 -> VALEM -> VALE
 * PETRI454 -> PETRI -> PETR
 */
static string ObterRaizTickerOpcao(
    string tickerOpcao)
{
    if (string.IsNullOrWhiteSpace(
            tickerOpcao))
    {
        return string.Empty;
    }

    var ticker =
        tickerOpcao
            .Trim()
            .ToUpperInvariant();

    var letras =
        new string(
            ticker
                .TakeWhile(char.IsLetter)
                .ToArray());

    if (letras.Length <= 1)
    {
        return string.Empty;
    }

    return letras[..^1];
}

public record CadastrarInvestidorRequest(
    string Nome);

public record CadastrarAtivoRequest(
    string Ticker,
    string Nome,
    string TipoAtivoCodigo);

public record CadastrarOperacaoRequest(
    DateTime Data,
    Guid InvestidorId,
    string Ticker,
    string TipoOperacaoCodigo,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal Taxas);

public record AtualizarOperacaoRequest(
    DateTime Data,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal Taxas);

public record CadastrarProventoRequest(
    Guid InvestidorId,
    string Ticker,
    string Tipo,
    DateTime? DataCom,
    DateTime DataPagamento,
    decimal QuantidadeBase,
    decimal ValorPorUnidade,
    decimal ValorRecebido,
    string? Descricao);

public record SalvarProventoTotalRequest(
    string Ticker,
    string Tipo,
    string? Descricao,
    DateTime? DataCom,
    DateTime DataPagamento,
    decimal ValorTotal);

public record AtualizarProventoRequest(
    string Ticker,
    string Tipo,
    string? Descricao,
    DateTime? DataCom,
    DateTime DataPagamento,
    decimal QuantidadeBase,
    decimal ValorPorUnidade,
    decimal ValorRecebido);

public record CadastrarDescontoFiscalRequest(
    string Tipo,
    DateTime DataPagamento,
    decimal Valor,
    string? Descricao,
    Guid? InvestidorId);

public record AtualizarDescontoFiscalRequest(
    DateTime DataPagamento,
    decimal Valor,
    string? Descricao,
    Guid? InvestidorId);

/*
 * Novo contrato da tela de inclusão.
 *
 * Não recebe:
 * - TickerAtivo
 * - Contratos
 * - Taxas
 * - Status
 *
 * Status é automaticamente EXECUTADA.
 */
public record CadastrarOperacaoOpcaoRequest(
    Guid InvestidorId,
    string Ticker,
    string TipoOpcao,
    string Natureza,
    DateTime DataOperacao,
    DateTime Vencimento,
    decimal Quantidade,
    decimal Strike,
    decimal PremioUnitario);

public record AtualizarOperacaoOpcaoRequest(
    DateTime DataOperacao,
    DateTime Vencimento,
    decimal Strike,
    int Contratos,
    decimal Quantidade,
    decimal PremioUnitario,
    string Situacao,
    DateTime? DataFinalizacao,
    decimal? PrecoRecompraUnitario,
    decimal? ValorExecucao,
    decimal? ResultadoInformado);

public record AtualizarParametroRequest(
    string Codigo,
    string Nome,
    bool Ativo);

public record AtualizarTipoAtivoParametroRequest(
    string Codigo,
    string Nome,
    int ClasseAtivoId,
    bool Ativo);


public record CriarMovimentacaoFinanceiraRequest(
    Guid InvestidorId,
    DateTime Data,
    string Tipo,
    decimal Valor,
    string? Descricao);

public record AtualizarMovimentacaoFinanceiraRequest(
    Guid InvestidorId,
    DateTime Data,
    string Tipo,
    decimal Valor,
    string? Descricao);
