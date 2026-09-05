using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;

namespace Investimentos.Domain.Tests.Entities
{
    public class OperacaoTests
    {
        [Fact]
        public void DeveCriarOperacaoValida()
        {
            var investidor = CriarInvestidor();
            var ativo = CriarAtivo();
            var tipoOperacao = CriarTipoOperacao();
            var data = new DateTime(2026, 9, 4);

            var operacao = new Operacao(
                data,
                1,
                investidor,
                ativo,
                tipoOperacao,
                100,
                40.00m,
                2.50m);

            Assert.NotEqual(Guid.Empty, operacao.Id);
            Assert.Equal(data, operacao.Data);
            Assert.Equal(1, operacao.Sequencia);
            Assert.Equal(investidor, operacao.Investidor);
            Assert.Equal(ativo, operacao.Ativo);
            Assert.Equal(tipoOperacao, operacao.TipoOperacao);
            Assert.Equal(100, operacao.Quantidade);
            Assert.Equal(40.00m, operacao.PrecoUnitario);
            Assert.Equal(2.50m, operacao.Taxas);
            Assert.Equal(4000.00m, operacao.ValorBruto);
        }

        [Fact]
        public void DeveCriarOperacaoComTaxaZeroPorPadrao()
        {
            var operacao = new Operacao(
                new DateTime(2026, 9, 4),
                1,
                CriarInvestidor(),
                CriarAtivo(),
                CriarTipoOperacao(),
                100,
                40.00m);

            Assert.Equal(0, operacao.Taxas);
        }

        [Fact]
        public void DeveCalcularValorBruto()
        {
            var operacao = new Operacao(
                new DateTime(2026, 9, 4),
                1,
                CriarInvestidor(),
                CriarAtivo(),
                CriarTipoOperacao(),
                150,
                42.50m);

            Assert.Equal(6375.00m, operacao.ValorBruto);
        }

        [Fact]
        public void NaoDeveCriarOperacaoComSequenciaZero()
        {
            Assert.Throws<ArgumentException>(() =>
                new Operacao(
                    new DateTime(2026, 9, 4),
                    0,
                    CriarInvestidor(),
                    CriarAtivo(),
                    CriarTipoOperacao(),
                    100,
                    40.00m));
        }

        [Fact]
        public void NaoDeveCriarOperacaoComSequenciaNegativa()
        {
            Assert.Throws<ArgumentException>(() =>
                new Operacao(
                    new DateTime(2026, 9, 4),
                    -1,
                    CriarInvestidor(),
                    CriarAtivo(),
                    CriarTipoOperacao(),
                    100,
                    40.00m));
        }

        [Fact]
        public void NaoDeveCriarOperacaoSemInvestidor()
        {
            Assert.Throws<ArgumentException>(() =>
                new Operacao(
                    new DateTime(2026, 9, 4),
                    1,
                    null!,
                    CriarAtivo(),
                    CriarTipoOperacao(),
                    100,
                    40.00m));
        }

        [Fact]
        public void NaoDeveCriarOperacaoSemAtivo()
        {
            Assert.Throws<ArgumentException>(() =>
                new Operacao(
                    new DateTime(2026, 9, 4),
                    1,
                    CriarInvestidor(),
                    null!,
                    CriarTipoOperacao(),
                    100,
                    40.00m));
        }

        [Fact]
        public void NaoDeveCriarOperacaoSemTipoOperacao()
        {
            Assert.Throws<ArgumentException>(() =>
                new Operacao(
                    new DateTime(2026, 9, 4),
                    1,
                    CriarInvestidor(),
                    CriarAtivo(),
                    null!,
                    100,
                    40.00m));
        }

        [Fact]
        public void NaoDeveCriarOperacaoComQuantidadeZero()
        {
            Assert.Throws<ArgumentException>(() =>
                new Operacao(
                    new DateTime(2026, 9, 4),
                    1,
                    CriarInvestidor(),
                    CriarAtivo(),
                    CriarTipoOperacao(),
                    0,
                    40.00m));
        }

        [Fact]
        public void NaoDeveCriarOperacaoComQuantidadeNegativa()
        {
            Assert.Throws<ArgumentException>(() =>
                new Operacao(
                    new DateTime(2026, 9, 4),
                    1,
                    CriarInvestidor(),
                    CriarAtivo(),
                    CriarTipoOperacao(),
                    -100,
                    40.00m));
        }

        [Fact]
        public void DevePermitirPrecoUnitarioZero()
        {
            var operacao = new Operacao(
                new DateTime(2026, 9, 4),
                1,
                CriarInvestidor(),
                CriarAtivo(),
                CriarTipoOperacao(),
                100,
                0);

            Assert.Equal(0, operacao.PrecoUnitario);
            Assert.Equal(0, operacao.ValorBruto);
        }

        [Fact]
        public void NaoDeveCriarOperacaoComPrecoUnitarioNegativo()
        {
            Assert.Throws<ArgumentException>(() =>
                new Operacao(
                    new DateTime(2026, 9, 4),
                    1,
                    CriarInvestidor(),
                    CriarAtivo(),
                    CriarTipoOperacao(),
                    100,
                    -1));
        }

        [Fact]
        public void NaoDeveCriarOperacaoComTaxaNegativa()
        {
            Assert.Throws<ArgumentException>(() =>
                new Operacao(
                    new DateTime(2026, 9, 4),
                    1,
                    CriarInvestidor(),
                    CriarAtivo(),
                    CriarTipoOperacao(),
                    100,
                    40.00m,
                    -1));
        }

        private static Investidor CriarInvestidor()
        {
            return new Investidor("Silvio");
        }

        private static Ativo CriarAtivo()
        {
            var classeAtivo = new ClasseAtivo(
                "RENDA_VARIAVEL",
                "Renda Variável");

            var tipoAtivo = new TipoAtivo(
                "ACAO",
                "Ação",
                classeAtivo);

            return new Ativo(
                new Ticker("ITUB4"),
                "Itaú Unibanco",
                tipoAtivo);
        }

        private static TipoOperacao CriarTipoOperacao()
        {
            return new TipoOperacao(
                "COMPRA",
                "Compra");
        }
    }
}