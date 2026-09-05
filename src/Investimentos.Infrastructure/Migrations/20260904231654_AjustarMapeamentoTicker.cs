using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    public partial class AjustarMapeamentoTicker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // O índice IX_Ativo_Ticker já foi criado
            // manualmente na migration inicial.
            // Esta migration apenas sincroniza o ModelSnapshot
            // com o modelo atual do EF Core.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nenhuma alteração física foi realizada no banco.
        }
    }
}