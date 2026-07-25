using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoatTechChallenge.Infrastucture.Database.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoCpfEAtivoAoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "usuario",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "cpf",
                table: "usuario",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_cpf",
                table: "usuario",
                column: "cpf",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuario_cpf",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "ativo",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "cpf",
                table: "usuario");
        }
    }
}
