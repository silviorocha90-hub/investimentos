using Investimentos.Application.Ativos.CadastrarAtivo;
using Investimentos.Application.Interfaces;
using Investimentos.Domain.Entities;

namespace Investimentos.Application.Tests.Ativos
{
    public class CadastrarAtivoHandlerTests
    {
        [Fact]
        public async Task DeveCadastrarAtivo()
        {
            var repository = new FakeAtivoRepository();

            var handler =
                new CadastrarAtivoHandler(repository);

            var command =
                new CadastrarAtivoCommand(
                    "itub4",
                    "Itaú Unibanco",
                    "ACAO");

            var id = await handler.HandleAsync(command);

            Assert.NotEqual(Guid.Empty, id);
            Assert.Single(repository.Ativos);
            Assert.Equal(
                "ITUB4",
                repository.Ativos[0].Ticker.Codigo);
        }

        [Fact]
        public async Task NaoDeveCadastrarTickerDuplicado()
        {
            var repository = new FakeAtivoRepository();

            var handler =
                new CadastrarAtivoHandler(repository);

            await handler.HandleAsync(
                new CadastrarAtivoCommand(
                    "ITUB4",
                    "Itaú Unibanco",
                    "ACAO"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.HandleAsync(
                    new CadastrarAtivoCommand(
                        "itub4",
                        "Outro nome",
                        "ACAO")));

            Assert.Single(repository.Ativos);
        }

        [Fact]
        public async Task NaoDeveCadastrarTipoInexistente()
        {
            var repository = new FakeAtivoRepository();

            var handler =
                new CadastrarAtivoHandler(repository);

            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleAsync(
                    new CadastrarAtivoCommand(
                        "ITUB4",
                        "Itaú Unibanco",
                        "TIPO_INEXISTENTE")));

            Assert.Empty(repository.Ativos);
        }

        private class FakeAtivoRepository
            : IAtivoRepository
        {
            public List<Ativo> Ativos { get; } = [];

            private readonly TipoAtivo _acao;

            public FakeAtivoRepository()
            {
                var rendaVariavel =
                    new ClasseAtivo(
                        "RENDA_VARIAVEL",
                        "Renda Variável");

                _acao =
                    new TipoAtivo(
                        "ACAO",
                        "Ação",
                        rendaVariavel);
            }

            public Task AdicionarAsync(
                Ativo ativo,
                CancellationToken cancellationToken = default)
            {
                Ativos.Add(ativo);

                return Task.CompletedTask;
            }

            public Task<bool> ExistePorTickerAsync(
                string ticker,
                CancellationToken cancellationToken = default)
            {
                var existe = Ativos.Any(
                    x => string.Equals(
                        x.Ticker.Codigo,
                        ticker,
                        StringComparison.OrdinalIgnoreCase));

                return Task.FromResult(existe);
            }

            public Task<TipoAtivo?> ObterTipoAtivoPorCodigoAsync(
                string codigo,
                CancellationToken cancellationToken = default)
            {
                TipoAtivo? resultado =
                    string.Equals(
                        _acao.Codigo,
                        codigo,
                        StringComparison.OrdinalIgnoreCase)
                        ? _acao
                        : null;

                return Task.FromResult(resultado);
            }
        }
    }
}