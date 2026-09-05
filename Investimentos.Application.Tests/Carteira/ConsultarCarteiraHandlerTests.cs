using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Tests.Carteira
{
    public class ConsultarCarteiraHandlerTests
    {
        [Fact]
        public async Task DeveConsultarCarteira()
        {
            var investidorId = Guid.NewGuid();

            var repository =
                new FakeCarteiraRepository();

            var handler =
                new ConsultarCarteiraHandler(repository);

            var resultado =
                await handler.HandleAsync(investidorId);

            Assert.Single(resultado);

            Assert.Equal(
                "ITUB4",
                resultado[0].Ticker);

            Assert.Equal(
                100,
                resultado[0].Quantidade);

            Assert.Equal(
                40.00m,
                resultado[0].PrecoMedio);

            Assert.Equal(
                4000.00m,
                resultado[0].CustoTotal);
        }

        [Fact]
        public async Task NaoDeveConsultarSemInvestidor()
        {
            var repository =
                new FakeCarteiraRepository();

            var handler =
                new ConsultarCarteiraHandler(repository);

            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleAsync(Guid.Empty));
        }

        private class FakeCarteiraRepository
            : ICarteiraRepository
        {
            public Task<IReadOnlyList<PosicaoAtivoDto>>
                ObterPosicoesAsync(
                    Guid investidorId,
                    CancellationToken cancellationToken = default)
            {
                IReadOnlyList<PosicaoAtivoDto> resultado =
                [
                    new PosicaoAtivoDto(
                        "ITUB4",
                        "Itaú Unibanco",
                        100,
                        40.00m,
                        4000.00m)
                ];

                return Task.FromResult(resultado);
            }
        }
    }
}