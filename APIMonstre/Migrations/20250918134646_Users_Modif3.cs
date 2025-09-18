using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIMonstre.Migrations
{
    /// <inheritdoc />
    public partial class Users_Modif3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "estConnecte",
                table: "Utilisateur",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estConnecte",
                table: "Utilisateur");
        }
    }
}
