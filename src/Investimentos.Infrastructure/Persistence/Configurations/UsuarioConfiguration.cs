using Investimentos.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class UsuarioConfiguration
        : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(
            EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuario");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Nome)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(254)
                .IsRequired();

            builder.Property(x => x.SenhaHash)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Perfil)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.DataCadastro)
                .IsRequired();

            builder.Property(x => x.DataAprovacao);

            builder.Property(x =>
                    x.AprovadoPorUsuarioId);

            builder.Property(x => x.UltimoLogin);

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.HasMany(x => x.Permissoes)
                .WithOne(x => x.Usuario)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Investidores)
                .WithOne(x => x.Usuario)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(
                    x => x.AprovadoPorUsuarioId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}