using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;

namespace Investimentos.Domain.Tests;

public class AtivoTests
{
    [Fact]
    public void DeveCriarAtivoComDadosValidos()
    {
        var classe = new ClasseAtivo(
            "RENDA_VARIAVEL",
            "Renda Variável");

        var tipo = new TipoAtivo(
            "ACAO",
            "Ação",
            classe);

        var ticker = new Ticker("ITUB4");

        var ativo = new Ativo(
            ticker,
            "Itaú Unibanco",
            tipo);

        Assert.NotEqual(Guid.Empty, ativo.Id);
        Assert.Equal(ticker, ativo.Ticker);
        Assert.Equal("Itaú Unibanco", ativo.Nome);
        Assert.Equal(tipo, ativo.TipoAtivo);
        Assert.Equal(classe, ativo.TipoAtivo.ClasseAtivo);
    }

    [Fact]
    public void NaoDeveCriarAtivoSemTicker()
    {
        var classe = new ClasseAtivo(
            "RENDA_VARIAVEL",
            "Renda Variável");

        var tipo = new TipoAtivo(
            "ACAO",
            "Ação",
            classe);

        var exception = Assert.Throws<ArgumentException>(
            () => new Ativo(
                null!,
                "Itaú Unibanco",
                tipo));

        Assert.Equal(
            "O ticker do ativo é obrigatório.",
            exception.Message);
    }

    [Fact]
    public void NaoDeveCriarAtivoSemNome()
    {
        var classe = new ClasseAtivo(
            "RENDA_VARIAVEL",
            "Renda Variável");

        var tipo = new TipoAtivo(
            "ACAO",
            "Ação",
            classe);

        var ticker = new Ticker("ITUB4");

        var exception = Assert.Throws<ArgumentException>(
            () => new Ativo(
                ticker,
                "",
                tipo));

        Assert.Equal(
            "O nome do ativo é obrigatório.",
            exception.Message);
    }

    [Fact]
    public void NaoDeveCriarAtivoSemTipo()
    {
        var ticker = new Ticker("ITUB4");

        var exception = Assert.Throws<ArgumentException>(
            () => new Ativo(
                ticker,
                "Itaú Unibanco",
                null!));

        Assert.Equal(
            "O tipo do ativo é obrigatório.",
            exception.Message);
    }
}