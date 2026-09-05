using Investimentos.Application.Ativos.CadastrarAtivo;
using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Investidores.CadastrarInvestidor;
using Investimentos.Application.Operacoes.CadastrarOperacao;
using Investimentos.Infrastructure;

var builder =
    WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
        try
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
                new { Id = id });
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(
                new { Erro = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(
                new { Erro = exception.Message });
        }
    });

app.MapPost(
    "/api/ativos",
    async (
        CadastrarAtivoRequest request,
        CadastrarAtivoHandler handler,
        CancellationToken cancellationToken) =>
    {
        try
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
                new { Id = id });
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(
                new { Erro = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(
                new { Erro = exception.Message });
        }
    });

app.MapPost(
    "/api/operacoes",
    async (
        CadastrarOperacaoRequest request,
        CadastrarOperacaoHandler handler,
        CancellationToken cancellationToken) =>
    {
        try
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
                new { Id = id });
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(
                new { Erro = exception.Message });
        }
    });

app.MapGet(
    "/api/carteira/{investidorId:guid}",
    async (
        Guid investidorId,
        ConsultarCarteiraHandler handler,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var posicoes =
                await handler.HandleAsync(
                    investidorId,
                    cancellationToken);

            return Results.Ok(
                posicoes);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(
                new { Erro = exception.Message });
        }
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