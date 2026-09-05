using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Investimentos.Infrastructure.Persistence
{
    public class InvestimentosDbContextFactory
        : IDesignTimeDbContextFactory<InvestimentosDbContext>
    {
        public InvestimentosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<InvestimentosDbContext>();

            var connectionString =
                @"Server=localhost;Database=InvestimentosDb;Trusted_Connection=True;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString);

            return new InvestimentosDbContext(
                optionsBuilder.Options);
        }
    }
}