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
                    .ToList();

            foreach (var exercicio in exercicios)
            {
                var tipoOperacao =
                    exercicio.TipoOpcao == "PUT"
                        ? exercicio.Natureza == "VENDA"
                            ? "COMPRA"
                            : "VENDA"
                        : exercicio.Natureza == "VENDA"
                            ? "VENDA"
                            : "COMPRA";

                var dataExercicio =
                    exercicio.DataFinalizacao ??
                    exercicio.Vencimento;

                var operacaoExistente =
                    operacoes
                        .Select((operacao, indice) =>
                            new { operacao, indice })
                        .FirstOrDefault(x =>
                            x.operacao.AtivoId ==
                                exercicio.AtivoId &&
                            x.operacao.TipoOperacao ==
                                tipoOperacao &&
                            x.operacao.Quantidade ==
                                exercicio.Quantidade &&
                            x.operacao.Data.Date ==
                                dataExercicio.Date);

                if (operacaoExistente is null)
                {
                    continue;
                }

                var precoUnitario =
                    exercicio.ValorExecucao!.Value /
                    exercicio.Quantidade;

                var original =
                    operacaoExistente.operacao;

                operacoes[operacaoExistente.indice] =
                    original with
                    {
                        PrecoUnitario =
                            precoUnitario
                    };
            }

            return operacoes
                .OrderBy(x => x.Data)
                .ThenBy(x => x.Sequencia)
                .ToList();
        }
    }
}