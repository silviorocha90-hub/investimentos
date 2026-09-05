using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;

namespace Investimentos.Domain.Tests.Entities
{
    public class OperacaoOpcaoTests
    {
        [Fact]
        public void DeveCriarVendaDePut()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

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

            Assert.Equal(
                "PUT",
                opcao.TipoOpcao);

            Assert.Equal(
                "VENDA",
                opcao.Natureza);

            Assert.Equal(
                "ABERTA",
                opcao.Situacao);

            Assert.Equal(
                176.00m,
                opcao.PremioTotal);
        }

        [Fact]
        public void NaoDeveCriarTipoDeOpcaoInvalido()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

            Assert.Throws<ArgumentException>(
                () =>
                    new OperacaoOpcao(
                        investidor,
                        ativo,
                        "TESTE",
                        "INVALIDA",
                        "VENDA",
                        new DateTime(2026, 9, 4),
                        new DateTime(2026, 9, 18),
                        40,
                        1,
                        100,
                        1));
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
    }
}