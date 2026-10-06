using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations;

public class RelatorioDiarioIaConfiguration
    : IEntityTypeConfiguration<RelatorioDiarioIa>
{
    public void Configure(
        EntityTypeBuilder<RelatorioDiarioIa> builder)
    {
        builder.ToTable("RelatorioDiarioIa");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DataReferencia)
            .HasColumnType("date");

        builder.Property(x => x.DataGeracao)
            .HasColumnType("datetime2");

        builder.Property(x => x.Conteudo)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.Modelo)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Escopo)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.InvestidorId);

        builder.HasIndex(x => new
            {
                x.DataReferencia,
                x.Escopo,
                x.InvestidorId
            })
            .IsUnique();
    }
}
