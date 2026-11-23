using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Investimentos.Api.Tests;

public class RemoverEntradasSaidasMigrationTests
{
    [Fact]
    public void MigrationRemoveSomenteLivroDeFluxosEPreservaDemaisTabelas()
    {
        using var db = new InvestimentosDbContext(new DbContextOptionsBuilder<InvestimentosDbContext>()
            .UseSqlServer("Server=localhost;Database=InvestimentosDb;Trusted_Connection=True;TrustServerCertificate=True").Options);
        var migrations = db.GetService<IMigrationsAssembly>();
        var migration = migrations.CreateMigration(
            migrations.Migrations["20261123152640_RemoverEntradasSaidas"], db.Database.ProviderName!);
        var drop = Assert.IsType<DropTableOperation>(Assert.Single(migration.UpOperations));
        Assert.Equal("MovimentacaoFinanceira", drop.Name);
        Assert.DoesNotContain(db.Model.GetEntityTypes(), x => x.GetTableName() == "MovimentacaoFinanceira");
        foreach (var table in new[] { "Operacao", "OperacaoOpcao", "Provento", "SaldoDisponivel", "HistoricoPatrimonio", "HistoricoEstimativaIr" })
            Assert.Contains(db.Model.GetEntityTypes(), x => x.GetTableName() == table);
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
