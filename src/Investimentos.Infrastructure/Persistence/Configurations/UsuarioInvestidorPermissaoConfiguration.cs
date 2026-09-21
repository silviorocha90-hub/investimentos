using Investimentos.Domain.Entities;
using Investimentos.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configurations
{
    public class UsuarioInvestidorPermissaoConfiguration
        : IEntityTypeConfiguration<UsuarioInvestidorPermissao>
    {
        public void Configure(EntityTypeBuilder<UsuarioInvestidorPermissao> builder)
        {
            builder.ToTable("UsuarioInvestidorPermissao");
            builder.HasKey(x => new { x.UsuarioId, x.InvestidorId, x.Permissao });
            builder.Property(x => x.Permissao).HasConversion<string>().HasMaxLength(50).IsRequired();
            builder.HasOne(x => x.Usuario).WithMany(x => x.InvestidoresPermissoes)
                .HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Investidor>().WithMany().HasForeignKey(x => x.InvestidorId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.InvestidorId);
        }
    }
}