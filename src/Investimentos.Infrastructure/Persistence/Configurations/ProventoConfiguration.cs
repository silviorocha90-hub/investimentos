using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class ProventoConfiguration :
        IEntityTypeConfiguration<Provento>
    {
        public void Configure(
            EntityTypeBuilder<Provento> builder)
        {
            builder.ToTable("Provento");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Tipo)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Descricao)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(x => x.DataCom)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(x => x.DataPagamento)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.QuantidadeBase)
                .HasPrecision(18, 8)
                .IsRequired();

            builder.Property(x => x.ValorPorUnidade)
                .HasPrecision(18, 8)
                .IsRequired();

            builder.Property(x => x.ValorRecebido)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Ignore(x => x.ValorBruto);
            builder.Ignore(x => x.ImpostoRetido);

            builder.HasOne(x => x.Investidor)
                .WithMany()
                .HasForeignKey(x => x.InvestidorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Ativo)
                .WithMany()
                .HasForeignKey(x => x.AtivoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(
                x => new
                {
                    x.InvestidorId,
                    x.DataPagamento
                });
        }
    }
}