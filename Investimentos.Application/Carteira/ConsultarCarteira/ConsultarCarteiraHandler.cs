using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Carteira.ConsultarCarteira
{
    public class ConsultarCarteiraHandler
    {
        private readonly ICarteiraRepository _repository;
        private readonly IOperacaoOpcaoRepository _opcaoRepository;
        private readonly CalcularCarteiraService _calcularCarteiraService;

        public ConsultarCarteiraHandler(
            ICarteiraRepository repository,
            IOperacaoOpcaoRepository opcaoRepository,
            CalcularCarteiraService calcularCarteiraService)
        {
            _repository = repository;
            _opcaoRepository = opcaoRepository;
            _calcularCarteiraService =
                calcularCarteiraService;
        }

        public async Task<IReadOnlyList<PosicaoAtivoDto>>
            HandleAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default)
        {
            var operacoes =
                await ObterOperacoesAsync(
                    investidorId,
                    cancellationToken);

            return _calcularCarteiraService.Calcular(
                operacoes);
        }

        public async Task<decimal>
            HandleResultadoRealizadoAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default)
        {
            var operacoes =
                await ObterOperacoesAsync(
                    investidorId,
                    cancellationToken);

            return _calcularCarteiraService
                .CalcularResultadoRealizado(
                    operacoes);
        }

        public async Task<IReadOnlyList<OperacaoCarteiraDto>>
            ObterOperacoesAsync(
                Guid investidorId,
                CancellationToken cancellationToken)
        {
            if (investidorId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }

            var operacoes =
                (await _repository.ObterOperacoesAsync(
                    investidorId,
                    cancellationToken))
                .ToList();

            var opcoes =
                await _opcaoRepository.ListarAsync(
                    investidorId,
                    cancellationToken);

            var exercicios =
                opcoes
                    .Where(x =>
                        x.ValorExecucao.HasValue &&
                        x.ValorExecucao.Value > 0)
                    .Select(x =>
                    {
                        var tipoOperacao =
                            x.TipoOpcao == "PUT"
                                ? x.Natureza == "VENDA"
                                    ? "COMPRA"
                                    : "VENDA"
                                : x.Natureza == "VENDA"
                                    ? "VENDA"
                                    : "COMPRA";

                        return new OperacaoCarteiraDto(
                            x.Id,
                            x.AtivoId,
                            x.Ativo.Ticker.Codigo,
                            x.Ativo.Nome,
                            tipoOperacao,
                            x.Quantidade,
                            x.ValorExecucao!.Value,
                            0m,
                            x.DataFinalizacao ?? x.Vencimento,
                            int.MaxValue)
                        {
                            TipoAtivoCodigo =
                                x.Ativo.TipoAtivo.Codigo,
                            TipoAtivoNome =
                                x.Ativo.TipoAtivo.Nome
                        };
                    });

            operacoes.AddRange(
                exercicios);

            return operacoes
                .OrderBy(x => x.Data)
                .ThenBy(x => x.Sequencia)
                .ToList();
        }
    }
}