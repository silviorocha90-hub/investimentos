using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    public partial class NormalizarStatusOpcoes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE OperacaoOpcao SET Situacao = 'EXECUTADA' WHERE Situacao = 'EXERCIDA';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE OperacaoOpcao SET Situacao = 'EXERCIDA' WHERE Situacao = 'EXECUTADA';");
        }
    }
}
