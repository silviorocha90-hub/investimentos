using Investimentos.Api.Endpoints;
using Investimentos.Api.ExceptionHandling;
using Investimentos.Api.Startup;
using Investimentos.Application.Administracao;
using Investimentos.Application.Ativos.CadastrarAtivo;
using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Dashboard;
using Investimentos.Application.Interfaces;
using Investimentos.Application.Investidores.CadastrarInvestidor;
using Investimentos.Application.Investidores.ListarInvestidores;
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
using Investimentos.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Investimentos.Application.Usuarios.Administracao;

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
                SameSiteMode.Lax;

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

builder.Services.AddScoped<ConsultarDashboardHandler>();
builder.Services.AddScoped<ConsultarDashboardConsolidadoHandler>();

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

var app = builder.Build();

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
                request.ValorRecebido);

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
                request.ValorExecucao);

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
                    x.Descricao
                }));
    });

app.MapPost(
    "/api/descontos-fiscais",
    async (
        CadastrarDescontoFiscalRequest request,
        IDescontoFiscalRepository repository,
        CancellationToken cancellationToken) =>
    {
        var desconto =
            new DescontoFiscal(
                request.Tipo
                    .Trim()
                    .ToUpperInvariant(),
                request.DataPagamento,
                request.Valor,
                request.Descricao);

        await repository.AdicionarAsync(
            desconto,
            cancellationToken);

        return Results.Created(
            $"/api/descontos-fiscais/{desconto.Id}",
            new { desconto.Id });
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
    decimal ValorRecebido);

public record CadastrarDescontoFiscalRequest(
    string Tipo,
    DateTime DataPagamento,
    decimal Valor,
    string? Descricao);

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
    decimal? ValorExecucao);