using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSequenciaOperacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Operacao_InvestidorId",
                table: "Operacao");

            migrationBuilder.AddColumn<int>(
                name: "Sequencia",
                table: "Operacao",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                WITH OperacoesOrdenadas AS
                (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY InvestidorId, Data
                            ORDER BY
                                CASE
                                    WHEN PrecoUnitario = 40.00 THEN 1
                                    WHEN PrecoUnitario = 42.00 THEN 2
                                    WHEN PrecoUnitario = 45.00 THEN 3
                                    ELSE 4
                                END,
                                Id
                        ) AS NovaSequencia
                    FROM Operacao
                )
                UPDATE Operacao
                SET Sequencia = OperacoesOrdenadas.NovaSequencia
                FROM Operacao
                INNER JOIN OperacoesOrdenadas
                    ON Operacao.Id = OperacoesOrdenadas.Id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Operacao_InvestidorId_Data_Sequencia",
                table: "Operacao",
                columns: new[]
                {
                    "InvestidorId",
                    "Data",
                    "Sequencia"
                },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Operacao_InvestidorId_Data_Sequencia",
                table: "Operacao");

            migrationBuilder.DropColumn(
                name: "Sequencia",
                table: "Operacao");

            migrationBuilder.CreateIndex(
                name: "IX_Operacao_InvestidorId",
                table: "Operacao",
                column: "InvestidorId");
        }
    }
}