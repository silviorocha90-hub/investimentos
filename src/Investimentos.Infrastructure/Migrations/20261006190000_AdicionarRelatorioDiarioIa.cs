using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations;

[DbContext(typeof(InvestimentosDbContext))]
[Migration("20261006190000_AdicionarRelatorioDiarioIa")]
public partial class AdicionarRelatorioDiarioIa : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RelatorioDiarioIa",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false),
                DataReferencia = table.Column<DateTime>(
                    type: "date",
                    nullable: false),
                DataGeracao = table.Column<DateTime>(
                    type: "datetime2",
                    nullable: false),
                Conteudo = table.Column<string>(
                    type: "nvarchar(max)",
                    nullable: false),
                Modelo = table.Column<string>(
                    type: "nvarchar(100)",
                    maxLength: 100,
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_RelatorioDiarioIa",
                    x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RelatorioDiarioIa_DataReferencia",
            table: "RelatorioDiarioIa",
            column: "DataReferencia",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RelatorioDiarioIa");
    }
}
