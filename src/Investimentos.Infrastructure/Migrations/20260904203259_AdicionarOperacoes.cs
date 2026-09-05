using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarOperacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TipoOperacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoOperacao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Operacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvestidorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtivoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoOperacaoId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Taxas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operacao_Ativo_AtivoId",
                        column: x => x.AtivoId,
                        principalTable: "Ativo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operacao_Investidor_InvestidorId",
                        column: x => x.InvestidorId,
                        principalTable: "Investidor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operacao_TipoOperacao_TipoOperacaoId",
                        column: x => x.TipoOperacaoId,
                        principalTable: "TipoOperacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TipoOperacao",
                columns: new[] { "Id", "Ativo", "Codigo", "Nome" },
                values: new object[,]
                {
                    { 1, true, "COMPRA", "Compra" },
                    { 2, true, "VENDA", "Venda" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Operacao_AtivoId",
                table: "Operacao",
                column: "AtivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Operacao_InvestidorId",
                table: "Operacao",
                column: "InvestidorId");

            migrationBuilder.CreateIndex(
                name: "IX_Operacao_TipoOperacaoId",
                table: "Operacao",
                column: "TipoOperacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_TipoOperacao_Codigo",
                table: "TipoOperacao",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Operacao");

            migrationBuilder.DropTable(
                name: "TipoOperacao");
        }
    }
}
