using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class SaldoDisponivelConfiguration :
        IEntityTypeConfiguration<SaldoDisponivel>
    {
        public void Configure(
            EntityTypeBuilder<SaldoDisponivel> builder)
        {
            builder.ToTable("SaldoDisponivel");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DataReferencia)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.Valor)
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