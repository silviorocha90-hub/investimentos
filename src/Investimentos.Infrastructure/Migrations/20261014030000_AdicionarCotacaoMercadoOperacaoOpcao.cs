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
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.OperacaoOpcao', N'PrecoMercadoAtual') IS NULL ALTER TABLE [dbo].[OperacaoOpcao] ADD [PrecoMercadoAtual] decimal(18,8) NULL;");
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.OperacaoOpcao', N'DataAtualizacaoCotacao') IS NULL ALTER TABLE [dbo].[OperacaoOpcao] ADD [DataAtualizacaoCotacao] datetime2 NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.OperacaoOpcao', N'PrecoMercadoAtual') IS NOT NULL ALTER TABLE [dbo].[OperacaoOpcao] DROP COLUMN [PrecoMercadoAtual];");
            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.OperacaoOpcao', N'DataAtualizacaoCotacao') IS NOT NULL ALTER TABLE [dbo].[OperacaoOpcao] DROP COLUMN [DataAtualizacaoCotacao];");
        }
    }
}
