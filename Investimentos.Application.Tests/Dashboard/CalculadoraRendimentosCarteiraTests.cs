using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Dashboard;

namespace Investimentos.Application.Tests.Dashboard;

public class CalculadoraRendimentosCarteiraTests
{
    [Fact]
    public void SomaValorizacaoVendasEProventosLiquidosSemDuplicarOpcoes()
    {
        var acao = Posicao("ACAO") with { Valorizacao = 100, ResultadoRealizado = 20, Proventos = 15, ResultadoOpcoes = 500 };
        var fii = Posicao("FII") with { Valorizacao = -30, ResultadoRealizado = -5, Proventos = 10 };
        var etf = Posicao("ETF") with { Valorizacao = 1000, Proventos = 1000 };
        var previdencia = Posicao("PREVIDENCIA") with { Valorizacao = 2000 };
        Assert.Equal(110m, CalculadoraRendimentosCarteira.CalcularAcoesFiis([acao, fii, etf, previdencia]));
    }

    [Fact]
    public void ConservaVendasEProventosDeAtivosZeradosESemCotacao()
    {
        var zerada = Posicao("ACAO") with { Quantidade = 0, PrecoAtual = null, Valorizacao = -100, ResultadoRealizado = 50, Proventos = 10 };
        var semCotacao = Posicao("FII") with { PrecoAtual = null, Valorizacao = -100, ResultadoRealizado = -20, Proventos = 5 };
        Assert.Equal(45m, CalculadoraRendimentosCarteira.CalcularAcoesFiis([zerada, semCotacao]));
    }

    [Fact]
    public void PreservaPrejuizoECarteiraVaziaRetornaZero()
    {
        Assert.Equal(-60m, CalculadoraRendimentosCarteira.CalcularAcoesFiis([Posicao("ACAO") with { Valorizacao = -50, ResultadoRealizado = -10 }]));
        Assert.Equal(0m, CalculadoraRendimentosCarteira.CalcularAcoesFiis([]));
    }

    private static PosicaoAtivoDto Posicao(string tipo) =>
        new("TEST11", "Teste", tipo, tipo, 10, 10, 100, 0) { PrecoAtual = 10 };
}
