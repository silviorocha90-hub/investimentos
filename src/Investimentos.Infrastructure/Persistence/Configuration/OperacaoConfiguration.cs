using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class OperacaoConfiguration
        : IEntityTypeConfiguration<Operacao>
    {
        public void Configure(
            EntityTypeBuilder<Operacao> builder)
        {
            builder.ToTable("Operacao");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Data)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.Sequencia)
                .IsRequired();

            builder.Property(x => x.Quantidade)
                .HasPrecision(18, 8)
                .IsRequired();

            builder.Property(x => x.PrecoUnitario)
                .HasPrecision(18, 8)
                .IsRequired();

            builder.Property(x => x.Taxas)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Ignore(x => x.ValorBruto);

            builder.HasOne(x => x.Investidor)
                .WithMany()
                .HasForeignKey(x => x.InvestidorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Ativo)
                .WithMany()
                .HasForeignKey(x => x.AtivoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TipoOperacao)
                .WithMany()
                .HasForeignKey(x => x.TipoOperacaoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.InvestidorId,
                x.Data,
                x.Sequencia
            })
            .IsUnique();
        }
    }
}