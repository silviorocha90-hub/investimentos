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
            var ativoId = Guid.NewGuid();

            var operacoes = new[]
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
                _service.Calcular(operacoes);

            Assert.Single(resultado);

            var posicao = resultado[0];

            Assert.Equal(100, posicao.Quantidade);
            Assert.Equal(40.00m, posicao.PrecoMedio);
            Assert.Equal(4000.00m, posicao.CustoTotal);
            Assert.Equal(0, posicao.ResultadoRealizado);
        }

        [Fact]
        public void DeveCalcularPrecoMedioDeDuasCompras()
        {
            var ativoId = Guid.NewGuid();

            var operacoes = new[]
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
                _service.Calcular(operacoes);

            Assert.Single(resultado);

            var posicao = resultado[0];

            Assert.Equal(200, posicao.Quantidade);
            Assert.Equal(41.00m, posicao.PrecoMedio);
            Assert.Equal(8200.00m, posicao.CustoTotal);
            Assert.Equal(0, posicao.ResultadoRealizado);
        }

        [Fact]
        public void DeveCalcularLucroRealizadoEmVendaParcial()
        {
            var ativoId = Guid.NewGuid();

            var operacoes = new[]
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
                _service.Calcular(operacoes);

            Assert.Single(resultado);

            var posicao = resultado[0];

            Assert.Equal(150, posicao.Quantidade);
            Assert.Equal(41.00m, posicao.PrecoMedio);
            Assert.Equal(6150.00m, posicao.CustoTotal);
            Assert.Equal(200.00m, posicao.ResultadoRealizado);
        }

        [Fact]
        public void DeveCalcularPrejuizoRealizado()
        {
            var ativoId = Guid.NewGuid();

            var operacoes = new[]
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
                    50,
                    35.00m,
                    0,
                    2)
            };

            var resultado =
                _service.Calcular(operacoes);

            Assert.Single(resultado);

            var posicao = resultado[0];

            Assert.Equal(50, posicao.Quantidade);
            Assert.Equal(40.00m, posicao.PrecoMedio);
            Assert.Equal(2000.00m, posicao.CustoTotal);
            Assert.Equal(-250.00m, posicao.ResultadoRealizado);
        }

        [Fact]
        public void DeveConsiderarTaxaNaCompra()
        {
            var ativoId = Guid.NewGuid();

            var operacoes = new[]
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
                _service.Calcular(operacoes);

            Assert.Single(resultado);

            var posicao = resultado[0];

            Assert.Equal(100, posicao.Quantidade);
            Assert.Equal(40.10m, posicao.PrecoMedio);
            Assert.Equal(4010.00m, posicao.CustoTotal);
            Assert.Equal(0, posicao.ResultadoRealizado);
        }

        [Fact]
        public void DeveConsiderarTaxaNaVenda()
        {
            var ativoId = Guid.NewGuid();

            var operacoes = new[]
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
                    50,
                    45.00m,
                    10.00m,
                    2)
            };

            var resultado =
                _service.Calcular(operacoes);

            Assert.Single(resultado);

            var posicao = resultado[0];

            Assert.Equal(50, posicao.Quantidade);
            Assert.Equal(40.00m, posicao.PrecoMedio);
            Assert.Equal(2000.00m, posicao.CustoTotal);

            Assert.Equal(
                240.00m,
                posicao.ResultadoRealizado);
        }

        [Fact]
        public void DeveAcumularResultadoDeVariasVendas()
        {
            var ativoId = Guid.NewGuid();

            var operacoes = new[]
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
                    25,
                    44.00m,
                    0,
                    2),

                CriarOperacao(
                    ativoId,
                    "VENDA",
                    25,
                    46.00m,
                    0,
                    3)
            };

            var resultado =
                _service.Calcular(operacoes);

            Assert.Single(resultado);

            var posicao = resultado[0];

            Assert.Equal(50, posicao.Quantidade);
            Assert.Equal(40.00m, posicao.PrecoMedio);
            Assert.Equal(2000.00m, posicao.CustoTotal);
            Assert.Equal(250.00m, posicao.ResultadoRealizado);
        }

        [Fact]
        public void DeveManterResultadoMesmoComPosicaoZerada()
        {
            var ativoId = Guid.NewGuid();

            var operacoes = new[]
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
                _service.Calcular(operacoes);

            Assert.Single(resultado);

            var posicao = resultado[0];

            Assert.Equal(0, posicao.Quantidade);
            Assert.Equal(0, posicao.PrecoMedio);
            Assert.Equal(0, posicao.CustoTotal);
            Assert.Equal(500.00m, posicao.ResultadoRealizado);
        }

        [Fact]
        public void NaoDevePermitirVendaSemPosicao()
        {
            var ativoId = Guid.NewGuid();

            var operacoes = new[]
            {
                CriarOperacao(
                    ativoId,
                    "VENDA",
                    50,
                    45.00m,
                    0,
                    1)
            };

            var exception =
                Assert.Throws<InvalidOperationException>(
                    () => _service.Calcular(operacoes));

            Assert.Contains(
                "Não existe posição disponível",
                exception.Message);
        }

        [Fact]
        public void NaoDevePermitirVendaMaiorQuePosicao()
        {
            var ativoId = Guid.NewGuid();

            var operacoes = new[]
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
                    150,
                    45.00m,
                    0,
                    2)
            };

            var exception =
                Assert.Throws<InvalidOperationException>(
                    () => _service.Calcular(operacoes));

            Assert.Contains(
                "maior que a posição disponível",
                exception.Message);

            Assert.Contains(
                "Disponível: 100",
                exception.Message);

            Assert.Contains(
                "Venda: 150",
                exception.Message);
        }

        [Fact]
        public void DevePermitirVendaExatamenteIgualAPosicao()
        {
            var ativoId = Guid.NewGuid();

            var operacoes = new[]
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
                    40.00m,
                    0,
                    2)
            };

            var resultado =
                _service.Calcular(operacoes);

            Assert.Empty(resultado);
        }

        [Fact]
        public void DeveRespeitarSequenciaDasOperacoes()
        {
            var ativoId = Guid.NewGuid();
            var data = new DateTime(2026, 9, 4);

            var operacoes = new[]
            {
                new OperacaoCarteiraDto(
                    Guid.NewGuid(),
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
                    Guid.NewGuid(),
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
                    Guid.NewGuid(),
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
                _service.Calcular(operacoes);

            Assert.Single(resultado);

            var posicao = resultado[0];

            Assert.Equal(150, posicao.Quantidade);
            Assert.Equal(41.00m, posicao.PrecoMedio);
            Assert.Equal(6150.00m, posicao.CustoTotal);
            Assert.Equal(200.00m, posicao.ResultadoRealizado);
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
                Guid.NewGuid(),
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