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

        [Fact]
        public void DeveRetornarZeroQuandoCapitalBaseNaoForPositivo()
        {
            Assert.Equal(
                0m,
                CalculadoraResultadoCarteira.CalcularRentabilidade(
                    100m,
                    100m));
        }
    }
}
