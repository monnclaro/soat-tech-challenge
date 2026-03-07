using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoatTechChallenge.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoEntidadesCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clienteveiculos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    placa = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    marca = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    modelo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ano = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clienteveiculos", x => x.id);
                    table.ForeignKey(
                        name: "FK_clienteveiculos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clientes_documento",
                table: "clientes",
                column: "documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clienteveiculos_cliente_id",
                table: "clienteveiculos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_clienteveiculos_placa",
                table: "clienteveiculos",
                column: "placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clienteveiculos");

            migrationBuilder.DropTable(
                name: "clientes");
        }
    }
}
