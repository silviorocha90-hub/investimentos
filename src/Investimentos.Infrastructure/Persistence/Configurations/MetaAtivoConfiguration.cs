using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class MetaAtivoConfiguration : IEntityTypeConfiguration<MetaAtivo>
    {
        public void Configure(EntityTypeBuilder<MetaAtivo> builder)
        {
            builder.ToTable("MetaAtivo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.QuantidadeDesejada)
                .HasPrecision(18, 4)
                .IsRequired();

            builder.HasIndex(x => new { x.InvestidorId, x.AtivoId })
                .IsUnique();

            builder.HasOne(x => x.Investidor)
                .WithMany()
                .HasForeignKey(x => x.InvestidorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Ativo)
                .WithMany()
                .HasForeignKey(x => x.AtivoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
