using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class ValorPatrimonialAtivoConfiguration
        : IEntityTypeConfiguration<ValorPatrimonialAtivo>
    {
        public void Configure(
            EntityTypeBuilder<ValorPatrimonialAtivo> builder)
        {
            builder.ToTable("ValorPatrimonialAtivo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DataReferencia)
                .IsRequired();

            builder.Property(x => x.Valor)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.AtivoId,
                x.DataReferencia
            }).IsUnique();

            builder.HasOne(x => x.Ativo)
                .WithMany()
                .HasForeignKey(x => x.AtivoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
