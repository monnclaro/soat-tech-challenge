using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoatTechChallenge.Infrastucture.Database.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoClienteAtivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "cliente",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ativo",
                table: "cliente");
        }
    }
}
