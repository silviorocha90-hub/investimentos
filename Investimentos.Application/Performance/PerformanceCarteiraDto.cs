namespace Investimentos.Application.Performance;

public record PerformancePontoDto(
    DateTime DataInicio,
    DateTime DataFim,
    decimal PatrimonioInicial,
    decimal PatrimonioFinal,
    decimal Aportes,
    decimal Retiradas,
    decimal FluxoLiquido,
    decimal GanhoLiquido,
    decimal RentabilidadePeriodo,
    decimal RentabilidadeAcumulada);

public record PerformanceCarteiraDto(
    string Metodologia,
    DateTime? DataInicio,
    DateTime? DataFim,
    decimal PatrimonioInicial,
    decimal PatrimonioFinal,
    decimal Aportes,
    decimal Retiradas,
    decimal GanhoLiquido,
    decimal RentabilidadeAcumulada,
    IReadOnlyList<PerformancePontoDto> Periodos);
