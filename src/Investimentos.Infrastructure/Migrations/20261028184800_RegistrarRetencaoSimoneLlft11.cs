using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Investimentos.Infrastructure.Migrations;

[DbContext(typeof(InvestimentosDbContext))]
[Migration("20261028184800_RegistrarRetencaoSimoneLlft11")]
public class RegistrarRetencaoSimoneLlft11 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Valor confirmado explicitamente pelo titular em 28/10/2026.
        // Só vincula quando existe UMA venda inequivocamente correspondente.
        migrationBuilder.Sql("""
            DECLARE @Candidatas TABLE (Id uniqueidentifier);
            INSERT INTO @Candidatas
            SELECT o.Id FROM Operacao o
            JOIN Investidor i ON i.Id = o.InvestidorId
            JOIN Ativo a ON a.Id = o.AtivoId
            JOIN TipoOperacao t ON t.Id = o.TipoOperacaoId
            WHERE UPPER(LTRIM(RTRIM(i.Nome))) = 'SIMONE'
              AND a.Ticker = 'LLFT11' AND t.Codigo = 'VENDA' AND o.Quantidade = 499
              AND o.Data >= '20261001' AND o.Data < '20261029';
            IF (SELECT COUNT(*) FROM @Candidatas) = 1
            BEGIN
                INSERT INTO RetencaoDefinitivaIr (Id, OperacaoId, Valor, Observacao, CriadoEmUtc, CriadoPor)
                SELECT NEWID(), c.Id, 62.28,
                    N'IR definitivo de R$ 62,28 confirmado pelo titular em 28/10/2026 para venda de 499 cotas de LLFT11 da Simone. Conferir com nota de corretagem.',
                    SYSUTCDATETIME(), 'confirmacao-titular-20261028'
                FROM @Candidatas c
                WHERE NOT EXISTS (SELECT 1 FROM RetencaoDefinitivaIr r WHERE r.OperacaoId = c.Id);
            END;
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM RetencaoDefinitivaIr WHERE CriadoPor = 'confirmacao-titular-20261028';");
    }
}
