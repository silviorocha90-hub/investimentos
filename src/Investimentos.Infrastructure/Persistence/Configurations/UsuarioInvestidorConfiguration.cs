using Investimentos.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class UsuarioInvestidorConfiguration
        : IEntityTypeConfiguration<UsuarioInvestidor>
    {
        public void Configure(
            EntityTypeBuilder<UsuarioInvestidor> builder)
        {
            builder.ToTable(
                "UsuarioInvestidor");

            builder.HasKey(
                x => new
                {
                    x.UsuarioId,
                    x.InvestidorId
                });

            builder.HasOne(x => x.Investidor)
                .WithMany()
                .HasForeignKey(x => x.InvestidorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}