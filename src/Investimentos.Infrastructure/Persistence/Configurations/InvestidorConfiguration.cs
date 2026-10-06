using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class InvestidorConfiguration
        : IEntityTypeConfiguration<Investidor>
    {
        public void Configure(
            EntityTypeBuilder<Investidor> builder)
        {
            builder.ToTable("Investidor");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Nome)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.WhatsApp)
                .HasMaxLength(15);

            builder.Property(x => x.ReceberRelatorioIa)
                .IsRequired();

            builder.Property(x => x.FrequenciaRelatorioIa)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.Nome)
                .IsUnique();
        }
    }
}