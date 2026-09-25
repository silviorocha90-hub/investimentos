using Investimentos.Application.Performance;
using Investimentos.Domain.Entities;

namespace Investimentos.Application.Tests;

public class CalculadoraPerformanceCarteiraTests
{
    [Fact]
    public void Calcula_retorno_sem_fluxos()
    {
        var investidor = new Investidor("Teste");
        var historicos = new[]
        {
            new HistoricoPatrimonio(investidor, new DateTime(2026, 1, 1), 1000),
            new HistoricoPatrimonio(investidor, new DateTime(2026, 2, 1), 1100)
        };

        var resultado = CalculadoraPerformanceCarteira.Calcular(
            historicos,
            Array.Empty<MovimentacaoFinanceira>());

        Assert.Equal(10m, resultado.RentabilidadeAcumulada);
        Assert.Equal(100m, resultado.GanhoLiquido);
    }

    [Fact]
    public void Aporte_nao_e_tratado_como_rendimento()
    {
        var investidor = new Investidor("Teste");
        var historicos = new[]
        {
            new HistoricoPatrimonio(investidor, new DateTime(2026, 1, 1), 1000),
            new HistoricoPatrimonio(investidor, new DateTime(2026, 2, 1), 1500)
        };
        var fluxos = new[]
        {
            new MovimentacaoFinanceira(
                investidor,
                new DateTime(2026, 1, 16),
                "APORTE",
                500)
        };

        var resultado = CalculadoraPerformanceCarteira.Calcular(
            historicos,
            fluxos);

        Assert.Equal(0m, resultado.GanhoLiquido);
        Assert.Equal(0m, resultado.RentabilidadeAcumulada);
        Assert.Equal(500m, resultado.Aportes);
    }

    [Fact]
    public void Retirada_nao_e_tratada_como_prejuizo()
    {
        var investidor = new Investidor("Teste");
        var historicos = new[]
        {
            new HistoricoPatrimonio(investidor, new DateTime(2026, 1, 1), 1000),
            new HistoricoPatrimonio(investidor, new DateTime(2026, 2, 1), 800)
        };
        var fluxos = new[]
        {
            new MovimentacaoFinanceira(
                investidor,
                new DateTime(2026, 1, 16),
                "RETIRADA",
                200)
        };

        var resultado = CalculadoraPerformanceCarteira.Calcular(
            historicos,
            fluxos);

        Assert.Equal(0m, resultado.GanhoLiquido);
        Assert.Equal(0m, resultado.RentabilidadeAcumulada);
        Assert.Equal(200m, resultado.Retiradas);
    }
}
