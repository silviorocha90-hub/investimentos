using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Investimentos.Infrastructure.Persistence;

#nullable disable

namespace Investimentos.Infrastructure.Migrations;

[DbContext(typeof(InvestimentosDbContext))]
[Migration("20261022050000_CalendarioDividendos")]
public partial class CalendarioDividendos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CalendariosDividendos",
            columns: table => new
            {
                InvestidorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Ticker = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                Meses = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                AtualizadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CalendariosDividendos", x => new { x.InvestidorId, x.Ticker });
                table.ForeignKey("FK_CalendariosDividendos_Investidor_InvestidorId",
                    x => x.InvestidorId, "Investidor", "Id", onDelete: ReferentialAction.Cascade);
            });
    }
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "CalendariosDividendos");
}
