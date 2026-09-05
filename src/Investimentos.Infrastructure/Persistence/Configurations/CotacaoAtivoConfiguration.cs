using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class CotacaoAtivoConfiguration :
        IEntityTypeConfiguration<CotacaoAtivo>
    {
        public void Configure(
            EntityTypeBuilder<CotacaoAtivo> builder)
        {
            builder.ToTable("CotacaoAtivo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DataReferencia)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.Preco)
                .HasPrecision(18, 8)
                .IsRequired();

            builder.HasOne(x => x.Ativo)
                .WithMany()
                .HasForeignKey(x => x.AtivoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(
                x => new
                {
                    x.AtivoId,
                    x.DataReferencia
                })
                .IsUnique();
        }
    }
}