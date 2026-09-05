using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class TipoAtivoConfiguration
        : IEntityTypeConfiguration<TipoAtivo>
    {
        public void Configure(
            EntityTypeBuilder<TipoAtivo> builder)
        {
            builder.ToTable("TipoAtivo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Codigo)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Nome)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Ativo)
                .IsRequired();

            builder.HasIndex(x => x.Codigo)
                .IsUnique();

            builder.HasOne(x => x.ClasseAtivo)
                .WithMany()
                .HasForeignKey(x => x.ClasseAtivoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                new { Id = 1, Codigo = "ACAO", Nome = "Ação", Ativo = true, ClasseAtivoId = 2 },
                new { Id = 2, Codigo = "FII", Nome = "Fundo Imobiliário", Ativo = true, ClasseAtivoId = 3 },
                new { Id = 3, Codigo = "ETF", Nome = "ETF", Ativo = true, ClasseAtivoId = 3 },
                new { Id = 4, Codigo = "BDR", Nome = "BDR", Ativo = true, ClasseAtivoId = 2 },
                new { Id = 5, Codigo = "TESOURO_DIRETO", Nome = "Tesouro Direto", Ativo = true, ClasseAtivoId = 1 },
                new { Id = 6, Codigo = "CDB", Nome = "CDB", Ativo = true, ClasseAtivoId = 1 },
                new { Id = 7, Codigo = "LCI", Nome = "LCI", Ativo = true, ClasseAtivoId = 1 },
                new { Id = 8, Codigo = "LCA", Nome = "LCA", Ativo = true, ClasseAtivoId = 1 },
                new { Id = 9, Codigo = "DEBENTURE", Nome = "Debênture", Ativo = true, ClasseAtivoId = 1 },
                new { Id = 10, Codigo = "POUPANCA", Nome = "Poupança", Ativo = true, ClasseAtivoId = 1 },
                new { Id = 11, Codigo = "FUNDO_RENDA_FIXA", Nome = "Fundo de Renda Fixa", Ativo = true, ClasseAtivoId = 3 },
                new { Id = 12, Codigo = "FUNDO_ACOES", Nome = "Fundo de Ações", Ativo = true, ClasseAtivoId = 3 },
                new { Id = 13, Codigo = "FUNDO_MULTIMERCADO", Nome = "Fundo Multimercado", Ativo = true, ClasseAtivoId = 3 },
                new { Id = 14, Codigo = "FUNDO_CAMBIAL", Nome = "Fundo Cambial", Ativo = true, ClasseAtivoId = 3 },
                new { Id = 15, Codigo = "CRIPTOMOEDA", Nome = "Criptomoeda", Ativo = true, ClasseAtivoId = 4 },
                new { Id = 16, Codigo = "CAMBIO", Nome = "Câmbio", Ativo = true, ClasseAtivoId = 4 },
                new { Id = 17, Codigo = "COMMODITY", Nome = "Commodity", Ativo = true, ClasseAtivoId = 4 },
                new { Id = 18, Codigo = "OURO", Nome = "Ouro", Ativo = true, ClasseAtivoId = 4 });
        }
    }
}