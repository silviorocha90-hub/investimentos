using Investimentos.Application.EstimativaIr;

namespace Investimentos.Application.Tests;
public class EstimativaIrTests
{
    private readonly Guid a = Guid.NewGuid(), b = Guid.NewGuid();
    private MovimentoIr M(Guid investidor, string tipo, DateTime data, decimal preco,
        string ticker = "TEST3", string categoria = "ACAO", decimal quantidade = 100) =>
        new(Guid.NewGuid(), investidor, investidor == a ? "Simone" : "Lucas", ticker,
            categoria, tipo, data, 1, quantidade, preco, 0);

    [Fact]
    public void ComprasIsoladasNaoGeramEncargo()
    {
        var r = CalculadoraEstimativaIr.Calcular(2026, [M(a, "COMPRA", new(2026, 1, 1), 10)]);
        Assert.Empty(r.Operacoes);
        Assert.Equal(0, r.Investidores.Single().Total);
    }
    [Fact]
    public void LimiteIsencaoUsaVendasDeTodoCpfMesmoComFiltroPorCarteira()
    {
        var r = CalculadoraEstimativaIr.Calcular(2026, [
            M(a, "COMPRA", new(2025, 1, 1), 100), M(b, "COMPRA", new(2025, 1, 1), 100),
            M(a, "VENDA", new(2026, 10, 26), 150), M(b, "VENDA", new(2026, 10, 27), 150)]);
        Assert.Equal(1500, r.Investidores.Sum(x => x.Estimativa));
        Assert.All(r.Operacoes, x => Assert.Equal("ESTIMADO", x.Situacao));
    }
    [Fact]
    public void IsencaoNoLimiteDeVinteMil()
    {
        var r = CalculadoraEstimativaIr.Calcular(2026, [
            M(a, "COMPRA", new(2025, 1, 1), 100), M(a, "VENDA", new(2026, 1, 2), 200)]);
        Assert.Equal("ISENTO", r.Operacoes.Single().Situacao);
        Assert.Equal(0, r.Investidores.Single().Total);
    }
    [Fact]
    public void PerdaAnoAnteriorCompensaOpcoesSemMisturarInvestidores()
    {
        var perda = M(a, "OPCAO ENCERRADA", new(2025, 12, 1), 1, categoria: "OPCAO") with { ResultadoInformado = -100 };
        var ganhoA = M(a, "OPCAO ENCERRADA", new(2026, 1, 1), 1, categoria: "OPCAO") with { ResultadoInformado = 200 };
        var ganhoB = M(b, "OPCAO ENCERRADA", new(2026, 1, 1), 1, categoria: "OPCAO") with { ResultadoInformado = 200 };
        var r = CalculadoraEstimativaIr.Calcular(2026, [perda, ganhoA, ganhoB]);
        Assert.Equal(15, r.Investidores.Single(x => x.InvestidorId == a).Estimativa);
        Assert.Equal(30, r.Investidores.Single(x => x.InvestidorId == b).Estimativa);
        Assert.Equal(2, r.Operacoes.Count);
    }
    [Fact]
    public void RetencaoDefinitivaNaoEDuplicadaNaEstimativa()
    {
        var compra = M(a, "COMPRA", new(2026, 10, 20), 100, "LFTS11", "ETF", 499);
        var venda = M(a, "VENDA", new(2026, 10, 27), 101, "LFTS11", "ETF", 499);
        var r = CalculadoraEstimativaIr.Calcular(2026, [compra, venda], [new(venda.Id, 62.28m, "Confirmado pelo titular")]);
        Assert.Equal(62.28m, r.Investidores.Single().Total);
        Assert.Equal(0, r.Investidores.Single().Estimativa);
        Assert.Equal("RETIDO CONFIRMADO", r.Operacoes.Single().Situacao);
    }
    [Fact]
    public void IntradiaEHistoricoInsuficienteFicamPendentes()
    {
        var r = CalculadoraEstimativaIr.Calcular(2026, [M(a, "COMPRA", new(2026, 1, 2), 10), M(a, "VENDA", new(2026, 1, 2), 11)]);
        Assert.Equal("PENDENTE", r.Operacoes.Single().Situacao);
        Assert.Equal(0, r.Investidores.Single().Total);
    }
    [Fact]
    public void FiiTemVintePorCentoSemIsencao()
    {
        var r = CalculadoraEstimativaIr.Calcular(2026, [M(a, "COMPRA", new(2026, 1, 1), 10, categoria: "FII"), M(a, "VENDA", new(2026, 1, 2), 11, categoria: "FII")]);
        Assert.Equal(20, r.Investidores.Single().Estimativa);
    }
    [Fact]
    public void RateioEmCentavosFechaComImpostoMensal()
    {
        var ops = Enumerable.Range(0, 3).Select(i => M(a, "OPCAO ENCERRADA", new(2026, 1, 2), 0, ticker: "OPT" + i, categoria: "OPCAO") with { ResultadoInformado = 0.04m });
        var r = CalculadoraEstimativaIr.Calcular(2026, ops);
        Assert.Equal(.02m, r.Operacoes.Sum(x => x.Estimativa));
    }
}
