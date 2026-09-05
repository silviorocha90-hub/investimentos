using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    public partial class AdicionarDescontosFiscais : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DescontoFiscal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvestidorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "date", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescontoFiscal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DescontoFiscal_Investidor_InvestidorId",
                        column: x => x.InvestidorId,
                        principalTable: "Investidor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DescontoFiscal_InvestidorId",
                table: "DescontoFiscal",
                column: "InvestidorId");
            migrationBuilder.CreateIndex(
                name: "IX_DescontoFiscal_InvestidorId_DataPagamento_Tipo",
                table: "DescontoFiscal",
                columns: new[] { "InvestidorId", "DataPagamento", "Tipo" });
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable(name: "DescontoFiscal");
    }
}
