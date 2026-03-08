using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoatTechChallenge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoEntidadesIniciais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cliente",
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
                    table.PrimaryKey("PK_cliente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordem_servico",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: false),
                    id_veiculo = table.Column<Guid>(type: "uuid", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_inicio_execucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_finalizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordem_servico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "produto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    quantidade_estoque = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servico",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cliente_veiculos",
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
                    table.PrimaryKey("PK_cliente_veiculos", x => x.id);
                    table.ForeignKey(
                        name: "FK_cliente_veiculos_cliente_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ordem_servico_produtos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idordemservico = table.Column<Guid>(type: "uuid", nullable: false),
                    idproduto = table.Column<Guid>(type: "uuid", nullable: false),
                    nomeproduto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    valorunitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordem_servico_produtos", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordem_servico_produtos_ordem_servico_idordemservico",
                        column: x => x.idordemservico,
                        principalTable: "ordem_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ordem_servico_servicos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idordemservico = table.Column<Guid>(type: "uuid", nullable: false),
                    idservico = table.Column<Guid>(type: "uuid", nullable: false),
                    nomeservico = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    valorunitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordem_servico_servicos", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordem_servico_servicos_ordem_servico_idordemservico",
                        column: x => x.idordemservico,
                        principalTable: "ordem_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cliente_documento",
                table: "cliente",
                column: "documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cliente_veiculos_cliente_id",
                table: "cliente_veiculos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_veiculos_placa",
                table: "cliente_veiculos",
                column: "placa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_produtos_idordemservico",
                table: "ordem_servico_produtos",
                column: "idordemservico");

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_servicos_idordemservico",
                table: "ordem_servico_servicos",
                column: "idordemservico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cliente_veiculos");

            migrationBuilder.DropTable(
                name: "ordem_servico_produtos");

            migrationBuilder.DropTable(
                name: "ordem_servico_servicos");

            migrationBuilder.DropTable(
                name: "produto");

            migrationBuilder.DropTable(
                name: "servico");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "ordem_servico");
        }
    }
}
