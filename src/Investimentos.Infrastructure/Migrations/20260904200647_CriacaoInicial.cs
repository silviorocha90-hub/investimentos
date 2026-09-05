using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Investimentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClasseAtivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false),
                    Nome = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),
                    Ativo = table.Column<bool>(
                        type: "bit",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasseAtivo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoAtivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false),
                    Nome = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),
                    Ativo = table.Column<bool>(
                        type: "bit",
                        nullable: false),
                    ClasseAtivoId = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoAtivo", x => x.Id);

                    table.ForeignKey(
                        name: "FK_TipoAtivo_ClasseAtivo_ClasseAtivoId",
                        column: x => x.ClasseAtivoId,
                        principalTable: "ClasseAtivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ativo",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),
                    Nome = table.Column<string>(
                        type: "nvarchar(150)",
                        maxLength: 150,
                        nullable: false),
                    TipoAtivoId = table.Column<int>(
                        type: "int",
                        nullable: false),
                    Ticker = table.Column<string>(
                        type: "nvarchar(20)",
                        maxLength: 20,
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ativo", x => x.Id);

                    table.ForeignKey(
                        name: "FK_Ativo_TipoAtivo_TipoAtivoId",
                        column: x => x.TipoAtivoId,
                        principalTable: "TipoAtivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Dados iniciais das classes de ativos
            migrationBuilder.InsertData(
                table: "ClasseAtivo",
                columns: new[]
                {
                    "Id",
                    "Ativo",
                    "Codigo",
                    "Nome"
                },
                values: new object[,]
                {
                    {
                        1,
                        true,
                        "RENDA_FIXA",
                        "Renda Fixa"
                    },
                    {
                        2,
                        true,
                        "RENDA_VARIAVEL",
                        "Renda Variável"
                    },
                    {
                        3,
                        true,
                        "FUNDOS",
                        "Fundos"
                    },
                    {
                        4,
                        true,
                        "ALTERNATIVOS",
                        "Alternativos"
                    }
                });

            // Dados iniciais dos tipos de ativos
            migrationBuilder.InsertData(
                table: "TipoAtivo",
                columns: new[]
                {
                    "Id",
                    "Ativo",
                    "ClasseAtivoId",
                    "Codigo",
                    "Nome"
                },
                values: new object[,]
                {
                    { 1, true, 2, "ACAO", "Ação" },
                    { 2, true, 3, "FII", "Fundo Imobiliário" },
                    { 3, true, 3, "ETF", "ETF" },
                    { 4, true, 2, "BDR", "BDR" },

                    {
                        5,
                        true,
                        1,
                        "TESOURO_DIRETO",
                        "Tesouro Direto"
                    },

                    { 6, true, 1, "CDB", "CDB" },
                    { 7, true, 1, "LCI", "LCI" },
                    { 8, true, 1, "LCA", "LCA" },

                    {
                        9,
                        true,
                        1,
                        "DEBENTURE",
                        "Debênture"
                    },

                    {
                        10,
                        true,
                        1,
                        "POUPANCA",
                        "Poupança"
                    },

                    {
                        11,
                        true,
                        3,
                        "FUNDO_RENDA_FIXA",
                        "Fundo de Renda Fixa"
                    },

                    {
                        12,
                        true,
                        3,
                        "FUNDO_ACOES",
                        "Fundo de Ações"
                    },

                    {
                        13,
                        true,
                        3,
                        "FUNDO_MULTIMERCADO",
                        "Fundo Multimercado"
                    },

                    {
                        14,
                        true,
                        3,
                        "FUNDO_CAMBIAL",
                        "Fundo Cambial"
                    },

                    {
                        15,
                        true,
                        4,
                        "CRIPTOMOEDA",
                        "Criptomoeda"
                    },

                    {
                        16,
                        true,
                        4,
                        "CAMBIO",
                        "Câmbio"
                    },

                    {
                        17,
                        true,
                        4,
                        "COMMODITY",
                        "Commodity"
                    },

                    {
                        18,
                        true,
                        4,
                        "OURO",
                        "Ouro"
                    }
                });

            // Índices
            migrationBuilder.CreateIndex(
                name: "IX_Ativo_TipoAtivoId",
                table: "Ativo",
                column: "TipoAtivoId");

            // Ticker não pode se repetir.
            migrationBuilder.CreateIndex(
                name: "IX_Ativo_Ticker",
                table: "Ativo",
                column: "Ticker",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClasseAtivo_Codigo",
                table: "ClasseAtivo",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoAtivo_ClasseAtivoId",
                table: "TipoAtivo",
                column: "ClasseAtivoId");

            migrationBuilder.CreateIndex(
                name: "IX_TipoAtivo_Codigo",
                table: "TipoAtivo",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ativo");

            migrationBuilder.DropTable(
                name: "TipoAtivo");

            migrationBuilder.DropTable(
                name: "ClasseAtivo");
        }
    }
}