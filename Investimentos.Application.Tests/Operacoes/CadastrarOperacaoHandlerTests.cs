using Investimentos.Application.Interfaces;
using Investimentos.Application.Operacoes.CadastrarOperacao;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;

namespace Investimentos.Application.Tests.Operacoes
{
    public class CadastrarOperacaoHandlerTests
    {
        [Fact]
        public async Task DeveCadastrarOperacao()
        {
            var repository =
                new FakeOperacaoRepository();

            var handler =
                new CadastrarOperacaoHandler(
                    repository);

            var command =
                new CadastrarOperacaoCommand(
                    new DateTime(2026, 9, 4),
                    repository.Investidor.Id,
                    "itub4",
                    "compra",
                    100,
                    40.00m,
                    0);

            var id =
                await handler.HandleAsync(command);

            Assert.NotEqual(
                Guid.Empty,
                id);

            Assert.Single(
                repository.Operacoes);

            var operacao =
                repository.Operacoes[0];

            Assert.Equal(
                1,
                operacao.Sequencia);

            Assert.Equal(
                100,
                operacao.Quantidade);

            Assert.Equal(
                40.00m,
                operacao.PrecoUnitario);

            Assert.Equal(
                4000.00m,
                operacao.ValorBruto);
        }

        [Fact]
        public async Task NaoDeveCadastrarComInvestidorInexistente()
        {
            var repository =
                new FakeOperacaoRepository();

            var handler =
                new CadastrarOperacaoHandler(
                    repository);

            var command =
                new CadastrarOperacaoCommand(
                    new DateTime(2026, 9, 4),
                    Guid.NewGuid(),
                    "ITUB4",
                    "COMPRA",
                    100,
                    40.00m,
                    0);

            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleAsync(command));

            Assert.Empty(
                repository.Operacoes);
        }

        [Fact]
        public async Task NaoDeveCadastrarComAtivoInexistente()
        {
            var repository =
                new FakeOperacaoRepository();

            var handler =
                new CadastrarOperacaoHandler(
                    repository);

            var command =
                new CadastrarOperacaoCommand(
                    new DateTime(2026, 9, 4),
                    repository.Investidor.Id,
                    "XXXX3",
                    "COMPRA",
                    100,
                    40.00m,
                    0);

            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleAsync(command));

            Assert.Empty(
                repository.Operacoes);
        }

        [Fact]
        public async Task NaoDeveCadastrarQuantidadeInvalida()
        {
            var repository =
                new FakeOperacaoRepository();

            var handler =
                new CadastrarOperacaoHandler(
                    repository);

            var command =
                new CadastrarOperacaoCommand(
                    new DateTime(2026, 9, 4),
                    repository.Investidor.Id,
                    "ITUB4",
                    "COMPRA",
                    0,
                    40.00m,
                    0);

            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleAsync(command));

            Assert.Empty(
                repository.Operacoes);
        }

        [Fact]
        public async Task DeveIncrementarSequencia()
        {
            var repository =
                new FakeOperacaoRepository();

            var handler =
                new CadastrarOperacaoHandler(
                    repository);

            var data =
                new DateTime(2026, 9, 4);

            await handler.HandleAsync(
                new CadastrarOperacaoCommand(
                    data,
                    repository.Investidor.Id,
                    "ITUB4",
                    "COMPRA",
                    100,
                    40.00m,
                    0));

            await handler.HandleAsync(
                new CadastrarOperacaoCommand(
                    data,
                    repository.Investidor.Id,
                    "ITUB4",
                    "COMPRA",
                    100,
                    42.00m,
                    0));

            Assert.Equal(
                1,
                repository.Operacoes[0].Sequencia);

            Assert.Equal(
                2,
                repository.Operacoes[1].Sequencia);
        }

        private class FakeOperacaoRepository
            : IOperacaoRepository
        {
            public Investidor Investidor { get; }

            public Ativo Ativo { get; }

            public TipoOperacao TipoOperacaoCompra { get; }

            public TipoOperacao TipoOperacaoVenda { get; }

            public List<Operacao> Operacoes { get; } = [];

            public FakeOperacaoRepository()
            {
                Investidor =
                    new Investidor(
                        "Silvio");

                var classe =
                    new ClasseAtivo(
                        "RENDA_VARIAVEL",
                        "Renda Variável");

                var tipoAtivo =
                    new TipoAtivo(
                        "ACAO",
                        "Ação",
                        classe);

                Ativo =
                    new Ativo(
                        new Ticker("ITUB4"),
                        "Itaú Unibanco",
                        tipoAtivo);

                TipoOperacaoCompra =
                    new TipoOperacao(
                        "COMPRA",
                        "Compra");

                TipoOperacaoVenda =
                    new TipoOperacao(
                        "VENDA",
                        "Venda");
            }

            public Task AdicionarAsync(
                Operacao operacao,
                CancellationToken cancellationToken = default)
            {
                Operacoes.Add(
                    operacao);

                return Task.CompletedTask;
            }

            public Task<Investidor?>
                ObterInvestidorPorIdAsync(
                    Guid investidorId,
                    CancellationToken cancellationToken = default)
            {
                Investidor? resultado =
                    Investidor.Id == investidorId
                        ? Investidor
                        : null;

                return Task.FromResult(
                    resultado);
            }

            public Task<Ativo?>
                ObterAtivoPorTickerAsync(
                    string ticker,
                    CancellationToken cancellationToken = default)
            {
                Ativo? resultado =
                    string.Equals(
                        Ativo.Ticker.Codigo,
                        ticker,
                        StringComparison.OrdinalIgnoreCase)
                        ? Ativo
                        : null;

                return Task.FromResult(
                    resultado);
            }

            public Task<TipoOperacao?>
                ObterTipoOperacaoPorCodigoAsync(
                    string codigo,
                    CancellationToken cancellationToken = default)
            {
                TipoOperacao? resultado =
                    codigo.Equals(
                        "COMPRA",
                        StringComparison.OrdinalIgnoreCase)
                        ? TipoOperacaoCompra
                        : codigo.Equals(
                            "VENDA",
                            StringComparison.OrdinalIgnoreCase)
                            ? TipoOperacaoVenda
                            : null;

                return Task.FromResult(
                    resultado);
            }

            public Task<int>
                ObterProximaSequenciaAsync(
                    Guid investidorId,
                    DateTime data,
                    CancellationToken cancellationToken = default)
            {
                var ultimaSequencia =
                    Operacoes
                        .Where(x =>
                            x.Investidor.Id ==
                                investidorId &&
                            x.Data.Date ==
                                data.Date)
                        .Select(x =>
                            x.Sequencia)
                        .DefaultIfEmpty(0)
                        .Max();

                return Task.FromResult(
                    ultimaSequencia + 1);
            }
        }
    }
}