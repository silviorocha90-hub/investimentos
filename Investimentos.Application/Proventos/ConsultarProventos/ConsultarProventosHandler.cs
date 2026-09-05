using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Proventos.ConsultarProventos
{
    public class ConsultarProventosHandler
    {
        private readonly IProventoRepository _repository;

        public ConsultarProventosHandler(
            IProventoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<ProventoDto>>
            HandleAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default)
        {
            if (investidorId == Guid.Empty)
                throw new ArgumentException(
                    "O investidor é obrigatório.");

            var proventos =
                await _repository.ListarAsync(
                    investidorId,
                    cancellationToken);

            return proventos
                .Select(x =>
                    new ProventoDto(
                        x.Id,
                        x.Ativo.Ticker.Codigo,
                        x.Tipo,
                        x.DataCom,
                        x.DataPagamento,
                        x.QuantidadeBase,
                        x.ValorPorUnidade,
                        x.ValorTotal))
                .ToList();
        }
    }
}