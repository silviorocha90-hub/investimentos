using Investimentos.Domain.Entities;

namespace Investimentos.Domain.Tests;

public class ClasseAtivoTests
{
    [Fact]
    public void DeveCriarClasseAtivoComDadosValidos()
    {
        var classe = new ClasseAtivo(
            "renda_variavel",
            "Renda Variável");

        Assert.Equal("RENDA_VARIAVEL", classe.Codigo);
        Assert.Equal("Renda Variável", classe.Nome);
        Assert.True(classe.Ativo);
    }

    [Fact]
    public void NaoDeveCriarClasseAtivoSemCodigo()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new ClasseAtivo("", "Renda Variável"));

        Assert.Equal(
            "O código da classe do ativo é obrigatório.",
            exception.Message);
    }

    [Fact]
    public void NaoDeveCriarClasseAtivoSemNome()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new ClasseAtivo("RENDA_VARIAVEL", ""));

        Assert.Equal(
            "O nome da classe do ativo é obrigatório.",
            exception.Message);
    }
}