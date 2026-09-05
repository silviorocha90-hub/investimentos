using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class AtivoConfiguration
        : IEntityTypeConfiguration<Ativo>
    {
        public void Configure(
            EntityTypeBuilder<Ativo> builder)
        {
            builder.ToTable("Ativo");

            builder.HasKey(x => x.Id);

            var tickerConverter =
                new ValueConverter<Ticker, string>(
                    ticker => ticker.Codigo,
                    codigo => new Ticker(codigo));

            builder.Property(x => x.Ticker)
                .HasConversion(tickerConverter)
                .HasColumnName("Ticker")
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.Ticker)
                .IsUnique();

            builder.Property(x => x.Nome)
                .HasMaxLength(150)
                .IsRequired();

            builder.HasOne(x => x.TipoAtivo)
                .WithMany()
                .HasForeignKey(x => x.TipoAtivoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}