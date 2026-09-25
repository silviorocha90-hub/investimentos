using Investimentos.Application.Dashboard;
using Xunit;

namespace Investimentos.Application.Tests.Dashboard
{
    public class CalculadoraResultadoCarteiraTests
    {
        [Fact]
        public void DeveSomarValorizacaoProventosEOpcoes()
        {
            var resultado =
                CalculadoraResultadoCarteira.CalcularResultado(
                    -52.90m,
                    85.20m,
                    0m);

            Assert.Equal(
                32.30m,
                resultado);
        }

        [Fact]
        public void NaoDeveIncluirResultadoRealizadoDeAcoes()
        {
            var resultado =
                CalculadoraResultadoCarteira.CalcularResultado(
                    -100m,
                    25m,
                    10m);

            Assert.Equal(
                -65m,
                resultado);
        }

        [Fact]
        public void DeveCalcularRentabilidadeSobreCapitalBase()
        {
            var resultadoCarteira = 32.30m;
            var patrimonioAtual = 6373.44m;

            var rentabilidade =
                CalculadoraResultadoCarteira.CalcularRentabilidade(
                    patrimonioAtual,
                    resultadoCarteira);

            var capitalBase =
                patrimonioAtual -
                resultadoCarteira;

            Assert.Equal(
                resultadoCarteira /
                capitalBase * 100,
                rentabilidade);
        }

        [Theory]
        [InlineData(100, 100)]
        [InlineData(100, 150)]
        public void DeveRetornarZeroQuandoCapitalBaseNaoForPositivo(
            decimal patrimonioAtual,
            decimal resultadoCarteira)
        {
            Assert.Equal(
                0m,
                CalculadoraResultadoCarteira.CalcularRentabilidade(
                    patrimonioAtual,
                    resultadoCarteira));
        }

        [Fact]
        public void DevePreservarResultadoNegativo()
        {
            var resultado =
                CalculadoraResultadoCarteira.CalcularResultado(
                    -250m,
                    40m,
                    60m);

            Assert.Equal(
                -150m,
                resultado);
        }

        [Fact]
        public void DeveAceitarResultadoNegativoDeOpcoes()
        {
            var resultado =
                CalculadoraResultadoCarteira.CalcularResultado(
                    100m,
                    20m,
                    -50m);

            Assert.Equal(
                70m,
                resultado);
        }

        [Fact]
        public void DeveRetornarZeroQuandoNaoHaResultado()
        {
            Assert.Equal(
                0m,
                CalculadoraResultadoCarteira.CalcularResultado(
                    0m,
                    0m,
                    0m));
        }

        [Fact]
        public void DeveCalcularRentabilidadeNegativa()
        {
            var rentabilidade =
                CalculadoraResultadoCarteira.CalcularRentabilidade(
                    900m,
                    -100m);

            Assert.Equal(
                -10m,
                rentabilidade);
        }

        [Fact]
        public void CapitalBaseDeveSerPatrimonioMenosResultado()
        {
            Assert.Equal(
                1000m,
                CalculadoraResultadoCarteira.CalcularCapitalBase(
                    900m,
                    -100m));

            Assert.Equal(
                800m,
                CalculadoraResultadoCarteira.CalcularCapitalBase(
                    900m,
                    100m));
        }
    }
}
