using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPedidosReparacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PedidosReparacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VeiculoId = table.Column<int>(type: "int", nullable: false),
                    DescricaoProblema = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataSubmissao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObservacoesAdmin = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosReparacao", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PedidosReparacao");
        }
    }
}
