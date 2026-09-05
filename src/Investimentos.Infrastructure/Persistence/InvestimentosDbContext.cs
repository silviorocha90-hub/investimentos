using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence
{
    public class InvestimentosDbContext : DbContext
    {
        public InvestimentosDbContext(
            DbContextOptions<InvestimentosDbContext> options)
            : base(options)
        {
        }

        public DbSet<ClasseAtivo> ClassesAtivos { get; set; }
        public DbSet<TipoAtivo> TiposAtivos { get; set; }
        public DbSet<Ativo> Ativos { get; set; }
        public DbSet<Investidor> Investidores { get; set; }
        public DbSet<TipoOperacao> TiposOperacoes { get; set; }
        public DbSet<Operacao> Operacoes { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(InvestimentosDbContext).Assembly);
        }
    }
}