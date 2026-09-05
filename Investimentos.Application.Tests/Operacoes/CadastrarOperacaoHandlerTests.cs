using Investimentos.Application.Interfaces;
using Investimentos.Application.Operacoes.CadastrarOperacao;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;

namespace Investimentos.Application.Tests.Operacoes
{
    public class CadastrarOperacaoHandlerTests
    {
        [Fact]
        public async Task DeveCadastrarCompra()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

            var tipoOperacao =
                new TipoOperacao(
                    "COMPRA",
                    "Compra");

            var repository =
                new FakeOperacaoRepository(
                    investidor,
                    ativo,
                    tipoOperacao);

            var handler =
                new CadastrarOperacaoHandler(
                    repository);

            var command =
                new CadastrarOperacaoCommand(
                    new DateTime(2026, 9, 4),
                    investidor.Id,
                    "ITUB4",
                    "COMPRA",
                    100,
                    40.00m,
                    0);

            var id =
                await handler.HandleAsync(command);

            Assert.NotEqual(Guid.Empty, id);
            Assert.NotNull(repository.OperacaoAdicionada);

            Assert.Equal(
                100,
                repository.OperacaoAdicionada!.Quantidade);

            Assert.Equal(
                1,
                repository.OperacaoAdicionada.Sequencia);
        }

        [Fact]
        public async Task DeveCadastrarVendaComPosicaoSuficiente()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

            var tipoOperacao =
                new TipoOperacao(
                    "VENDA",
                    "Venda");

            var repository =
                new FakeOperacaoRepository(
                    investidor,
                    ativo,
                    tipoOperacao,
                    quantidadeDisponivel: 150);

            var handler =
                new CadastrarOperacaoHandler(
                    repository);

            var command =
                new CadastrarOperacaoCommand(
                    new DateTime(2026, 9, 5),
                    investidor.Id,
                    "ITUB4",
                    "VENDA",
                    50,
                    45.00m,
                    0);

            var id =
                await handler.HandleAsync(command);

            Assert.NotEqual(Guid.Empty, id);
            Assert.NotNull(repository.OperacaoAdicionada);

            Assert.Equal(
                50,
                repository.OperacaoAdicionada!.Quantidade);
        }

        [Fact]
        public async Task DevePermitirVendaIgualAPosicaoDisponivel()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

            var tipoOperacao =
                new TipoOperacao(
                    "VENDA",
                    "Venda");

            var repository =
                new FakeOperacaoRepository(
                    investidor,
                    ativo,
                    tipoOperacao,
                    quantidadeDisponivel: 150);

            var handler =
                new CadastrarOperacaoHandler(
                    repository);

            var command =
                new CadastrarOperacaoCommand(
                    new DateTime(2026, 9, 5),
                    investidor.Id,
                    "ITUB4",
                    "VENDA",
                    150,
                    45.00m,
                    0);

            await handler.HandleAsync(command);

            Assert.NotNull(
                repository.OperacaoAdicionada);
        }

        [Fact]
        public async Task NaoDeveCadastrarVendaMaiorQuePosicao()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

            var tipoOperacao =
                new TipoOperacao(
                    "VENDA",
                    "Venda");

            var repository =
                new FakeOperacaoRepository(
                    investidor,
                    ativo,
                    tipoOperacao,
                    quantidadeDisponivel: 150);

            var handler =
                new CadastrarOperacaoHandler(
                    repository);

            var command =
                new CadastrarOperacaoCommand(
                    new DateTime(2026, 9, 5),
                    investidor.Id,
                    "ITUB4",
                    "VENDA",
                    151,
                    45.00m,
                    0);

            var exception =
                await Assert.ThrowsAsync<
                    InvalidOperationException>(
                    () => handler.HandleAsync(command));

            Assert.Contains(
                "maior que a posição disponível",
                exception.Message);

            Assert.Null(
                repository.OperacaoAdicionada);
        }

        [Fact]
        public async Task NaoDeveCadastrarVendaSemPosicao()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

            var tipoOperacao =
                new TipoOperacao(
                    "VENDA",
                    "Venda");

            var repository =
                new FakeOperacaoRepository(
                    investidor,
                    ativo,
                    tipoOperacao,
                    quantidadeDisponivel: 0);

            var handler =
                new CadastrarOperacaoHandler(
                    repository);

            var command =
                new CadastrarOperacaoCommand(
                    new DateTime(2026, 9, 5),
                    investidor.Id,
                    "ITUB4",
                    "VENDA",
                    1,
                    45.00m,
                    0);

            var exception =
                await Assert.ThrowsAsync<
                    InvalidOperationException>(
                    () => handler.HandleAsync(command));

            Assert.Contains(
                "Não existe posição disponível",
                exception.Message);

            Assert.Null(
                repository.OperacaoAdicionada);
        }

        private static Ativo CriarAtivo()
        {
            var classeAtivo =
                new ClasseAtivo(
                    "RENDA_VARIAVEL",
                    "Renda Variável");

            var tipoAtivo =
                new TipoAtivo(
                    "ACAO",
                    "Ação",
                    classeAtivo);

            return new Ativo(
                new Ticker("ITUB4"),
                "Itaú Unibanco",
                tipoAtivo);
        }

        private class FakeOperacaoRepository
            : IOperacaoRepository
        {
            private readonly Investidor _investidor;
            private readonly Ativo _ativo;
            private readonly TipoOperacao _tipoOperacao;
            private readonly decimal _quantidadeDisponivel;

            public Operacao? OperacaoAdicionada
            {
                get;
                private set;
            }

            public FakeOperacaoRepository(
                Investidor investidor,
                Ativo ativo,
                TipoOperacao tipoOperacao,
                decimal quantidadeDisponivel = 0)
            {
                _investidor = investidor;
                _ativo = ativo;
                _tipoOperacao = tipoOperacao;
                _quantidadeDisponivel =
                    quantidadeDisponivel;
            }

            public Task AdicionarAsync(
                Operacao operacao,
                CancellationToken cancellationToken = default)
            {
                OperacaoAdicionada =
                    operacao;

                return Task.CompletedTask;
            }

            public Task<Investidor?> ObterInvestidorPorIdAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult<Investidor?>(
                    _investidor);
            }

            public Task<Ativo?> ObterAtivoPorTickerAsync(
                string ticker,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult<Ativo?>(
                    _ativo);
            }

            public Task<TipoOperacao?>
                ObterTipoOperacaoPorCodigoAsync(
                    string codigo,
                    CancellationToken cancellationToken = default)
            {
                return Task.FromResult<TipoOperacao?>(
                    _tipoOperacao);
            }

            public Task<int> ObterProximaSequenciaAsync(
                Guid investidorId,
                DateTime data,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(1);
            }

            public Task<decimal>
                ObterQuantidadeDisponivelAsync(
                    Guid investidorId,
                    Guid ativoId,
                    DateTime data,
                    int sequencia,
                    CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    _quantidadeDisponivel);
            }
        }
    }
}