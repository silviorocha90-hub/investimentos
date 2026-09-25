using Investimentos.Domain.Entities;

namespace Investimentos.Application.Performance;

/// <summary>
/// Calcula performance temporal por Modified Dietz.
/// Aportes são fluxos positivos e retiradas, negativos.
/// Cada intervalo usa os snapshots patrimoniais como limites e pondera
/// os fluxos pela fração do período em que permaneceram investidos.
/// Os retornos dos intervalos são encadeados geometricamente.
/// </summary>
public static class CalculadoraPerformanceCarteira
{
    public static PerformanceCarteiraDto Calcular(
        IEnumerable<HistoricoPatrimonio> historicos,
        IEnumerable<MovimentacaoFinanceira> movimentacoes)
    {
        var pontos = historicos
            .OrderBy(x => x.DataReferencia)
            .ToList();

        if (pontos.Count < 2)
        {
            return new PerformanceCarteiraDto(
                "Modified Dietz",
                pontos.FirstOrDefault()?.DataReferencia,
                pontos.LastOrDefault()?.DataReferencia,
                pontos.FirstOrDefault()?.ValorCarteira ?? 0,
                pontos.LastOrDefault()?.ValorCarteira ?? 0,
                0, 0, 0, 0,
                Array.Empty<PerformancePontoDto>());
        }

        var fluxos = movimentacoes
            .OrderBy(x => x.Data)
            .ToList();

        var periodos = new List<PerformancePontoDto>();
        decimal fatorAcumulado = 1m;

        for (var i = 1; i < pontos.Count; i++)
        {
            var inicio = pontos[i - 1];
            var fim = pontos[i];
            var dias = Math.Max(
                (fim.DataReferencia.Date - inicio.DataReferencia.Date).Days,
                1);

            var fluxosPeriodo = fluxos
                .Where(x =>
                    x.Data.Date > inicio.DataReferencia.Date &&
                    x.Data.Date <= fim.DataReferencia.Date)
                .ToList();

            var aportes = fluxosPeriodo
                .Where(x => x.Tipo == "APORTE")
                .Sum(x => x.Valor);

            var retiradas = fluxosPeriodo
                .Where(x => x.Tipo == "RETIRADA")
                .Sum(x => x.Valor);

            var fluxoLiquido = aportes - retiradas;
            decimal fluxosPonderados = 0;

            foreach (var fluxo in fluxosPeriodo)
            {
                var valorAssinado =
                    fluxo.Tipo == "APORTE"
                        ? fluxo.Valor
                        : -fluxo.Valor;

                var diasInvestidos = Math.Max(
                    (fim.DataReferencia.Date - fluxo.Data.Date).Days,
                    0);

                var peso = (decimal)diasInvestidos / dias;
                fluxosPonderados += valorAssinado * peso;
            }

            var ganhoLiquido =
                fim.ValorCarteira -
                inicio.ValorCarteira -
                fluxoLiquido;

            var denominador =
                inicio.ValorCarteira +
                fluxosPonderados;

            var retorno =
                denominador > 0
                    ? ganhoLiquido / denominador
                    : 0;

            fatorAcumulado *= 1 + retorno;

            periodos.Add(new PerformancePontoDto(
                inicio.DataReferencia,
                fim.DataReferencia,
                inicio.ValorCarteira,
                fim.ValorCarteira,
                aportes,
                retiradas,
                fluxoLiquido,
                ganhoLiquido,
                retorno * 100,
                (fatorAcumulado - 1) * 100));
        }

        var primeiro = pontos[0];
        var ultimo = pontos[^1];
        var aportesTotais = periodos.Sum(x => x.Aportes);
        var retiradasTotais = periodos.Sum(x => x.Retiradas);

        return new PerformanceCarteiraDto(
            "Modified Dietz",
            primeiro.DataReferencia,
            ultimo.DataReferencia,
            primeiro.ValorCarteira,
            ultimo.ValorCarteira,
            aportesTotais,
            retiradasTotais,
            ultimo.ValorCarteira -
                primeiro.ValorCarteira -
                (aportesTotais - retiradasTotais),
            (fatorAcumulado - 1) * 100,
            periodos);
    }
}
