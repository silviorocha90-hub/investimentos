using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPreferenciasRelatorioIaAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FrequenciaRelatorioIa",
                table: "Usuario",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "DIARIO");

            migrationBuilder.AddColumn<bool>(
                name: "ReceberRelatorioIa",
                table: "Usuario",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoEnvioRelatorioIa",
                table: "Usuario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "Usuario",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrequenciaRelatorioIa",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "ReceberRelatorioIa",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "UltimoEnvioRelatorioIa",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "Usuario");
        }
    }
}
