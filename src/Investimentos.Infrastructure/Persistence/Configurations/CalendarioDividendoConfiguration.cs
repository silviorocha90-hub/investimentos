using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations;

public sealed class CalendarioDividendoConfiguration : IEntityTypeConfiguration<CalendarioDividendo>
{
    public void Configure(EntityTypeBuilder<CalendarioDividendo> builder)
    {
        builder.ToTable("CalendariosDividendos");
        builder.HasKey(x => new { x.InvestidorId, x.Ticker });
        builder.Property(x => x.Ticker).HasMaxLength(24).IsRequired();
        builder.Property(x => x.Meses).HasMaxLength(48).IsRequired();
        builder.Property(x => x.AtualizadoEmUtc).IsRequired();
        builder.HasOne<Investidor>().WithMany().HasForeignKey(x => x.InvestidorId).OnDelete(DeleteBehavior.Cascade);
    }
}
