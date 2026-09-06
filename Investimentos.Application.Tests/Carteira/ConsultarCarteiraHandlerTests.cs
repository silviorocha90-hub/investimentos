using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Tests.Carteira
{
    public class ConsultarCarteiraHandlerTests
    {
        [Fact]
        public async Task DeveConsultarCarteira()
        {
            var investidorId =
                Guid.NewGuid();

            var ativoId =
                Guid.NewGuid();

            var repository =
                new FakeCarteiraRepository(
                    new List<OperacaoCarteiraDto>
                    {
                        new OperacaoCarteiraDto(
                            Guid.NewGuid(),
                            ativoId,
                            "ITUB4",
                            "Itaú Unibanco",
                            "COMPRA",
                            100,
                            40.00m,
                            0,
                            new DateTime(2026, 9, 4),
                            1),

                        new OperacaoCarteiraDto(
                            Guid.NewGuid(),
                            ativoId,
                            "ITUB4",
                            "Itaú Unibanco",
                            "VENDA",
                            50,
                            45.00m,
                            0,
                            new DateTime(2026, 9, 4),
                            2)
                    });

            var service =
                new CalcularCarteiraService();

            var handler =
                new ConsultarCarteiraHandler(
                    repository,
                    service);

            var resultado =
                await handler.HandleAsync(
                    investidorId);

            Assert.Single(resultado);

            var posicao =
                resultado[0];

            Assert.Equal(
                "ITUB4",
                posicao.Ticker);

            Assert.Equal(
                50,
                posicao.Quantidade);

            Assert.Equal(
                40.00m,
                posicao.PrecoMedio);

            Assert.Equal(
                2000.00m,
                posicao.CustoTotal);

            Assert.Equal(
                250.00m,
                posicao.ResultadoRealizado);
        }

        [Fact]
        public async Task NaoDeveConsultarComInvestidorVazio()
        {
            var repository =
                new FakeCarteiraRepository([]);

            var service =
                new CalcularCarteiraService();

            var handler =
                new ConsultarCarteiraHandler(
                    repository,
                    service);

            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleAsync(
                    Guid.Empty));
        }

        private class FakeCarteiraRepository
            : ICarteiraRepository
        {
            private readonly
                IReadOnlyList<OperacaoCarteiraDto>
                _operacoes;

            public FakeCarteiraRepository(
                IReadOnlyList<OperacaoCarteiraDto> operacoes)
            {
                _operacoes = operacoes;
            }

            public Task<IReadOnlyList<OperacaoCarteiraDto>>
                ObterOperacoesAsync(
                    Guid investidorId,
                    CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    _operacoes);
            }
        }
    }
}