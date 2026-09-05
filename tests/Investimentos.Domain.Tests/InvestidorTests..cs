using Investimentos.Domain.Entities;

namespace Investimentos.Domain.Tests;

public class InvestidorTests
{
    [Fact]
    public void DeveCriarInvestidorComNomeValido()
    {
        var investidor = new Investidor("Silvio");

        Assert.NotEqual(Guid.Empty, investidor.Id);
        Assert.Equal("Silvio", investidor.Nome);
    }

    [Fact]
    public void DeveRemoverEspacosDoNomeAoCriarInvestidor()
    {
        var investidor = new Investidor("  Silvio  ");

        Assert.Equal("Silvio", investidor.Nome);
    }

    [Fact]
    public void NaoDeveCriarInvestidorSemNome()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new Investidor(""));

        Assert.Equal(
            "O nome do investidor é obrigatório.",
            exception.Message);
    }

    [Fact]
    public void DeveAlterarNomeDoInvestidor()
    {
        var investidor = new Investidor("Silvio");

        investidor.AlterarNome("Silvio Xavier");

        Assert.Equal("Silvio Xavier", investidor.Nome);
    }

    [Fact]
    public void NaoDeveAlterarNomeParaNomeVazio()
    {
        var investidor = new Investidor("Silvio");

        var exception = Assert.Throws<ArgumentException>(
            () => investidor.AlterarNome(""));

        Assert.Equal(
            "O nome do investidor é obrigatório.",
            exception.Message);
    }
}