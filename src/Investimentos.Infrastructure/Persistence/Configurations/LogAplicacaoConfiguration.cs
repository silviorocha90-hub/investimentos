using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations;

public sealed class LogAplicacaoConfiguration : IEntityTypeConfiguration<LogAplicacao>
{
    public void Configure(EntityTypeBuilder<LogAplicacao> builder)
    {
        builder.ToTable("LogsAplicacao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DataUtc).IsRequired();
        builder.Property(x => x.Ambiente).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Modulo).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Operacao).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Nivel).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Mensagem).HasMaxLength(1500).IsRequired();
        builder.Property(x => x.Detalhes).HasColumnType("nvarchar(max)");
        builder.Property(x => x.CorrelacaoId).HasMaxLength(100);
        builder.HasIndex(x => x.DataUtc);
        builder.HasIndex(x => x.CorrelacaoId);
        builder.HasIndex(x => new { x.Modulo, x.Nivel, x.DataUtc });
    }
}
