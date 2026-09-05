using Investimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investimentos.Infrastructure.Persistence.Configuration
{
    public class OperacaoOpcaoConfiguration :
        IEntityTypeConfiguration<OperacaoOpcao>
    {
        public void Configure(
            EntityTypeBuilder<OperacaoOpcao> builder)
        {
            builder.ToTable("OperacaoOpcao");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TickerOpcao)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.TipoOpcao)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Natureza)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Situacao)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.DataOperacao)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.Vencimento)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.DataFinalizacao)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(x => x.Strike)
                .HasPrecision(18, 8)
                .IsRequired();

            builder.Property(x => x.Contratos)
                .IsRequired();

            builder.Property(x => x.Quantidade)
                .HasPrecision(18, 8)
                .IsRequired();

            builder.Property(x => x.PremioUnitario)
                .HasPrecision(18, 8)
                .IsRequired();

            builder.Property(x => x.Taxas)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.PrecoRecompraUnitario)
                .HasPrecision(18, 8)
                .IsRequired(false);

            builder.Property(x => x.ValorExecucao)
                .HasPrecision(18, 2)
                .IsRequired(false);

            builder.Ignore(x => x.PremioTotal);
            builder.Ignore(x => x.ValorRecompraTotal);
            builder.Ignore(x => x.ResultadoFinal);

            builder.HasOne(x => x.Investidor)
                .WithMany()
                .HasForeignKey(x => x.InvestidorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Ativo)
                .WithMany()
                .HasForeignKey(x => x.AtivoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(
                x => new
                {
                    x.InvestidorId,
                    x.DataOperacao
                });

            builder.HasIndex(
                x => x.TickerOpcao);
        }
    }
}