using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EstimativaIrPorInvestidor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoricoEstimativaIr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    CriadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoEstimativaIr", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetencaoDefinitivaIr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CriadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetencaoDefinitivaIr", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoEstimativaIr_Ano_CriadoEmUtc",
                table: "HistoricoEstimativaIr",
                columns: new[] { "Ano", "CriadoEmUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RetencaoDefinitivaIr_OperacaoId_CriadoEmUtc",
                table: "RetencaoDefinitivaIr",
                columns: new[] { "OperacaoId", "CriadoEmUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricoEstimativaIr");

            migrationBuilder.DropTable(
                name: "RetencaoDefinitivaIr");
        }
    }
}
