using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPreferenciasRelatorioIaInvestidor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RelatorioDiarioIa_DataReferencia",
                table: "RelatorioDiarioIa");

            migrationBuilder.AddColumn<string>(
                name: "Escopo",
                table: "RelatorioDiarioIa",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "TODOS");

            migrationBuilder.AddColumn<Guid>(
                name: "InvestidorId",
                table: "RelatorioDiarioIa",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrequenciaRelatorioIa",
                table: "Investidor",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "DIARIO");

            migrationBuilder.AddColumn<bool>(
                name: "ReceberRelatorioIa",
                table: "Investidor",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "Investidor",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelatorioDiarioIa_DataReferencia_Escopo_InvestidorId",
                table: "RelatorioDiarioIa",
                columns: new[] { "DataReferencia", "Escopo", "InvestidorId" },
                unique: true,
                filter: "[InvestidorId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RelatorioDiarioIa_DataReferencia_Escopo_InvestidorId",
                table: "RelatorioDiarioIa");

            migrationBuilder.DropColumn(
                name: "Escopo",
                table: "RelatorioDiarioIa");

            migrationBuilder.DropColumn(
                name: "InvestidorId",
                table: "RelatorioDiarioIa");

            migrationBuilder.DropColumn(
                name: "FrequenciaRelatorioIa",
                table: "Investidor");

            migrationBuilder.DropColumn(
                name: "ReceberRelatorioIa",
                table: "Investidor");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "Investidor");

            migrationBuilder.CreateIndex(
                name: "IX_RelatorioDiarioIa_DataReferencia",
                table: "RelatorioDiarioIa",
                column: "DataReferencia",
                unique: true);
        }
    }
}
