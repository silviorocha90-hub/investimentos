using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    public partial class AdicionarPermissoesPorInvestidor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuarioInvestidorPermissao",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvestidorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permissao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioInvestidorPermissao", x => new { x.UsuarioId, x.InvestidorId, x.Permissao });
                    table.ForeignKey("FK_UsuarioInvestidorPermissao_Investidor_InvestidorId", x => x.InvestidorId, "Investidor", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_UsuarioInvestidorPermissao_Usuario_UsuarioId", x => x.UsuarioId, "Usuario", "Id", onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(
                name: "IX_UsuarioInvestidorPermissao_InvestidorId",
                table: "UsuarioInvestidorPermissao",
                column: "InvestidorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UsuarioInvestidorPermissao");
        }
    }
}