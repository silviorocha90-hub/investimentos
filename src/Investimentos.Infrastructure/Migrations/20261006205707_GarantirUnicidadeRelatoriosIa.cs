using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GarantirUnicidadeRelatoriosIa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RelatorioDiarioIa_DataReferencia_Escopo",
                table: "RelatorioDiarioIa",
                columns: new[] { "DataReferencia", "Escopo" },
                unique: true,
                filter: "[InvestidorId] IS NULL AND [Escopo] = 'TODOS'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RelatorioDiarioIa_DataReferencia_Escopo",
                table: "RelatorioDiarioIa");
        }
    }
}
