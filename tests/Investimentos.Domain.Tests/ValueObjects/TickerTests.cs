using Investimentos.Domain.ValueObjects;

namespace Investimentos.Domain.Tests.ValueObjects;

public class TickerTests
{
    [Fact]
    public void TickersComMesmoCodigoDevemSerIguais()
    {
        // Arrange
        var ticker1 = new Ticker("ITUB4");
        var ticker2 = new Ticker("ITUB4");

        // Act
        var saoIguais = ticker1 == ticker2;

        // Assert
        Assert.True(saoIguais);
    }

    [Fact]
    public void NaoDeveCriarTickerSemCodigo()
    {
        // Arrange
        var codigo = "";

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => new Ticker(codigo));

        // Assert
        Assert.Equal(
            "O código do ticker é obrigatório.",
            exception.Message);
    }

    [Fact]
    public void DeveNormalizarCodigoDoTicker()
    {
        // Arrange
        var codigo = "  itub4  ";

        // Act
        var ticker = new Ticker(codigo);

        // Assert
        Assert.Equal("ITUB4", ticker.Codigo);
    }
}