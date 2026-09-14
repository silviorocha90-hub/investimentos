using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DescontoFiscalPorCarteira : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A tabela DescontoFiscal já existe no banco.
            //
            // Ela foi criada anteriormente com vínculo ao Investidor,
            // mas essa criação não ficou registrada corretamente no
            // ModelSnapshot do Entity Framework.
            //
            // Esta migration reconcilia o banco existente com o modelo
            // atual, no qual o desconto fiscal pertence à carteira
            // consolidada e não a um investidor específico.

            migrationBuilder.DropForeignKey(
                name: "FK_DescontoFiscal_Investidor_InvestidorId",
                table: "DescontoFiscal");

            migrationBuilder.DropIndex(
                name: "IX_DescontoFiscal_InvestidorId",
                table: "DescontoFiscal");

            migrationBuilder.DropIndex(
                name: "IX_DescontoFiscal_InvestidorId_DataPagamento_Tipo",
                table: "DescontoFiscal");

            migrationBuilder.DropColumn(
                name: "InvestidorId",
                table: "DescontoFiscal");

            migrationBuilder.CreateIndex(
                name: "IX_DescontoFiscal_DataPagamento_Tipo",
                table: "DescontoFiscal",
                columns: new[] { "DataPagamento", "Tipo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DescontoFiscal_DataPagamento_Tipo",
                table: "DescontoFiscal");

            migrationBuilder.AddColumn<Guid>(
                name: "InvestidorId",
                table: "DescontoFiscal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DescontoFiscal_InvestidorId",
                table: "DescontoFiscal",
                column: "InvestidorId");

            migrationBuilder.CreateIndex(
                name: "IX_DescontoFiscal_InvestidorId_DataPagamento_Tipo",
                table: "DescontoFiscal",
                columns: new[] { "InvestidorId", "DataPagamento", "Tipo" });

            migrationBuilder.AddForeignKey(
                name: "FK_DescontoFiscal_Investidor_InvestidorId",
                table: "DescontoFiscal",
                column: "InvestidorId",
                principalTable: "Investidor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}