using Investimentos.Domain.Entities;
using Investimentos.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence
{
    public class InvestimentosDbContext
        : DbContext
    {
        public InvestimentosDbContext(
            DbContextOptions<InvestimentosDbContext>
                options)
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

        public DbSet<CotacaoAtivo> CotacoesAtivos =>
            Set<CotacaoAtivo>();

        public DbSet<ValorPatrimonialAtivo> ValoresPatrimoniaisAtivos =>
            Set<ValorPatrimonialAtivo>();

        public DbSet<MetaAtivo> MetasAtivos =>
            Set<MetaAtivo>();

        public DbSet<SaldoDisponivel> SaldosDisponiveis =>
            Set<SaldoDisponivel>();

        public DbSet<MovimentacaoFinanceira> MovimentacoesFinanceiras =>
            Set<MovimentacaoFinanceira>();

        public DbSet<HistoricoPatrimonio>
            HistoricosPatrimonio =>
                Set<HistoricoPatrimonio>();

        public DbSet<DescontoFiscal> DescontosFiscais =>
            Set<DescontoFiscal>();

        public DbSet<Usuario> Usuarios =>
            Set<Usuario>();

        public DbSet<UsuarioPermissao>
            UsuariosPermissoes =>
                Set<UsuarioPermissao>();

        public DbSet<UsuarioInvestidor>
            UsuariosInvestidores =>
                Set<UsuarioInvestidor>();

        public DbSet<UsuarioInvestidorPermissao>
            UsuariosInvestidoresPermissoes =>
                Set<UsuarioInvestidorPermissao>();

        public DbSet<TokenRecuperacaoSenha>
            TokensRecuperacaoSenha =>
                Set<TokenRecuperacaoSenha>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .ApplyConfigurationsFromAssembly(
                    typeof(InvestimentosDbContext)
                        .Assembly);
        }
    }
}