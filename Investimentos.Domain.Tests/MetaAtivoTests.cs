using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;

namespace Investimentos.Domain.Tests
{
    public class MetaAtivoTests
    {
        [Fact]
        public void DeveCriarEAtualizarMeta()
        {
            var classe = new ClasseAtivo(
                "RV",
                "Renda Variável");
            var tipo = new TipoAtivo(
                "ACAO",
                "Ação",
                classe);
            var ativo = new Ativo(
                new Ticker("TEST3"),
                "Teste",
                tipo);
            var investidor =
                new Investidor("Investidor");

            var meta = new MetaAtivo(
                investidor,
                ativo,
                1000m);

            Assert.Equal(
                1000m,
                meta.QuantidadeDesejada);

            meta.Atualizar(1500m);

            Assert.Equal(
                1500m,
                meta.QuantidadeDesejada);
        }

        [Fact]
        public void NaoDeveAceitarQuantidadeInvalida()
        {
            var classe = new ClasseAtivo(
                "RV",
                "Renda Variável");
            var tipo = new TipoAtivo(
                "ACAO",
                "Ação",
                classe);
            var ativo = new Ativo(
                new Ticker("TEST3"),
                "Teste",
                tipo);
            var investidor =
                new Investidor("Investidor");

            Assert.Throws<ArgumentException>(
                () => new MetaAtivo(
                    investidor,
                    ativo,
                    0));
        }
    }
}
