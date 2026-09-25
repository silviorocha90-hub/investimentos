using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Application.Dashboard;
using Investimentos.Application.Interfaces;
using Investimentos.Application.Opcoes.ConsultarOpcoes;
using Investimentos.Application.Proventos.ConsultarProventos;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;
using Xunit;

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
                            new DateTime(
                                2026,
                                9,
                                1),
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
                            new DateTime(
                                2026,
                                9,
                                2),
                            1)
                    });

            var provento =
                new Provento(
                    investidor,
                    ativo,
                    "DIVIDENDO",
                    "Dividendo ITUB4",
                    new DateTime(
                        2026,
                        8,
                        10),
                    new DateTime(
                        2026,
                        8,
                        20),
                    100,
                    0.50m,
                    50.00m);

            var proventoRepository =
                new FakeProventoRepository(
                    investidor,
                    ativo,
                    new[]
                    {
                        provento
                    });

            var opcao =
                new OperacaoOpcao(
                    investidor,
                    ativo,
                    "ITUBU407",
                    "PUT",
                    "VENDA",
                    new DateTime(
                        2026,
                        9,
                        4),
                    new DateTime(
                        2026,
                        9,
                        18),
                    40.03m,
                    8,
                    800,
                    0.22m);

            var opcaoRepository =
                new FakeOperacaoOpcaoRepository(
                    investidor,
                    ativo,
                    new[]
                    {
                        opcao
                    });

            var cotacaoRepository =
                new FakeCotacaoAtivoRepository(
                    new Dictionary<
                        string,
                        decimal>
                    {
                        ["ITUB4"] = 40.00m
                    });

            var saldoRepository =
                new FakeSaldoDisponivelRepository(
                    investidor,
                    1000.00m);

            var carteiraHandler =
                new ConsultarCarteiraHandler(
                    carteiraRepository,
                    opcaoRepository,
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
                    cotacaoRepository,
                    saldoRepository,
                    new FakeMovimentacaoFinanceiraRepository());

            var resultado =
                await handler.HandleAsync(
                    investidor.Id);

            /*
             * 50 ações restantes x R$ 40,00
             */
            Assert.Equal(
                2000.00m,
                resultado.ValorAplicado);

            /*
             * Caixa disponível do investidor.
             */
            Assert.Equal(
                1000.00m,
                resultado.CaixaDisponivel);

            /*
             * Patrimônio =
             *
             * Valor Aplicado
             * + Previdência
             * + Caixa
             *
             * Neste teste não existe Previdência.
             *
             * R$ 2.000,00 + R$ 1.000,00
             * = R$ 3.000,00.
             */
            Assert.Equal(
                3000.00m,
                resultado.PatrimonioEstimado);

            /*
             * Desconto fiscal pertence somente
             * ao dashboard consolidado.
             */
            Assert.Equal(
                0m,
                resultado.DescontosFiscais);

            Assert.Equal(
                50.00m,
                resultado.TotalProventos);

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

            /*
             * A venda realizada de ações não entra no
             * resultado econômico padrão. Como a cotação
             * atual é igual ao PM da posição restante,
             * valorização = 0 e o resultado é somente
             * o provento líquido de R$ 50,00.
             */
            Assert.Equal(
                50.00m,
                resultado.ResultadoCarteira);

            Assert.Equal(
                250.00m,
                resultado.ResultadoRealizadoAcoes);

            var posicao =
                Assert.Single(
                    resultado.Posicoes);

            Assert.Equal(
                50.00m,
                posicao.Proventos);

            Assert.Equal(
                0m,
                posicao.ResultadoOpcoes);

            Assert.Equal(
                50.00m,
                posicao.ResultadoEconomico);

            Assert.Equal(
                2.50m,
                posicao.RentabilidadeEconomica);
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
                IReadOnlyList<OperacaoCarteiraDto>
                    operacoes)
            {
                _operacoes = operacoes;
            }

            public Task<
                IReadOnlyList<OperacaoCarteiraDto>>
                ObterOperacoesAsync(
                    Guid investidorId,
                    CancellationToken
                        cancellationToken = default)
            {
                return Task.FromResult(
                    _operacoes);
            }
        }

        private class FakeProventoRepository
            : IProventoRepository
        {
            private readonly Investidor
                _investidor;

            private readonly Ativo
                _ativo;

            private readonly List<Provento>
                _proventos;

            public FakeProventoRepository(
                Investidor investidor,
                Ativo ativo,
                IReadOnlyList<Provento>
                    proventos)
            {
                _investidor =
                    investidor;

                _ativo =
                    ativo;

                _proventos =
                    proventos.ToList();
            }

            public Task AdicionarAsync(
                Provento provento,
                CancellationToken
                    cancellationToken = default)
            {
                _proventos.Add(
                    provento);

                return Task.CompletedTask;
            }

            public Task<Provento?>
                ObterPorIdAsync(
                    Guid id,
                    CancellationToken
                        cancellationToken = default)
            {
                var provento =
                    _proventos
                        .FirstOrDefault(
                            x =>
                                x.Id ==
                                id);

                return Task.FromResult<
                    Provento?>(
                    provento);
            }

            public Task<Investidor?>
                ObterInvestidorAsync(
                    Guid investidorId,
                    CancellationToken
                        cancellationToken = default)
            {
                return Task.FromResult<
                    Investidor?>(
                    _investidor);
            }

            public Task<Ativo?>
                ObterAtivoPorTickerAsync(
                    string ticker,
                    CancellationToken
                        cancellationToken = default)
            {
                return Task.FromResult<
                    Ativo?>(
                    _ativo);
            }

            public Task<
                IReadOnlyList<Provento>>
                ListarAsync(
                    Guid investidorId,
                    CancellationToken
                        cancellationToken = default)
            {
                IReadOnlyList<Provento>
                    resultado =
                        _proventos;

                return Task.FromResult(
                    resultado);
            }

            public void Excluir(
                Provento provento)
            {
                _proventos.Remove(
                    provento);
            }

            public Task SalvarAlteracoesAsync(
                CancellationToken
                    cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        private class FakeOperacaoOpcaoRepository
            : IOperacaoOpcaoRepository
        {
            private readonly Investidor
                _investidor;

            private readonly Ativo
                _ativo;

            private readonly
                IReadOnlyList<OperacaoOpcao>
                _operacoes;

            public FakeOperacaoOpcaoRepository(
                Investidor investidor,
                Ativo ativo,
                IReadOnlyList<OperacaoOpcao>
                    operacoes)
            {
                _investidor =
                    investidor;

                _ativo =
                    ativo;

                _operacoes =
                    operacoes;
            }

            public Task AdicionarAsync(
                OperacaoOpcao operacao,
                CancellationToken
                    cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<Investidor?>
                ObterInvestidorAsync(
                    Guid investidorId,
                    CancellationToken
                        cancellationToken = default)
            {
                return Task.FromResult<
                    Investidor?>(
                    _investidor);
            }

            public Task<Ativo?>
                ObterAtivoPorTickerAsync(
                    string ticker,
                    CancellationToken
                        cancellationToken = default)
            {
                return Task.FromResult<
                    Ativo?>(
                    _ativo);
            }

            public Task<
                IReadOnlyList<OperacaoOpcao>>
                ListarAsync(
                    Guid investidorId,
                    CancellationToken
                        cancellationToken = default)
            {
                return Task.FromResult(
                    _operacoes);
            }
        }

        private class FakeCotacaoAtivoRepository
            : ICotacaoAtivoRepository
        {
            private readonly
                IReadOnlyDictionary<
                    string,
                    decimal>
                _cotacoes;

            public FakeCotacaoAtivoRepository(
                IReadOnlyDictionary<
                    string,
                    decimal> cotacoes)
            {
                _cotacoes =
                    cotacoes;
            }

            public Task<
                IReadOnlyDictionary<
                    string,
                    decimal>>
                ObterUltimasPorTickerAsync(
                    CancellationToken
                        cancellationToken = default)
            {
                return Task.FromResult(
                    _cotacoes);
            }
        }

        private class FakeMovimentacaoFinanceiraRepository
            : IMovimentacaoFinanceiraRepository
        {
            public Task<IReadOnlyList<MovimentacaoFinanceira>>
                ListarAnoAsync(
                    Guid investidorId,
                    int ano,
                    CancellationToken cancellationToken = default)
            {
                IReadOnlyList<MovimentacaoFinanceira> resultado =
                    Array.Empty<MovimentacaoFinanceira>();

                return Task.FromResult(resultado);
            }
        }

        private class FakeSaldoDisponivelRepository
            : ISaldoDisponivelRepository
        {
            private readonly Investidor
                _investidor;

            private readonly decimal
                _valor;

            private SaldoDisponivel?
                _saldoDisponivel;

            public FakeSaldoDisponivelRepository(
                Investidor investidor,
                decimal valor)
            {
                _investidor =
                    investidor;

                _valor =
                    valor;

                _saldoDisponivel =
                    new SaldoDisponivel(
                        investidor,
                        DateTime.Today,
                        valor);
            }

            public Task<decimal>
                ObterTotalAtualAsync(
                    CancellationToken
                        cancellationToken = default)
            {
                return Task.FromResult(
                    _valor);
            }

            public Task<
                IReadOnlyList<SaldoInvestidorDto>>
                ListarAtuaisAsync(
                    CancellationToken
                        cancellationToken = default)
            {
                IReadOnlyList<
                    SaldoInvestidorDto>
                    saldos =
                        new[]
                        {
                            new SaldoInvestidorDto(
                                _investidor.Nome,
                                _valor)
                        };

                return Task.FromResult(
                    saldos);
            }

            public Task<SaldoDisponivel?>
                ObterAtualAsync(
                    Guid investidorId,
                    CancellationToken
                        cancellationToken = default)
            {
                return Task.FromResult(
                    _saldoDisponivel);
            }

            public Task AdicionarAsync(
                SaldoDisponivel saldoDisponivel,
                CancellationToken
                    cancellationToken = default)
            {
                _saldoDisponivel =
                    saldoDisponivel;

                return Task.CompletedTask;
            }

            public Task SalvarAlteracoesAsync(
                CancellationToken
                    cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}