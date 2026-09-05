using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Importador.BancoDados
{
    public static class ConfiguracaoBancoDados
    {
        private const string ConnectionString =
            "Server=localhost;" +
            "Database=InvestimentosDb;" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;";

        public static InvestimentosDbContext CriarContexto()
        {
            var options =
                new DbContextOptionsBuilder<InvestimentosDbContext>()
                    .UseSqlServer(ConnectionString)
                    .Options;

            return new InvestimentosDbContext(options);
        }
    }
}