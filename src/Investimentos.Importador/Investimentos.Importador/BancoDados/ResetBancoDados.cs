using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Importador.BancoDados
{
    public class ResetBancoDados
    {
        private const string BancoPermitido = "InvestimentosDb";

        private readonly InvestimentosDbContext _context;

        public ResetBancoDados(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task ExecutarAsync()
        {
            ValidarBancoPermitido();

            // Tabelas dependentes de Ativo e/ou Investidor.
            await _context.OperacoesOpcoes
                .ExecuteDeleteAsync();

            await _context.Proventos
                .ExecuteDeleteAsync();

            await _context.Operacoes
                .ExecuteDeleteAsync();

            await _context.CotacoesAtivos
                .ExecuteDeleteAsync();

            await _context.SaldosDisponiveis
                .ExecuteDeleteAsync();

            await _context.HistoricosPatrimonio
                .ExecuteDeleteAsync();

            // Entidades principais reconstruídas a partir do Excel.
            await _context.Ativos
                .ExecuteDeleteAsync();

            await _context.Investidores
                .ExecuteDeleteAsync();

            _context.ChangeTracker.Clear();
        }

        private void ValidarBancoPermitido()
        {
            var bancoAtual =
                _context.Database
                    .GetDbConnection()
                    .Database;

            if (!string.Equals(
                    bancoAtual,
                    BancoPermitido,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Reset bloqueado. O banco conectado é '{bancoAtual}', " +
                    $"mas somente '{BancoPermitido}' pode ser reconstruído " +
                    $"pelo importador.");
            }
        }
    }
}