using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Investimentos.Infrastructure.Persistence;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    [DbContext(typeof(InvestimentosDbContext))]
    [Migration("20261014030000_AdicionarCotacaoMercadoOperacaoOpcao")]
    public partial class AdicionarCotacaoMercadoOperacaoOpcao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(name: "PrecoMercadoAtual", table: "OperacaoOpcao", type: "decimal(18,8)", nullable: true);
            migrationBuilder.AddColumn<System.DateTime>(name: "DataAtualizacaoCotacao", table: "OperacaoOpcao", type: "datetime2", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PrecoMercadoAtual", table: "OperacaoOpcao");
            migrationBuilder.DropColumn(name: "DataAtualizacaoCotacao", table: "OperacaoOpcao");
        }
    }
}
