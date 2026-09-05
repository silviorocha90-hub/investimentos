using Investimentos.Api.ExceptionHandling;
using Investimentos.Application.Ativos.CadastrarAtivo;
using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Dashboard;
using Investimentos.Application.Interfaces;
using Investimentos.Application.Investidores.CadastrarInvestidor;
using Investimentos.Application.Investidores.ListarInvestidores;
using Investimentos.Application.Opcoes.CadastrarOperacaoOpcao;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Operacoes.CadastrarOperacao;
using Investimentos.Application.Proventos.CadastrarProvento;
using Investimentos.Application.Proventos.ConsultarProventos;
using Investimentos.Domain.Entities;
using Investimentos.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<CadastrarInvestidorHandler>();
builder.Services.AddScoped<ListarInvestidoresHandler>();
builder.Services.AddScoped<CadastrarAtivoHandler>();
builder.Services.AddScoped<CadastrarOperacaoHandler>();
builder.Services.AddScoped<ConsultarCarteiraHandler>();
builder.Services.AddScoped<CalcularCarteiraService>();
builder.Services.AddScoped<CadastrarProventoHandler>();
builder.Services.AddScoped<ConsultarProventosHandler>();
builder.Services.AddScoped<CadastrarOperacaoOpcaoHandler>();
builder.Services.AddScoped<ConsultarOpcoesHandler>();
builder.Services.AddScoped<ConsultarDashboardHandler>();
builder.Services.AddScoped<ConsultarDashboardConsolidadoHandler>();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("Frontend");

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

app.MapPost(
    "/api/opcoes",
    async (
        CadastrarOperacaoOpcaoRequest request,
        CadastrarOperacaoOpcaoHandler handler,
        CancellationToken cancellationToken) =>
    {
        var command =
            new CadastrarOperacaoOpcaoCommand(
                request.InvestidorId,
                request.TickerAtivo,
                request.TickerOpcao,
                request.TipoOpcao,
                request.Natureza,
                request.DataOperacao,
                request.Vencimento,
                request.Strike,
                request.Contratos,
                request.Quantidade,
                request.PremioUnitario,
                request.Taxas);

        var id =
            await handler.HandleAsync(
                command,
                cancellationToken);

        return Results.Created(
            $"/api/opcoes/{id}",
            new { id });
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
        var descontos = await repository.ListarAsync(null, cancellationToken);
        return Results.Ok(descontos.Select(x => new
        {
            x.Id,
            x.InvestidorId,
            Investidor = x.Investidor.Nome,
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
        var investidor = await repository.ObterInvestidorAsync(
            request.InvestidorId,
            cancellationToken);

        if (investidor is null)
            return Results.NotFound("Investidor não encontrado.");

        var desconto = new DescontoFiscal(
            investidor,
            request.Tipo.Trim().ToUpperInvariant(),
            request.DataPagamento,
            request.Valor,
            request.Descricao);

        await repository.AdicionarAsync(desconto, cancellationToken);
        return Results.Created($"/api/descontos-fiscais/{desconto.Id}", new { desconto.Id });
    });

app.Run();

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
    Guid InvestidorId,
    string Tipo,
    DateTime DataPagamento,
    decimal Valor,
    string? Descricao);

public record CadastrarOperacaoOpcaoRequest(
    Guid InvestidorId,
    string TickerAtivo,
    string TickerOpcao,
    string TipoOpcao,
    string Natureza,
    DateTime DataOperacao,
    DateTime Vencimento,
    decimal Strike,
    int Contratos,
    decimal Quantidade,
    decimal PremioUnitario,
    decimal Taxas);
