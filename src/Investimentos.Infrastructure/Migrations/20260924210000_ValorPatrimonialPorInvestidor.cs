using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    [DbContext(typeof(InvestimentosDbContext))]
    [Migration("20260924210000_ValorPatrimonialPorInvestidor")]
    public class ValorPatrimonialPorInvestidor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InvestidorId", table: "ValorPatrimonialAtivo",
                type: "uniqueidentifier", nullable: true);

            migrationBuilder.Sql(@"
UPDATE V
SET InvestidorId = COALESCE(
    (SELECT TOP 1 O.InvestidorId FROM Operacao O
     WHERE O.AtivoId = V.AtivoId
     GROUP BY O.InvestidorId ORDER BY COUNT(*) DESC),
    (SELECT TOP 1 I.Id FROM Investidor I ORDER BY I.Nome))
FROM ValorPatrimonialAtivo V;");

            migrationBuilder.AlterColumn<Guid>(
                name: "InvestidorId", table: "ValorPatrimonialAtivo",
                type: "uniqueidentifier", nullable: false,
                oldClrType: typeof(Guid), oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_ValorPatrimonialAtivo_AtivoId_DataReferencia",
                table: "ValorPatrimonialAtivo");

            migrationBuilder.CreateIndex(
                name: "IX_ValorPatrimonialAtivo_AtivoId_InvestidorId_DataReferencia",
                table: "ValorPatrimonialAtivo",
                columns: new[] { "AtivoId", "InvestidorId", "DataReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValorPatrimonialAtivo_InvestidorId",
                table: "ValorPatrimonialAtivo",
                column: "InvestidorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ValorPatrimonialAtivo_Investidor_InvestidorId",
                table: "ValorPatrimonialAtivo", column: "InvestidorId",
                principalTable: "Investidor", principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ValorPatrimonialAtivo_Investidor_InvestidorId",
                table: "ValorPatrimonialAtivo");
            migrationBuilder.DropIndex(
                name: "IX_ValorPatrimonialAtivo_AtivoId_InvestidorId_DataReferencia",
                table: "ValorPatrimonialAtivo");
            migrationBuilder.DropIndex(
                name: "IX_ValorPatrimonialAtivo_InvestidorId",
                table: "ValorPatrimonialAtivo");
            migrationBuilder.DropColumn(
                name: "InvestidorId", table: "ValorPatrimonialAtivo");
            migrationBuilder.CreateIndex(
                name: "IX_ValorPatrimonialAtivo_AtivoId_DataReferencia",
                table: "ValorPatrimonialAtivo",
                columns: new[] { "AtivoId", "DataReferencia" },
                unique: true);
        }
    }
}
