using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class MovimentacaoFinanceiraConfiguration :
        IEntityTypeConfiguration<MovimentacaoFinanceira>
    {
        public void Configure(EntityTypeBuilder<MovimentacaoFinanceira> builder)
        {
            builder.ToTable("MovimentacaoFinanceira");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Data).HasColumnType("date").IsRequired();
            builder.Property(x => x.Tipo).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Valor).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Descricao).HasMaxLength(200);
            builder.HasOne(x => x.Investidor).WithMany()
                .HasForeignKey(x => x.InvestidorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.InvestidorId, x.Data });
        }
    }
}