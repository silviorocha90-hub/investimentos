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
    public class ConsultarDashboardConsolidadoCalculosTests
    {
        [Fact]
        public async Task DeveConsolidarResultadoSemDescontarIrEstimadoDuasVezes()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

            /*
             * Compra:
             * 100 x 40 = 4.000
             *
             * Venda:
             * 50 x 45 = 2.250
             *
             * Resultado realizado:
             * 50 x 5 = 250
             *
             * Restam:
             * 50 ações a PM 40.
             */
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

            /*
             * Provento líquido = 50.
             */
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

            /*
             * Neste teste não precisamos de
             * opção real finalizada.
             *
             * O objetivo principal é validar
             * consolidação de posições, vendas,
             * proventos e descontos.
             */
            var opcaoRepository =
                new FakeOperacaoOpcaoRepository(
                    investidor,
                    ativo);

            /*
             * Cotação atual = 42.
             *
             * Posição:
             * 50 x 42 = 2.100
             *
             * Custo:
             * 50 x 40 = 2.000
             *
             * Valorização não realizada = 100.
             */
            var cotacaoRepository =
                new FakeCotacaoAtivoRepository(
                    new Dictionary<
                        string,
                        decimal>
                    {
                        ["ITUB4"] = 42.00m
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

            var dashboardHandler =
                new ConsultarDashboardHandler(
                    carteiraHandler,
                    proventosHandler,
                    opcoesHandler,
                    cotacaoRepository,
                    saldoRepository,
                    new FakeMovimentacaoFinanceiraRepository());

            var investidorRepository =
                new FakeInvestidorRepository(
                    new[]
                    {
                        investidor
                    });

            var descontoRepository =
                new FakeDescontoFiscalRepository(
                    0);

            var handler =
                new ConsultarDashboardConsolidadoHandler(
                    investidorRepository,
                    dashboardHandler,
                    saldoRepository,
                    cotacaoRepository,
                    descontoRepository);

            var resultado =
                await handler.HandleAsync();

            /*
             * Valor atual:
             * 50 x 42 = 2.100
             */
            Assert.Equal(
                2100.00m,
                resultado.ValorAplicado);

            /*
             * Patrimônio:
             * 2.100 + 1.000 de caixa
             */
            Assert.Equal(
                3100.00m,
                resultado.PatrimonioEstimado);

            /*
             * Venda realizada = 250.
             */
            Assert.Equal(
                250.00m,
                resultado.ResultadoRealizadoAcoes);

            /*
             * Valorização não realizada = 100.
             */
            Assert.Equal(
                100.00m,
                resultado.ValorizacaoAtivos);

            /*
             * Proventos = 50.
             */
            Assert.Equal(
                50.00m,
                resultado.TotalProventos);

            /*
             * Sem opções finalizadas.
             */
            Assert.Equal(
                0m,
                resultado.OpcoesBrutas);

            Assert.Equal(
                0m,
                resultado.PremioLiquidoOpcoes);

            /*
             * Resultado econômico da carteira:
             *
             * 100 valorização
             * + 50 provento
             * + 0 opções
             * = 150
             *
             * O lucro realizado de 250 na venda de ações
             * permanece separado e não compõe este indicador.
             */
            Assert.Equal(
                150.00m,
                resultado.ResultadoCarteira);

            Assert.Equal(
                resultado.ResultadoCarteira,
                resultado.ResultadoRealizado);

            Assert.Equal(
                100.00m,
                resultado.Posicoes.Single().Valorizacao);

            Assert.Equal(
                50.00m,
                resultado.Posicoes.Single().Proventos);

            Assert.Equal(
                150.00m,
                resultado.Posicoes.Single().ResultadoEconomico);

            Assert.Equal(
                7.50m,
                resultado.Posicoes.Single().RentabilidadeEconomica);

            Assert.Equal(
                2.50m,
                resultado.Posicoes.Single().YieldProventos);
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

        private class FakeInvestidorRepository
            : IInvestidorRepository
        {
            private readonly
                IReadOnlyList<Investidor>
                _investidores;

            public FakeInvestidorRepository(
                IReadOnlyList<Investidor>
                    investidores)
            {
                _investidores =
                    investidores;
            }

            public Task AdicionarAsync(
                Investidor investidor,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<bool> ExistePorNomeAsync(
                string nome,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    _investidores.Any(x =>
                        x.Nome == nome));
            }

            public Task<IReadOnlyList<Investidor>>
                ListarAsync(
                    CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    _investidores);
            }
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
                _operacoes =
                    operacoes;
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
                CancellationToken cancellationToken = default)
            {
                _proventos.Add(
                    provento);

                return Task.CompletedTask;
            }

            public Task<Provento?> ObterPorIdAsync(
                Guid id,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    _proventos.FirstOrDefault(
                        x => x.Id == id));
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
                CancellationToken cancellationToken = default)
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

            public FakeOperacaoOpcaoRepository(
                Investidor investidor,
                Ativo ativo)
            {
                _investidor =
                    investidor;

                _ativo =
                    ativo;
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
                IReadOnlyList<OperacaoOpcao>
                    operacoes =
                        Array.Empty<OperacaoOpcao>();

                return Task.FromResult(
                    operacoes);
            }
        }

        private class FakeCotacaoAtivoRepository
            : ICotacaoAtivoRepository
        {
            private readonly
                IReadOnlyDictionary<string, decimal>
                _cotacoes;

            public FakeCotacaoAtivoRepository(
                IReadOnlyDictionary<string, decimal>
                    cotacoes)
            {
                _cotacoes =
                    cotacoes;
            }

            public Task<IReadOnlyDictionary<string, decimal>>
                ObterUltimasPorTickerAsync(
                    CancellationToken cancellationToken = default)
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

            public FakeSaldoDisponivelRepository(
                Investidor investidor,
                decimal valor)
            {
                _investidor =
                    investidor;

                _valor =
                    valor;
            }

            public Task<decimal> ObterTotalAtualAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    _valor);
            }

            public Task<IReadOnlyList<SaldoInvestidorDto>>
                ListarAtuaisAsync(
                    CancellationToken cancellationToken = default)
            {
                IReadOnlyList<SaldoInvestidorDto>
                    resultado =
                        new[]
                        {
                            new SaldoInvestidorDto(
                                _investidor.Nome,
                                _valor)
                        };

                return Task.FromResult(
                    resultado);
            }

            public Task<SaldoDisponivel?> ObterAtualAsync(
                Guid investidorId,
                CancellationToken cancellationToken = default)
            {
                SaldoDisponivel saldo =
                    new(
                        _investidor,
                        DateTime.Today,
                        _valor);

                return Task.FromResult<SaldoDisponivel?>(
                    saldo);
            }

            public Task AdicionarAsync(
                SaldoDisponivel saldoDisponivel,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task SalvarAlteracoesAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        private class FakeDescontoFiscalRepository
            : IDescontoFiscalRepository
        {
            private readonly decimal
                _total;

            public FakeDescontoFiscalRepository(
                decimal total)
            {
                _total =
                    total;
            }

            public Task AdicionarAsync(
                DescontoFiscal desconto,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<IReadOnlyList<DescontoFiscal>>
                ListarAsync(
                    CancellationToken cancellationToken = default)
            {
                IReadOnlyList<DescontoFiscal>
                    descontos =
                        Array.Empty<DescontoFiscal>();

                return Task.FromResult(
                    descontos);
            }

            public Task<DescontoFiscal?> ObterPorIdAsync(
                Guid id,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult<DescontoFiscal?>(
                    null);
            }

            public void Excluir(
                DescontoFiscal desconto)
            {
            }

            public Task SalvarAlteracoesAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<decimal> ObterTotalAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    _total);
            }
        }
    }
}