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

        public DbSet<ClasseAtivo> ClassesAtivos =>
            Set<ClasseAtivo>();

        public DbSet<TipoAtivo> TiposAtivos =>
            Set<TipoAtivo>();

        public DbSet<Ativo> Ativos =>
            Set<Ativo>();

        public DbSet<Investidor> Investidores =>
            Set<Investidor>();

        public DbSet<TipoOperacao> TiposOperacoes =>
            Set<TipoOperacao>();

        public DbSet<Operacao> Operacoes =>
            Set<Operacao>();

        public DbSet<Provento> Proventos =>
            Set<Provento>();

        public DbSet<OperacaoOpcao> OperacoesOpcoes =>
            Set<OperacaoOpcao>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(InvestimentosDbContext).Assembly);
        }
    }
}