using Investimentos.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class UsuarioPermissaoConfiguration
        : IEntityTypeConfiguration<UsuarioPermissao>
    {
        public void Configure(
            EntityTypeBuilder<UsuarioPermissao> builder)
        {
            builder.ToTable(
                "UsuarioPermissao");

            builder.HasKey(
                x => new
                {
                    x.UsuarioId,
                    x.Permissao
                });

            builder.Property(x => x.Permissao)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}