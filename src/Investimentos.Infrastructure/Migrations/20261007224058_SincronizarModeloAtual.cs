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
            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "FrequenciaRelatorioIa",
                table: "Investidor");

            migrationBuilder.DropColumn(
                name: "ReceberRelatorioIa",
                table: "Investidor");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "Investidor");

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
