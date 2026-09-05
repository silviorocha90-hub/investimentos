using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Carteira.ConsultarCarteira
{
    public class ConsultarCarteiraHandler
    {
        private readonly ICarteiraRepository _repository;

        public ConsultarCarteiraHandler(
            ICarteiraRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<PosicaoAtivoDto>> HandleAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default)
        {
            if (investidorId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }

            return await _repository.ObterPosicoesAsync(
                investidorId,
                cancellationToken);
        }
    }
}