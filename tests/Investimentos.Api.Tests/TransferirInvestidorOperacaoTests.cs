using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Operacoes.AtualizarOperacao;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;
using Investimentos.Infrastructure.Persistence;
using Investimentos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.Tests;

public class TransferirInvestidorOperacaoTests
{
    private static readonly DateTime Dia = new(2025, 8, 1);

    [Fact]
    public async Task MoveCompraAntigaPreservandoIdentidadeERecalculandoAsDuasCarteiras()
    {
        await using var db = CriarBanco();
        var origem = new Investidor("Origem");
        var destino = new Investidor("Destino");
        var ativo = CriarAtivo();
        var compra = new TipoOperacao("COMPRA", "Compra");
        var operacao = new Operacao(Dia, 1, origem, ativo, compra, 100, 10, 1);
        db.Operacoes.AddRange(operacao, new Operacao(Dia, 1, destino, ativo, compra, 10, 20));
        await db.SaveChangesAsync();
        var id = operacao.Id;

        await new AtualizarOperacaoHandler(new OperacaoRepository(db)).HandleAsync(
            new AtualizarOperacaoCommand(id, Dia, 100, 10, 1, destino.Id));

        db.ChangeTracker.Clear();
        var gravada = await db.Operacoes.SingleAsync(x => x.Id == id);
        Assert.Equal(destino.Id, gravada.InvestidorId);
        Assert.Equal(ativo.Id, gravada.AtivoId);
        Assert.Equal(Dia, gravada.Data);
        Assert.Equal(2, gravada.Sequencia);
        Assert.Equal(2, await db.Operacoes.CountAsync());
        var repositorio = new CarteiraRepository(db);
        var calculadora = new CalcularCarteiraService();
        Assert.Empty(calculadora.Calcular(await repositorio.ObterOperacoesAsync(origem.Id)));
        var posicao = Assert.Single(calculadora.Calcular(await repositorio.ObterOperacoesAsync(destino.Id)));
        Assert.Equal(110m, posicao.Quantidade);
        Assert.Equal(1201m, posicao.CustoTotal);
    }

    [Fact]
    public async Task BloqueiaTransferenciaQueDeixariaVendaSemCompraNaOrigem()
    {
        await using var db = CriarBanco();
        var origem = new Investidor("Origem");
        var destino = new Investidor("Destino");
        db.Investidores.Add(destino);
        var ativo = CriarAtivo();
        var operacao = new Operacao(Dia, 1, origem, ativo, new TipoOperacao("COMPRA", "Compra"), 100, 10);
        db.Operacoes.AddRange(operacao, new Operacao(Dia.AddDays(1), 1, origem, ativo, new TipoOperacao("VENDA", "Venda"), 50, 12));
        await db.SaveChangesAsync();

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AtualizarOperacaoHandler(new OperacaoRepository(db)).HandleAsync(
                new AtualizarOperacaoCommand(operacao.Id, Dia, 100, 11, 2, destino.Id)));

        Assert.Contains("origem", erro.Message);
        db.ChangeTracker.Clear();
        var gravada = await db.Operacoes.SingleAsync(x => x.Id == operacao.Id);
        Assert.Equal(origem.Id, gravada.InvestidorId);
        Assert.Equal(10m, gravada.PrecoUnitario);
        Assert.Equal(0m, gravada.Taxas);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BloqueiaVendaTransferidaSemPosicaoNoDestinoInclusiveEmVendaPosterior(bool vendaPosterior)
    {
        await using var db = CriarBanco();
        var origem = new Investidor("Origem");
        var destino = new Investidor("Destino");
        var ativo = CriarAtivo();
        var compra = new TipoOperacao("COMPRA", "Compra");
        var venda = new TipoOperacao("VENDA", "Venda");
        var operacao = new Operacao(Dia.AddDays(1), 1, origem, ativo, venda, 50, 12);
        db.Operacoes.AddRange(new Operacao(Dia, 1, origem, ativo, compra, 100, 10), operacao);
        if (vendaPosterior)
            db.Operacoes.AddRange(new Operacao(Dia, 1, destino, ativo, compra, 100, 10),
                new Operacao(Dia.AddDays(2), 1, destino, ativo, venda, 100, 12));
        else
            db.Operacoes.Add(new Operacao(Dia.AddDays(2), 1, destino, ativo, compra, 100, 10));
        await db.SaveChangesAsync();

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AtualizarOperacaoHandler(new OperacaoRepository(db)).HandleAsync(
                new AtualizarOperacaoCommand(operacao.Id, operacao.Data, 50, 12, 0, destino.Id)));

        Assert.Contains("destino", erro.Message);
        db.ChangeTracker.Clear();
        Assert.Equal(origem.Id, (await db.Operacoes.SingleAsync(x => x.Id == operacao.Id)).InvestidorId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RejeitaInvestidorVazioOuInexistente(bool vazio)
    {
        await using var db = CriarBanco();
        var origem = new Investidor("Origem");
        var operacao = new Operacao(Dia, 1, origem, CriarAtivo(), new TipoOperacao("COMPRA", "Compra"), 100, 10);
        db.Operacoes.Add(operacao);
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new AtualizarOperacaoHandler(new OperacaoRepository(db)).HandleAsync(
                new AtualizarOperacaoCommand(operacao.Id, Dia, 100, 10, 0, vazio ? Guid.Empty : Guid.NewGuid())));
        Assert.Equal(origem.Id, operacao.InvestidorId);
    }

    [Fact]
    public async Task EdicaoSemInvestidorNovoMantemTitularESequencia()
    {
        await using var db = CriarBanco();
        var origem = new Investidor("Origem");
        var operacao = new Operacao(Dia, 7, origem, CriarAtivo(), new TipoOperacao("COMPRA", "Compra"), 100, 10);
        db.Operacoes.Add(operacao);
        await db.SaveChangesAsync();
        await new AtualizarOperacaoHandler(new OperacaoRepository(db)).HandleAsync(
            new AtualizarOperacaoCommand(operacao.Id, Dia, 200, 11, 1));
        Assert.Equal(origem.Id, operacao.InvestidorId);
        Assert.Equal(7, operacao.Sequencia);
        Assert.Equal(200m, operacao.Quantidade);
    }

    private static InvestimentosDbContext CriarBanco() => new(new DbContextOptionsBuilder<InvestimentosDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static Ativo CriarAtivo() => new(new Ticker("ITUB4"), "Itaú",
        new TipoAtivo("ACAO", "Ação", new ClasseAtivo("RENDA_VARIAVEL", "Renda variável")));
}
