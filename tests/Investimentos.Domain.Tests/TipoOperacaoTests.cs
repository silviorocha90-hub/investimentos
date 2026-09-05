using Investimentos.Domain.Entities;

namespace Investimentos.Domain.Tests;

public class TipoOperacaoTests
{
    [Fact]
    public void DeveCriarTipoOperacaoComDadosValidos()
    {
        var tipo = new TipoOperacao(
            "compra",
            "Compra");

        Assert.Equal("COMPRA", tipo.Codigo);
        Assert.Equal("Compra", tipo.Nome);
        Assert.True(tipo.Ativo);
    }

    [Fact]
    public void NaoDeveCriarTipoOperacaoSemCodigo()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new TipoOperacao("", "Compra"));

        Assert.Equal(
            "O código do tipo de operação é obrigatório.",
            exception.Message);
    }

    [Fact]
    public void NaoDeveCriarTipoOperacaoSemNome()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new TipoOperacao("COMPRA", ""));

        Assert.Equal(
            "O nome do tipo de operação é obrigatório.",
            exception.Message);
    }
}