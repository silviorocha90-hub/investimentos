using Investimentos.Application.Interfaces;
using Investimentos.Application.Investidores.CadastrarInvestidor;
using Investimentos.Domain.Entities;

namespace Investimentos.Application.Tests.Investidores
{
    public class CadastrarInvestidorHandlerTests
    {
        [Fact]
        public async Task DeveCadastrarInvestidor()
        {
            var repository =
                new FakeInvestidorRepository();

            var handler =
                new CadastrarInvestidorHandler(
                    repository);

            var command =
                new CadastrarInvestidorCommand(
                    "Silvio");

            var id =
                await handler.HandleAsync(command);

            Assert.NotEqual(
                Guid.Empty,
                id);

            Assert.Single(
                repository.Investidores);

            Assert.Equal(
                "Silvio",
                repository.Investidores[0].Nome);
        }

        [Fact]
        public async Task NaoDeveCadastrarInvestidorDuplicado()
        {
            var repository =
                new FakeInvestidorRepository();

            repository.Investidores.Add(
                new Investidor("Silvio"));

            var handler =
                new CadastrarInvestidorHandler(
                    repository);

            var command =
                new CadastrarInvestidorCommand(
                    "Silvio");

            await Assert.ThrowsAsync<
                InvalidOperationException>(
                () => handler.HandleAsync(command));

            Assert.Single(
                repository.Investidores);
        }

        [Fact]
        public async Task NaoDeveCadastrarInvestidorSemNome()
        {
            var repository =
                new FakeInvestidorRepository();

            var handler =
                new CadastrarInvestidorHandler(
                    repository);

            var command =
                new CadastrarInvestidorCommand(
                    "");

            await Assert.ThrowsAsync<
                ArgumentException>(
                () => handler.HandleAsync(command));

            Assert.Empty(
                repository.Investidores);
        }

        private class FakeInvestidorRepository
            : IInvestidorRepository
        {
            public List<Investidor> Investidores { get; }
                = [];

            public Task AdicionarAsync(
                Investidor investidor,
                CancellationToken cancellationToken = default)
            {
                Investidores.Add(
                    investidor);

                return Task.CompletedTask;
            }

            public Task<bool> ExistePorNomeAsync(
                string nome,
                CancellationToken cancellationToken = default)
            {
                var existe =
                    Investidores.Any(
                        x => string.Equals(
                            x.Nome,
                            nome,
                            StringComparison.OrdinalIgnoreCase));

                return Task.FromResult(
                    existe);
            }
        }
    }
}