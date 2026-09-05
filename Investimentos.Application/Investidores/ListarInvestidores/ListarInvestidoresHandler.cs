using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Investidores.ListarInvestidores
{
    public class ListarInvestidoresHandler
    {
        private readonly IInvestidorRepository _investidorRepository;

        public ListarInvestidoresHandler(
            IInvestidorRepository investidorRepository)
        {
            _investidorRepository = investidorRepository;
        }

        public async Task<IReadOnlyList<InvestidorDto>> HandleAsync(
            CancellationToken cancellationToken = default)
        {
            var investidores =
                await _investidorRepository.ListarAsync(
                    cancellationToken);

            return investidores
                .Select(x => new InvestidorDto(
                    x.Id,
                    x.Nome))
                .ToList();
        }
    }
}