using Investimentos.Domain.Entities;
using Investimentos.Infrastructure.Persistence;
using Investimentos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.IntegrationTests.Persistence.Repositories;

public class InvestidorRepositoryTests
{
    private const string ConnectionString =
        "Server=localhost;" +
        "Database=InvestimentosDbTests;" +
        "Trusted_Connection=True;" +
        "TrustServerCertificate=True;";

    [Fact]
    public async Task DeveAdicionarInvestidorNoBanco()
    {
        await using var context = CriarContexto();

        await PrepararBancoAsync(context);

        var repository =
            new InvestidorRepository(context);

        var investidor =
            new Investidor("Silvio Teste");

        await repository.AdicionarAsync(investidor);

        context.ChangeTracker.Clear();

        var investidorPersistido =
            await context.Investidores
                .SingleAsync(
                    x => x.Id == investidor.Id);

        Assert.Equal(
            investidor.Id,
            investidorPersistido.Id);

        Assert.Equal(
            "Silvio Teste",
            investidorPersistido.Nome);
    }

    [Fact]
    public async Task DeveEncontrarInvestidorPorNome()
    {
        await using var context = CriarContexto();

        await PrepararBancoAsync(context);

        var repository =
            new InvestidorRepository(context);

        var investidor =
            new Investidor("Silvio Teste");

        await repository.AdicionarAsync(investidor);

        var existe =
            await repository.ExistePorNomeAsync(
                "Silvio Teste");

        Assert.True(existe);
    }

    [Fact]
    public async Task NaoDeveEncontrarInvestidorInexistente()
    {
        await using var context = CriarContexto();

        await PrepararBancoAsync(context);

        var repository =
            new InvestidorRepository(context);

        var existe =
            await repository.ExistePorNomeAsync(
                "Investidor Inexistente");

        Assert.False(existe);
    }

    private static InvestimentosDbContext CriarContexto()
    {
        var options =
            new DbContextOptionsBuilder<InvestimentosDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

        return new InvestimentosDbContext(options);
    }

    private static async Task PrepararBancoAsync(
        InvestimentosDbContext context)
    {
        await context.Database.EnsureDeletedAsync();

        await context.Database.MigrateAsync();
    }
}