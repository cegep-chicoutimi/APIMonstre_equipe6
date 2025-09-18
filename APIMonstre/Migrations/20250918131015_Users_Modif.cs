using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIMonstre.Migrations
{
    /// <inheritdoc />
    public partial class Users_Modif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personnage_Utilisateur_UtilisateurIdUtilisateur",
                table: "Personnage");

            migrationBuilder.DropIndex(
                name: "IX_Personnage_UtilisateurIdUtilisateur",
                table: "Personnage");

            migrationBuilder.DropColumn(
                name: "UtilisateurIdUtilisateur",
                table: "Personnage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UtilisateurIdUtilisateur",
                table: "Personnage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Personnage_UtilisateurIdUtilisateur",
                table: "Personnage",
                column: "UtilisateurIdUtilisateur");

            migrationBuilder.AddForeignKey(
                name: "FK_Personnage_Utilisateur_UtilisateurIdUtilisateur",
                table: "Personnage",
                column: "UtilisateurIdUtilisateur",
                principalTable: "Utilisateur",
                principalColumn: "IdUtilisateur",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
