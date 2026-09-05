using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class TipoOperacaoConfiguration
        : IEntityTypeConfiguration<TipoOperacao>
    {
        public void Configure(
            EntityTypeBuilder<TipoOperacao> builder)
        {
            builder.ToTable("TipoOperacao");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Codigo)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Nome)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Ativo)
                .IsRequired();

            builder.HasIndex(x => x.Codigo)
                .IsUnique();

            builder.HasData(
                new
                {
                    Id = 1,
                    Codigo = "COMPRA",
                    Nome = "Compra",
                    Ativo = true
                },
                new
                {
                    Id = 2,
                    Codigo = "VENDA",
                    Nome = "Venda",
                    Ativo = true
                });
        }
    }
}