using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;

namespace Investimentos.Domain.Tests.Entities
{
    public class ProventoTests
    {
        [Fact]
        public void DeveCriarProvento()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

            var provento =
                new Provento(
                    investidor,
                    ativo,
                    "DIVIDENDO",
                    new DateTime(2026, 8, 10),
                    new DateTime(2026, 8, 20),
                    200,
                    0.50m);

            Assert.Equal(
                "DIVIDENDO",
                provento.Tipo);

            Assert.Equal(
                100.00m,
                provento.ValorTotal);
        }

        [Fact]
        public void NaoDeveCriarTipoInvalido()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

            Assert.Throws<ArgumentException>(
                () =>
                    new Provento(
                        investidor,
                        ativo,
                        "INVALIDO",
                        new DateTime(2026, 8, 10),
                        new DateTime(2026, 8, 20),
                        200,
                        0.50m));
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