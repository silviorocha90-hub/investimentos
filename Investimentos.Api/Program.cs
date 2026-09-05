using Investimentos.Application.Ativos.CadastrarAtivo;
using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Investidores.CadastrarInvestidor;
using Investimentos.Application.Opcoes.CadastrarOperacaoOpcao;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Operacoes.CadastrarOperacao;
using Investimentos.Application.Proventos.CadastrarProvento;
using Investimentos.Application.Proventos.ConsultarProventos;
using Investimentos.Infrastructure;

var builder =
    WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddScoped<
    CadastrarInvestidorHandler>();

builder.Services.AddScoped<
    CadastrarAtivoHandler>();

builder.Services.AddScoped<
    CadastrarOperacaoHandler>();

builder.Services.AddScoped<
    ConsultarCarteiraHandler>();

builder.Services.AddScoped<
    CalcularCarteiraService>();

builder.Services.AddScoped<
    CadastrarProventoHandler>();

builder.Services.AddScoped<
    ConsultarProventosHandler>();

builder.Services.AddScoped<
    CadastrarOperacaoOpcaoHandler>();

builder.Services.AddScoped<
    ConsultarOpcoesHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app =
    builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
                request.ValorPorUnidade);

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
    DateTime DataCom,
    DateTime DataPagamento,
    decimal QuantidadeBase,
    decimal ValorPorUnidade);

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