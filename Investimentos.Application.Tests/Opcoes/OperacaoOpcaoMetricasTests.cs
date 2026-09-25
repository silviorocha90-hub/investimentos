using Investimentos.Application.Opcoes.ConsultarOpcoes;

namespace Investimentos.Application.Tests.Opcoes
{
    public class OperacaoOpcaoMetricasTests
    {
        [Fact]
        public void PutVendaAtivaCalculaCapitalPremioERisco()
        {
            var opcao =
                Criar(
                    "PUT",
                    "VENDA",
                    "EXECUTADA",
                    40m,
                    200m,
                    1.50m) with
                {
                    ValorAcaoAtual = 38m
                };

            Assert.True(opcao.EstaAtiva);
            Assert.Equal(
                8000m,
                opcao.CapitalComprometidoPut);
            Assert.Equal(
                298m,
                opcao.PremioRecebidoAtivo);
            Assert.True(
                opcao.EmRiscoExercicio);
        }

        [Fact]
        public void CallVendaAtivaCalculaAcoesComprometidasERisco()
        {
            var opcao =
                Criar(
                    "CALL",
                    "VENDA",
                    "ABERTA",
                    50m,
                    300m,
                    2m) with
                {
                    ValorAcaoAtual = 52m
                };

            Assert.Equal(
                300m,
                opcao.AcoesComprometidasCall);
            Assert.True(
                opcao.EmRiscoExercicio);
        }

        [Fact]
        public void OpcaoEncerradaNaoComprometeCapitalOuAcoes()
        {
            var opcao =
                Criar(
                    "PUT",
                    "VENDA",
                    "ENCERRADA",
                    40m,
                    200m,
                    1.50m);

            Assert.False(opcao.EstaAtiva);
            Assert.Equal(
                0m,
                opcao.CapitalComprometidoPut);
            Assert.Equal(
                0m,
                opcao.PremioRecebidoAtivo);
            Assert.False(
                opcao.EmRiscoExercicio);
        }

        private static OperacaoOpcaoDto Criar(
            string tipo,
            string natureza,
            string situacao,
            decimal strike,
            decimal quantidade,
            decimal premio)
        {
            return new OperacaoOpcaoDto(
                Guid.NewGuid(),
                "TEST3",
                "TESTX",
                tipo,
                natureza,
                new DateTime(2026, 9, 1),
                new DateTime(2026, 10, 16),
                null,
                strike,
                (int)(quantidade / 100m),
                quantidade,
                premio,
                premio * quantidade,
                2m,
                null,
                null,
                null,
                null,
                null,
                situacao);
        }
    }
}
