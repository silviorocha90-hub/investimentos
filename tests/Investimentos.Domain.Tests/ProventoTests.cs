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
                    "Dividendo ITUB4",
                    new DateTime(2026, 8, 10),
                    new DateTime(2026, 8, 20),
                    200,
                    0.50m,
                    100.00m);

            Assert.Equal(
                "DIVIDENDO",
                provento.Tipo);

            Assert.Equal(
                "Dividendo ITUB4",
                provento.Descricao);

            Assert.Equal(
                100.00m,
                provento.ValorBruto);

            Assert.Equal(
                100.00m,
                provento.ValorRecebido);

            Assert.Equal(
                0.00m,
                provento.ImpostoRetido);
        }

        [Fact]
        public void DeveCalcularImpostoRetido()
        {
            var investidor =
                new Investidor("Silvio");

            var ativo =
                CriarAtivo();

            var provento =
                new Provento(
                    investidor,
                    ativo,
                    "JCP",
                    null,
                    null,
                    new DateTime(2026, 8, 20),
                    85,
                    0.36m,
                    26.25m);

            Assert.Equal(
                30.60m,
                provento.ValorBruto);

            Assert.Equal(
                26.25m,
                provento.ValorRecebido);

            Assert.Equal(
                4.35m,
                provento.ImpostoRetido);

            Assert.Null(
                provento.DataCom);

            Assert.Null(
                provento.Descricao);
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
                        null,
                        new DateTime(2026, 8, 10),
                        new DateTime(2026, 8, 20),
                        200,
                        0.50m,
                        100.00m));
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