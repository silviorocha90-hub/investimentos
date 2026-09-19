using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    public partial class AdicionarValorPatrimonialAtivo : Migration
    {
        protected override void Up(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ValorPatrimonialAtivo",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),
                    AtivoId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),
                    DataReferencia = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),
                    Valor = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ValorPatrimonialAtivo",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_ValorPatrimonialAtivo_Ativo_AtivoId",
                        column: x => x.AtivoId,
                        principalTable: "Ativo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ValorPatrimonialAtivo_AtivoId_DataReferencia",
                table: "ValorPatrimonialAtivo",
                columns: new[]
                {
                    "AtivoId",
                    "DataReferencia"
                },
                unique: true);
        }

        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ValorPatrimonialAtivo");
        }
    }
}
