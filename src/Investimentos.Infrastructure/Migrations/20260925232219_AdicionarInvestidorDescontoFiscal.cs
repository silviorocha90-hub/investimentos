using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarInvestidorDescontoFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InvestidorId",
                table: "DescontoFiscal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DescontoFiscal_InvestidorId",
                table: "DescontoFiscal",
                column: "InvestidorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DescontoFiscal_Investidor_InvestidorId",
                table: "DescontoFiscal",
                column: "InvestidorId",
                principalTable: "Investidor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DescontoFiscal_Investidor_InvestidorId",
                table: "DescontoFiscal");

            migrationBuilder.DropIndex(
                name: "IX_DescontoFiscal_InvestidorId",
                table: "DescontoFiscal");

            migrationBuilder.DropColumn(
                name: "InvestidorId",
                table: "DescontoFiscal");
        }
    }
}
