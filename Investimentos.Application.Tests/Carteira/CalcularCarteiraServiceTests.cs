using Investimentos.Application.Carteira.ConsultarCarteira;

namespace Investimentos.Application.Tests.Carteira
{
    public class CalcularCarteiraServiceTests
    {
        private readonly CalcularCarteiraService _service =
            new();

        [Fact]
        public void DeveCalcularUmaCompra()
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
                        40.00m,
                        0,
                        1)
                };

            var resultado =
                _service.Calcular(
                    operacoes);

            Assert.Single(resultado);

            Assert.Equal(
                100,
                resultado[0].Quantidade);

            Assert.Equal(
                40.00m,
                resultado[0].PrecoMedio);

            Assert.Equal(
                4000.00m,
                resultado[0].CustoTotal);
        }

        [Fact]
        public void DeveCalcularPrecoMedioDeDuasCompras()
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
                        40.00m,
                        0,
                        1),

                    CriarOperacao(
                        ativoId,
                        "COMPRA",
                        100,
                        42.00m,
                        0,
                        2)
                };

            var resultado =
                _service.Calcular(
                    operacoes);

            Assert.Single(resultado);

            Assert.Equal(
                200,
                resultado[0].Quantidade);

            Assert.Equal(
                41.00m,
                resultado[0].PrecoMedio);

            Assert.Equal(
                8200.00m,
                resultado[0].CustoTotal);
        }

        [Fact]
        public void DeveManterPrecoMedioAposVendaParcial()
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
                        40.00m,
                        0,
                        1),

                    CriarOperacao(
                        ativoId,
                        "COMPRA",
                        100,
                        42.00m,
                        0,
                        2),

                    CriarOperacao(
                        ativoId,
                        "VENDA",
                        50,
                        45.00m,
                        0,
                        3)
                };

            var resultado =
                _service.Calcular(
                    operacoes);

            Assert.Single(resultado);

            Assert.Equal(
                150,
                resultado[0].Quantidade);

            Assert.Equal(
                41.00m,
                resultado[0].PrecoMedio);

            Assert.Equal(
                6150.00m,
                resultado[0].CustoTotal);
        }

        [Fact]
        public void DeveConsiderarTaxaNoCustoDaCompra()
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
                        40.00m,
                        10.00m,
                        1)
                };

            var resultado =
                _service.Calcular(
                    operacoes);

            Assert.Single(resultado);

            Assert.Equal(
                100,
                resultado[0].Quantidade);

            Assert.Equal(
                40.10m,
                resultado[0].PrecoMedio);

            Assert.Equal(
                4010.00m,
                resultado[0].CustoTotal);
        }

        [Fact]
        public void NaoDeveRetornarAtivoComPosicaoZerada()
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
                        40.00m,
                        0,
                        1),

                    CriarOperacao(
                        ativoId,
                        "VENDA",
                        100,
                        45.00m,
                        0,
                        2)
                };

            var resultado =
                _service.Calcular(
                    operacoes);

            Assert.Empty(resultado);
        }

        [Fact]
        public void DeveRespeitarSequenciaDasOperacoes()
        {
            var ativoId =
                Guid.NewGuid();

            var data =
                new DateTime(2026, 9, 4);

            var operacoes =
                new[]
                {
                    new OperacaoCarteiraDto(
                        ativoId,
                        "ITUB4",
                        "Itaú Unibanco",
                        "VENDA",
                        50,
                        45.00m,
                        0,
                        data,
                        3),

                    new OperacaoCarteiraDto(
                        ativoId,
                        "ITUB4",
                        "Itaú Unibanco",
                        "COMPRA",
                        100,
                        42.00m,
                        0,
                        data,
                        2),

                    new OperacaoCarteiraDto(
                        ativoId,
                        "ITUB4",
                        "Itaú Unibanco",
                        "COMPRA",
                        100,
                        40.00m,
                        0,
                        data,
                        1)
                };

            var resultado =
                _service.Calcular(
                    operacoes);

            Assert.Single(resultado);

            Assert.Equal(
                150,
                resultado[0].Quantidade);

            Assert.Equal(
                41.00m,
                resultado[0].PrecoMedio);

            Assert.Equal(
                6150.00m,
                resultado[0].CustoTotal);
        }

        private static OperacaoCarteiraDto CriarOperacao(
            Guid ativoId,
            string tipoOperacao,
            decimal quantidade,
            decimal precoUnitario,
            decimal taxas,
            int sequencia)
        {
            return new OperacaoCarteiraDto(
                ativoId,
                "ITUB4",
                "Itaú Unibanco",
                tipoOperacao,
                quantidade,
                precoUnitario,
                taxas,
                new DateTime(2026, 9, 4),
                sequencia);
        }
    }
}