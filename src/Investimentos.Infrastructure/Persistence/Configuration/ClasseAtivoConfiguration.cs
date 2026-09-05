using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class ClasseAtivoConfiguration
        : IEntityTypeConfiguration<ClasseAtivo>
    {
        public void Configure(
            EntityTypeBuilder<ClasseAtivo> builder)
        {
            builder.ToTable("ClasseAtivo");

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
                    Codigo = "RENDA_FIXA",
                    Nome = "Renda Fixa",
                    Ativo = true
                },
                new
                {
                    Id = 2,
                    Codigo = "RENDA_VARIAVEL",
                    Nome = "Renda Variável",
                    Ativo = true
                },
                new
                {
                    Id = 3,
                    Codigo = "FUNDOS",
                    Nome = "Fundos",
                    Ativo = true
                },
                new
                {
                    Id = 4,
                    Codigo = "ALTERNATIVOS",
                    Nome = "Alternativos",
                    Ativo = true
                });
        }
    }
}