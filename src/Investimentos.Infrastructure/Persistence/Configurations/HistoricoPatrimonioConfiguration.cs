using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class HistoricoPatrimonioConfiguration :
        IEntityTypeConfiguration<HistoricoPatrimonio>
    {
        public void Configure(
            EntityTypeBuilder<HistoricoPatrimonio> builder)
        {
            builder.ToTable("HistoricoPatrimonio");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DataReferencia)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.ValorCarteira)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.HasOne(x => x.Investidor)
                .WithMany()
                .HasForeignKey(x => x.InvestidorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(
                x => new
                {
                    x.InvestidorId,
                    x.DataReferencia
                })
                .IsUnique();
        }
    }
}