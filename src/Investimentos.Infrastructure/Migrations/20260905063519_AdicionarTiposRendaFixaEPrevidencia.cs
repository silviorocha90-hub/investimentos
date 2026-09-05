using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTiposRendaFixaEPrevidencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM TipoAtivo
                    WHERE Codigo = 'RENDA_FIXA'
                )
                BEGIN
                    INSERT INTO TipoAtivo
                    (
                        Codigo,
                        Nome,
                        Ativo,
                        ClasseAtivoId
                    )
                    SELECT
                        'RENDA_FIXA',
                        'Renda Fixa',
                        1,
                        Id
                    FROM ClasseAtivo
                    WHERE Codigo = 'RENDA_FIXA';
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM TipoAtivo
                    WHERE Codigo = 'PREVIDENCIA'
                )
                BEGIN
                    INSERT INTO TipoAtivo
                    (
                        Codigo,
                        Nome,
                        Ativo,
                        ClasseAtivoId
                    )
                    SELECT
                        'PREVIDENCIA',
                        'Previdência',
                        1,
                        Id
                    FROM ClasseAtivo
                    WHERE Codigo = 'FUNDOS';
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM TipoAtivo
                WHERE Codigo IN (
                    'RENDA_FIXA',
                    'PREVIDENCIA'
                );
                """);
        }
    }
}