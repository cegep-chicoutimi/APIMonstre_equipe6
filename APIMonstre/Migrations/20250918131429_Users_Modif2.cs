using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIMonstre.Migrations
{
    /// <inheritdoc />
    public partial class Users_Modif2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Personnage_IdUtilisateur",
                table: "Personnage",
                column: "IdUtilisateur");

            migrationBuilder.AddForeignKey(
                name: "FK_Personnage_Utilisateur_IdUtilisateur",
                table: "Personnage",
                column: "IdUtilisateur",
                principalTable: "Utilisateur",
                principalColumn: "IdUtilisateur",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personnage_Utilisateur_IdUtilisateur",
                table: "Personnage");

            migrationBuilder.DropIndex(
                name: "IX_Personnage_IdUtilisateur",
                table: "Personnage");
        }
    }
}
