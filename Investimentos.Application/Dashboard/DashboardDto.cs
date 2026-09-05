using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Proventos.ConsultarProventos;

namespace Investimentos.Application.Dashboard
{
    public record DashboardDto(
        decimal PatrimonioPorCusto,
        decimal ResultadoRealizado,
        decimal TotalProventos,
        decimal PremioLiquidoOpcoes,
        int QuantidadeAtivos,
        int QuantidadeOpcoesAbertas,
        IReadOnlyList<PosicaoAtivoDto> Posicoes,
        IReadOnlyList<ProventoDto> Proventos,
        IReadOnlyList<OperacaoOpcaoDto> Opcoes);
}