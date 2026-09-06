using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Dashboard;
using Investimentos.Application.Interfaces;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Proventos.ConsultarProventos;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;

namespace Investimentos.Application.Tests.Dashboard
{
    public class ConsultarDashboardHandlerTests
    {
        [Fact]
        public async Task DeveConsolidarDashboard()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

            var carteiraRepository =
                new FakeCarteiraRepository(
                    new[]
                    {
                        new OperacaoCarteiraDto(
                            Guid.NewGuid(),
                            ativo.Id,
                            "ITUB4",
                            "Itaú Unibanco",
                            "COMPRA",
                            100,
                            40.00m,
                            0,
                            new DateTime(2026, 9, 1),
                            1),

                        new OperacaoCarteiraDto(
                            Guid.NewGuid(),
                            ativo.Id,
                            "ITUB4",
                            "Itaú Unibanco",
                            "VENDA",
                            50,
                            45.00m,
                            0,
                            new DateTime(2026, 9, 2),
                            1)
                    });

            var provento =
                new Provento(
                    investidor,
                    ativo,
                    "DIVIDENDO",
                    "Dividendo ITUB4",
                    new DateTime(2026, 8, 10),
                    new DateTime(2026, 8, 20),
                    100,
                    0.50m,
                    50.00m);

            var proventoRepository =
                new FakeProventoRepository(
                    investidor,
                    ativo,
                    new[] { provento });

            var opcao =
                new OperacaoOpcao(
                    investidor,
                    ativo,
                    "ITUBU407",
                    "PUT",
                    "VENDA",
                    new DateTime(2026, 9, 4),
                    new DateTime(2026, 9, 18),
                    40.03m,
                    8,
                    800,
                    0.22m);

            var opcaoRepository =
                new FakeOperacaoOpcaoRepository(
                    investidor,
                    ativo,
                    new[] { opcao });

            var carteiraHandler =
                new ConsultarCarteiraHandler(
                    carteiraRepository,
                    new CalcularCarteiraService());

            var proventosHandler =
                new ConsultarProventosHandler(
                    proventoRepository);

            var opcoesHandler =
                new ConsultarOpcoesHandler(
                    opcaoRepository);

            var handler =
                new ConsultarDashboardHandler(
                    carteiraHandler,
                    proventosHandler,
                    opcoesHandler,
                    null, // ICotacaoAtivoRepository not needed for this test
                    null  // IDescontoFiscalRepository not needed for this test
                );

            var resultado =
                await handler.HandleAsync(
                    investidor.Id);

            Assert.Equal(
                2000.00m,
                resultado.PatrimonioPorCusto);

            Assert.Equal(
                250.00m,
                resultado.ResultadoRealizado);

            Assert.Equal(
                50.00m,
                resultado.TotalProventos);

            Assert.Equal(
                176.00m,
                resultado.PremioLiquidoOpcoes);

            Assert.Equal(
                1,
                resultado.QuantidadeAtivos);

            Assert.Equal(
                1,
                resultado.QuantidadeOpcoesAbertas);

            Assert.Single(
                resultado.Posicoes);

            Assert.Single(
                resultado.Proventos);

            Assert.Single(
                resultado.Opcoes);
        }

        private static Ativo CriarAtivo()
        {
            var classe =
                new ClasseAtivo(
                    "RENDA_VARIAVEL",
                    "Renda Variável");

            var tipo =
                new TipoAtivo(
                    "ACAO",
                    "Ação",
                    classe);

            return new Ativo(
                new Ticker("ITUB4"),
                "Itaú Unibanco",
                tipo);
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

        private class FakeProventoRepository
            : IProventoRepository
        {
            private readonly Investidor _investidor;
            private readonly Ativo _ativo;
            private readonly IReadOnlyList<Provento> _proventos;

            public FakeProventoRepository(
                Investidor investidor,
                Ativo ativo,
                IReadOnlyList<Provento> proventos)
            {
                _investidor = investidor;
                _ativo = ativo;
                _proventos = proventos;
            }

            public Task AdicionarAsync(
                Provento provento,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<Investidor?> ObterInvestidorAsync(
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

            public Task<IReadOnlyList<Provento>>
                ListarAsync(
                    Guid investidorId,
                    CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    _proventos);
            }
        }

        private class FakeOperacaoOpcaoRepository
            : IOperacaoOpcaoRepository
        {
            private readonly Investidor _investidor;
            private readonly Ativo _ativo;
            private readonly
                IReadOnlyList<OperacaoOpcao>
                _operacoes;

            public FakeOperacaoOpcaoRepository(
                Investidor investidor,
                Ativo ativo,
                IReadOnlyList<OperacaoOpcao> operacoes)
            {
                _investidor = investidor;
                _ativo = ativo;
                _operacoes = operacoes;
            }

            public Task AdicionarAsync(
                OperacaoOpcao operacao,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<Investidor?> ObterInvestidorAsync(
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

            public Task<IReadOnlyList<OperacaoOpcao>>
                ListarAsync(
                    Guid investidorId,
                    CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    _operacoes);
            }
        }
    }
}