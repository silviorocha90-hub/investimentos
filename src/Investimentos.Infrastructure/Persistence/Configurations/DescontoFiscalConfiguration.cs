using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class DescontoFiscalConfiguration :
        IEntityTypeConfiguration<DescontoFiscal>
    {
        public void Configure(
            EntityTypeBuilder<DescontoFiscal> builder)
        {
            builder.ToTable(
                "DescontoFiscal");

            builder.HasKey(
                x => x.Id);

            builder.Property(
                    x => x.Tipo)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(
                    x => x.DataPagamento)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(
                    x => x.Valor)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(
                    x => x.Descricao)
                .HasMaxLength(250);

            builder.HasIndex(
                x => new
                {
                    x.DataPagamento,
                    x.Tipo
                });
        }
    }
}