using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations;
public class HistoricoEstimativaIrConfiguration : IEntityTypeConfiguration<HistoricoEstimativaIr>
{
    public void Configure(EntityTypeBuilder<HistoricoEstimativaIr> b)
    {
        b.ToTable("HistoricoEstimativaIr");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.Ano, x.CriadoEmUtc });
        b.Property(x => x.Conteudo).IsRequired();
        b.Property(x => x.CriadoPor).HasMaxLength(100).IsRequired();
    }
}
public class RetencaoDefinitivaIrConfiguration : IEntityTypeConfiguration<RetencaoDefinitivaIr>
{
    public void Configure(EntityTypeBuilder<RetencaoDefinitivaIr> b)
    {
        b.ToTable("RetencaoDefinitivaIr");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.OperacaoId, x.CriadoEmUtc });
        b.Property(x => x.Valor).HasPrecision(18, 2);
        b.Property(x => x.Observacao).HasMaxLength(1000).IsRequired();
        b.Property(x => x.CriadoPor).HasMaxLength(100).IsRequired();
    }
}
