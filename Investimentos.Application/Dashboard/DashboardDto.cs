using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Proventos.ConsultarProventos;

namespace Investimentos.Application.Dashboard
{
    public record EvolucaoPontoDto(
        DateTime Data,
        decimal Carteira);

    public record EvolucaoInvestidorDto(
        string Investidor,
        IReadOnlyList<EvolucaoPontoDto> Pontos);

    public record SaldoInvestidorDto(
        string Investidor,
        decimal Valor);

    public record DistribuicaoTipoAtivoDto(
        string Codigo,
        string Nome,
        decimal Valor,
        decimal Percentual);

    public record DashboardDto(
        decimal ValorAplicado,
        decimal ResultadoRealizado,
        decimal TotalProventos,
        decimal PremioLiquidoOpcoes,
        int QuantidadeAtivos,
        int QuantidadeOpcoesAbertas,
        IReadOnlyList<PosicaoAtivoDto> Posicoes,
        IReadOnlyList<ProventoDto> Proventos,
        IReadOnlyList<OperacaoOpcaoDto> Opcoes,
        decimal PatrimonioEstimado = 0,
        decimal CaixaDisponivel = 0,
        IReadOnlyList<SaldoInvestidorDto>? SaldosDisponiveis = null,
        decimal DescontosFiscais = 0,
        IReadOnlyList<DistribuicaoTipoAtivoDto>? DistribuicaoPorTipo = null,
        decimal ValorizacaoAtivos = 0,
        decimal ProventosBrutos = 0,
        decimal IrProventos = 0,
        decimal OpcoesBrutas = 0,
        decimal IrEstimadoOpcoes = 0,
        decimal ResultadoRealizadoAcoes = 0)
    {
        [Obsolete("Use ValorAplicado.")]
        public decimal PatrimonioPorCusto =>
            ValorAplicado;
    }
}