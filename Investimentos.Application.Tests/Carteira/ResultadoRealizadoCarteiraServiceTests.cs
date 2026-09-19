using Investimentos.Application.Carteira.ConsultarCarteira;
using Xunit;

namespace Investimentos.Application.Tests.Carteira
{
    public class ResultadoRealizadoCarteiraServiceTests
    {
        private readonly CalcularCarteiraService _service =
            new();

        [Fact]
        public void DevePreservarResultadoRealizadoMesmoComPosicaoZerada()
        {
            var ativoId =
                Guid.NewGuid();

            var operacoes =
                new[]
                {
                    CriarOperacao(
                        ativoId,
                        "COMPRA",
                        100,
                        30.00m,
                        0,
                        1),

                    CriarOperacao(
                        ativoId,
                        "VENDA",
                        100,
                        35.00m,
                        0,
                        2)
                };

            var posicoes =
                _service.Calcular(
                    operacoes);

            var resultadoRealizado =
                _service
                    .CalcularResultadoRealizado(
                        operacoes);

            /*
             * A posição zerada continua sem
             * aparecer na carteira.
             */
            Assert.Empty(
                posicoes);

            /*
             * Mas o resultado realizado
             * continua existindo.
             */
            Assert.Equal(
                500.00m,
                resultadoRealizado);
        }

        [Fact]
        public void DeveSomarResultadoDePosicaoAbertaEZerada()
        {
            var itubId =
                Guid.NewGuid();

            var petrId =
                Guid.NewGuid();

            var operacoes =
                new[]
                {
                    CriarOperacao(
                        itubId,
                        "COMPRA",
                        100,
                        40.00m,
                        0,
                        1,
                        "ITUB4",
                        "Itaú Unibanco"),

                    CriarOperacao(
                        itubId,
                        "VENDA",
                        50,
                        45.00m,
                        0,
                        2,
                        "ITUB4",
                        "Itaú Unibanco"),

                    CriarOperacao(
                        petrId,
                        "COMPRA",
                        100,
                        30.00m,
                        0,
                        3,
                        "PETR4",
                        "Petrobras"),

                    CriarOperacao(
                        petrId,
                        "VENDA",
                        100,
                        35.00m,
                        0,
                        4,
                        "PETR4",
                        "Petrobras")
                };

            var posicoes =
                _service.Calcular(
                    operacoes);

            var resultadoRealizado =
                _service
                    .CalcularResultadoRealizado(
                        operacoes);

            /*
             * Apenas ITUB4 permanece
             * em carteira.
             */
            Assert.Single(
                posicoes);

            /*
             * ITUB4:
             * 50 x (45 - 40) = 250
             *
             * PETR4:
             * 100 x (35 - 30) = 500
             *
             * Total = 750
             */
            Assert.Equal(
                750.00m,
                resultadoRealizado);
        }

        [Fact]
        public void DeveConsiderarTaxasNoResultadoRealizado()
        {
            var ativoId =
                Guid.NewGuid();

            var operacoes =
                new[]
                {
                    CriarOperacao(
                        ativoId,
                        "COMPRA",
                        100,
                        30.00m,
                        10.00m,
                        1),

                    CriarOperacao(
                        ativoId,
                        "VENDA",
                        100,
                        35.00m,
                        5.00m,
                        2)
                };

            var resultadoRealizado =
                _service
                    .CalcularResultadoRealizado(
                        operacoes);

            /*
             * Compra:
             * 100 x 30 + 10 = 3.010
             *
             * Venda:
             * 100 x 35 = 3.500
             *
             * Resultado:
             * 3.500 - 3.010 - 5
             * = 485
             */
            Assert.Equal(
                485.00m,
                resultadoRealizado);
        }

        private static OperacaoCarteiraDto CriarOperacao(
            Guid ativoId,
            string tipoOperacao,
            decimal quantidade,
            decimal precoUnitario,
            decimal taxas,
            int sequencia,
            string ticker = "ITUB4",
            string nome = "Itaú Unibanco")
        {
            return new OperacaoCarteiraDto(
                Guid.NewGuid(),
                ativoId,
                ticker,
                nome,
                tipoOperacao,
                quantidade,
                precoUnitario,
                taxas,
                new DateTime(
                    2026,
                    9,
                    4),
                sequencia);
        }
    }
}