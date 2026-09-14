using Investimentos.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class TokenRecuperacaoSenhaConfiguration
        : IEntityTypeConfiguration<TokenRecuperacaoSenha>
    {
        public void Configure(
            EntityTypeBuilder<TokenRecuperacaoSenha> builder)
        {
            builder.ToTable(
                "TokenRecuperacaoSenha");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TokenHash)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.DataCriacao)
                .IsRequired();

            builder.Property(x => x.DataExpiracao)
                .IsRequired();

            builder.Property(x => x.DataUtilizacao);

            builder.HasIndex(x => x.TokenHash)
                .IsUnique();

            builder.HasIndex(
                x => new
                {
                    x.UsuarioId,
                    x.DataExpiracao
                });

            builder.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}