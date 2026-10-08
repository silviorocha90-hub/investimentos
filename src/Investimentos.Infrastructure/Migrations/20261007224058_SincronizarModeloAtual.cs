using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SincronizarModeloAtual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF COL_LENGTH('Usuario', 'WhatsApp') IS NOT NULL ALTER TABLE [Usuario] DROP COLUMN [WhatsApp];");

            migrationBuilder.Sql(
                "IF COL_LENGTH('Investidor', 'FrequenciaRelatorioIa') IS NOT NULL ALTER TABLE [Investidor] DROP COLUMN [FrequenciaRelatorioIa];");

            migrationBuilder.Sql(
                "IF COL_LENGTH('Investidor', 'ReceberRelatorioIa') IS NOT NULL ALTER TABLE [Investidor] DROP COLUMN [ReceberRelatorioIa];");

            migrationBuilder.Sql(
                "IF COL_LENGTH('Investidor', 'WhatsApp') IS NOT NULL ALTER TABLE [Investidor] DROP COLUMN [WhatsApp];");

            migrationBuilder.AddColumn<string>(
                name: "TelegramChatId",
                table: "Usuario",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "Usuario");

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "Usuario",
                type: "nvarchar(15)",
                maxLength: 15,
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
        }
    }
}
