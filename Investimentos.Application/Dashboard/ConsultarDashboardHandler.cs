using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Proventos.ConsultarProventos;
using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Dashboard
{
    public class ConsultarDashboardHandler
    {
        private readonly ConsultarCarteiraHandler _carteiraHandler;
        private readonly ConsultarProventosHandler _proventosHandler;
        private readonly ConsultarOpcoesHandler _opcoesHandler;
        private readonly ICotacaoAtivoRepository _cotacaoRepository;
        private readonly IDescontoFiscalRepository _descontoRepository;

        public ConsultarDashboardHandler(
            ConsultarCarteiraHandler carteiraHandler,
            ConsultarProventosHandler proventosHandler,
            ConsultarOpcoesHandler opcoesHandler,
            ICotacaoAtivoRepository cotacaoRepository,
            IDescontoFiscalRepository descontoRepository)
        {
            _carteiraHandler = carteiraHandler;
            _proventosHandler = proventosHandler;
            _opcoesHandler = opcoesHandler;
            _cotacaoRepository = cotacaoRepository;
            _descontoRepository = descontoRepository;
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

            var cotacoes = await _cotacaoRepository
                .ObterUltimasPorTickerAsync(cancellationToken);

            var valorAplicado = posicoes
                .Where(x =>
                    x.Quantidade > 0 &&
                    x.Ticker != "PREV")
                .Sum(x => x.Quantidade *
                    (cotacoes.TryGetValue(x.Ticker, out var preco)
                        ? preco
                        : 0));

            var totalProventos =
                proventos.Sum(
                    x => x.ValorRecebido);

            var premioLiquidoOpcoes =
                opcoes.Where(x => x.Situacao == "ENCERRADA").Sum(x =>
                    x.ResultadoInformado ?? 0);

            var descontosFiscais =
                await _descontoRepository.ObterTotalAsync(
                    investidorId,
                    cancellationToken);

            premioLiquidoOpcoes -= descontosFiscais;

            var valorizacaoAtivos = posicoes
                .Where(x =>
                    x.Quantidade > 0 &&
                    x.Ticker != "PREV" &&
                    x.DataPrimeiraCompra.HasValue &&
                    x.DataPrimeiraCompra.Value < DateTime.Today.AddMonths(-1) &&
                    cotacoes.TryGetValue(x.Ticker, out _))
                .Sum(x =>
                {
                    var precoAtual = cotacoes[x.Ticker];
                    return (x.Quantidade * precoAtual) - x.CustoTotal;
                });

            var resultadoRealizado =
                premioLiquidoOpcoes +
                totalProventos +
                valorizacaoAtivos;

            /*
             * O desconto fiscal e separado do MyProfit para preservar
             * o resultado original da operacao no banco.
             */

            var quantidadeAtivos =
                posicoes.Count(
                    x => x.Quantidade > 0 &&
                        x.Ticker != "PREV");

            var quantidadeOpcoesAbertas =
                opcoes.Count(
                    x => x.Situacao == "ABERTA");

            return new DashboardDto(
                valorAplicado,
                resultadoRealizado,
                totalProventos,
                premioLiquidoOpcoes,
                quantidadeAtivos,
                quantidadeOpcoesAbertas,
                posicoes,
                proventos,
                opcoes,
                DescontosFiscais: descontosFiscais);
        }
    }
}
