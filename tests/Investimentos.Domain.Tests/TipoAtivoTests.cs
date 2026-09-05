using Investimentos.Domain.Entities;

namespace Investimentos.Domain.Tests;

public class TipoAtivoTests
{
    [Fact]
    public void DeveCriarTipoAtivoComDadosValidos()
    {
        var classe = new ClasseAtivo(
            "RENDA_VARIAVEL",
            "Renda Variável");

        var tipo = new TipoAtivo(
            "acao",
            "Ação",
            classe);

        Assert.Equal("ACAO", tipo.Codigo);
        Assert.Equal("Ação", tipo.Nome);
        Assert.Equal(classe, tipo.ClasseAtivo);
        Assert.True(tipo.Ativo);
    }

    [Fact]
    public void NaoDeveCriarTipoAtivoSemCodigo()
    {
        var classe = new ClasseAtivo(
            "RENDA_VARIAVEL",
            "Renda Variável");

        var exception = Assert.Throws<ArgumentException>(
            () => new TipoAtivo(
                "",
                "Ação",
                classe));

        Assert.Equal(
            "O código do tipo do ativo é obrigatório.",
            exception.Message);
    }

    [Fact]
    public void NaoDeveCriarTipoAtivoSemNome()
    {
        var classe = new ClasseAtivo(
            "RENDA_VARIAVEL",
            "Renda Variável");

        var exception = Assert.Throws<ArgumentException>(
            () => new TipoAtivo(
                "ACAO",
                "",
                classe));

        Assert.Equal(
            "O nome do tipo do ativo é obrigatório.",
            exception.Message);
    }

    [Fact]
    public void NaoDeveCriarTipoAtivoSemClasse()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new TipoAtivo(
                "ACAO",
                "Ação",
                null!));

        Assert.Equal(
            "A classe do ativo é obrigatória.",
            exception.Message);
    }
}