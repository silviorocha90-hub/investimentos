using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Carteira.ConsultarCarteira
{
    public class ConsultarCarteiraHandler
    {
        private readonly ICarteiraRepository _repository;
        private readonly CalcularCarteiraService _calcularCarteiraService;

        public ConsultarCarteiraHandler(
            ICarteiraRepository repository,
            CalcularCarteiraService calcularCarteiraService)
        {
            _repository = repository;
            _calcularCarteiraService =
                calcularCarteiraService;
        }

        public async Task<IReadOnlyList<PosicaoAtivoDto>>
            HandleAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default)
        {
            if (investidorId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }

            var operacoes =
                await _repository.ObterOperacoesAsync(
                    investidorId,
                    cancellationToken);

            return _calcularCarteiraService.Calcular(
                operacoes);
        }
    }
}