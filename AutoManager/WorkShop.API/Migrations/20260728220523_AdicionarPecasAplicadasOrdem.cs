using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPecasAplicadasOrdem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PecasAplicadasOrdem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdemReparacaoId = table.Column<int>(type: "int", nullable: false),
                    PecaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PecasAplicadasOrdem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PecasAplicadasOrdem_OrdensReparacao_OrdemReparacaoId",
                        column: x => x.OrdemReparacaoId,
                        principalTable: "OrdensReparacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PecasAplicadasOrdem_OrdemReparacaoId",
                table: "PecasAplicadasOrdem",
                column: "OrdemReparacaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PecasAplicadasOrdem");
        }
    }
}
