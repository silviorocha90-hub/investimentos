using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace Investimentos.Infrastructure.Migrations;

[DbContext(typeof(InvestimentosDbContext))]
[Migration("20261013160000_AdicionarLogsAplicacao")]
public sealed class AdicionarLogsAplicacao : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LogsAplicacao",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DataUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                Ambiente = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                Modulo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Operacao = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Nivel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Mensagem = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                Detalhes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CorrelacaoId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_LogsAplicacao", x => x.Id));
        migrationBuilder.CreateIndex("IX_LogsAplicacao_DataUtc", "LogsAplicacao", "DataUtc");
        migrationBuilder.CreateIndex("IX_LogsAplicacao_CorrelacaoId", "LogsAplicacao", "CorrelacaoId");
        migrationBuilder.CreateIndex("IX_LogsAplicacao_Modulo_Nivel_DataUtc", "LogsAplicacao",
            new[] { "Modulo", "Nivel", "DataUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("LogsAplicacao");
    }
}
