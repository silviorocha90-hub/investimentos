using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirHistoricoOperacoesOpcoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OperacaoOpcao_InvestidorId_TickerOpcao_DataOperacao",
                table: "OperacaoOpcao");

            migrationBuilder.RenameColumn(
                name: "ValorRecompra",
                table: "OperacaoOpcao",
                newName: "PrecoRecompraUnitario");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoRecompraUnitario",
                table: "OperacaoOpcao",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorExecucao",
                table: "OperacaoOpcao",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperacaoOpcao_InvestidorId_DataOperacao",
                table: "OperacaoOpcao",
                columns: new[] { "InvestidorId", "DataOperacao" });

            migrationBuilder.CreateIndex(
                name: "IX_OperacaoOpcao_TickerOpcao",
                table: "OperacaoOpcao",
                column: "TickerOpcao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OperacaoOpcao_InvestidorId_DataOperacao",
                table: "OperacaoOpcao");

            migrationBuilder.DropIndex(
                name: "IX_OperacaoOpcao_TickerOpcao",
                table: "OperacaoOpcao");

            migrationBuilder.DropColumn(
                name: "ValorExecucao",
                table: "OperacaoOpcao");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoRecompraUnitario",
                table: "OperacaoOpcao",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "PrecoRecompraUnitario",
                table: "OperacaoOpcao",
                newName: "ValorRecompra");

            migrationBuilder.CreateIndex(
                name: "IX_OperacaoOpcao_InvestidorId_TickerOpcao_DataOperacao",
                table: "OperacaoOpcao",
                columns: new[]
                {
                    "InvestidorId",
                    "TickerOpcao",
                    "DataOperacao"
                });
        }
    }
}