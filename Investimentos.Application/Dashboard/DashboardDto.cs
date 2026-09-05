using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Proventos.ConsultarProventos;

namespace Investimentos.Application.Dashboard
{
    public record EvolucaoPontoDto(DateTime Data, decimal Carteira);

    public record EvolucaoInvestidorDto(
        string Investidor,
        IReadOnlyList<EvolucaoPontoDto> Pontos);

    public record SaldoInvestidorDto(string Investidor, decimal Valor);

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
        decimal DescontosFiscais = 0)
    {
        [Obsolete("Use ValorAplicado.")]
        public decimal PatrimonioPorCusto => ValorAplicado;
    }
}
