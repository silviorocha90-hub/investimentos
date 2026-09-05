using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Importador.BancoDados
{
    public class VerificadorBancoDados
    {
        private const string BancoEsperado = "InvestimentosDb";

        private readonly InvestimentosDbContext _context;

        public VerificadorBancoDados(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoVerificacaoBanco> VerificarAsync()
        {
            var consegueConectar =
                await _context.Database.CanConnectAsync();

            if (!consegueConectar)
            {
                return new ResultadoVerificacaoBanco(
                    false,
                    null,
                    "Não foi possível conectar ao SQL Server.");
            }

            var bancoAtual =
                _context.Database
                    .GetDbConnection()
                    .Database;

            if (!string.Equals(
                    bancoAtual,
                    BancoEsperado,
                    StringComparison.OrdinalIgnoreCase))
            {
                return new ResultadoVerificacaoBanco(
                    false,
                    bancoAtual,
                    $"Banco inválido. Esperado: {BancoEsperado}.");
            }

            var investidores =
                await _context.Investidores.CountAsync();

            var ativos =
                await _context.Ativos.CountAsync();

            var operacoes =
                await _context.Operacoes.CountAsync();

            var proventos =
                await _context.Proventos.CountAsync();

            var opcoes =
                await _context.OperacoesOpcoes.CountAsync();

            var cotacoes =
                await _context.CotacoesAtivos.CountAsync();

            var saldosDisponiveis =
                await _context.SaldosDisponiveis.CountAsync();

            var historicosPatrimonio =
                await _context.HistoricosPatrimonio.CountAsync();

            var mensagem =
                $"Conexão OK. " +
                $"Investidores={investidores}, " +
                $"Ativos={ativos}, " +
                $"Operações={operacoes}, " +
                $"Proventos={proventos}, " +
                $"Opções={opcoes}, " +
                $"Cotações={cotacoes}, " +
                $"Saldos={saldosDisponiveis}, " +
                $"Históricos={historicosPatrimonio}.";

            return new ResultadoVerificacaoBanco(
                true,
                bancoAtual,
                mensagem);
        }
    }

    public record ResultadoVerificacaoBanco(
        bool Valido,
        string? Banco,
        string Mensagem);
}