using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Proventos.ConsultarProventos;

namespace Investimentos.Application.Dashboard
{
    public class ConsultarDashboardHandler
    {
        private readonly ConsultarCarteiraHandler _carteiraHandler;
        private readonly ConsultarProventosHandler _proventosHandler;
        private readonly ConsultarOpcoesHandler _opcoesHandler;

        public ConsultarDashboardHandler(
            ConsultarCarteiraHandler carteiraHandler,
            ConsultarProventosHandler proventosHandler,
            ConsultarOpcoesHandler opcoesHandler)
        {
            _carteiraHandler = carteiraHandler;
            _proventosHandler = proventosHandler;
            _opcoesHandler = opcoesHandler;
        }

        public async Task<DashboardDto> HandleAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default)
        {
            if (investidorId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }

            var posicoes =
                await _carteiraHandler.HandleAsync(
                    investidorId,
                    cancellationToken);

            var proventos =
                await _proventosHandler.HandleAsync(
                    investidorId,
                    cancellationToken);

            var opcoes =
                await _opcoesHandler.HandleAsync(
                    investidorId,
                    cancellationToken);

            var patrimonioPorCusto =
                posicoes
                    .Where(x => x.Quantidade > 0)
                    .Sum(x => x.CustoTotal);

            var resultadoRealizado =
                posicoes.Sum(
                    x => x.ResultadoRealizado);

            var totalProventos =
                proventos.Sum(
                    x => x.ValorTotal);

            var premioLiquidoOpcoes =
                opcoes.Sum(x =>
                {
                    var premioLiquido =
                        x.PremioTotal - x.Taxas;

                    return x.Natureza == "VENDA"
                        ? premioLiquido
                        : -premioLiquido;
                });

            var quantidadeAtivos =
                posicoes.Count(
                    x => x.Quantidade > 0);

            var quantidadeOpcoesAbertas =
                opcoes.Count(
                    x => x.Situacao == "ABERTA");

            return new DashboardDto(
                patrimonioPorCusto,
                resultadoRealizado,
                totalProventos,
                premioLiquidoOpcoes,
                quantidadeAtivos,
                quantidadeOpcoesAbertas,
                posicoes,
                proventos,
                opcoes);
        }
    }
}