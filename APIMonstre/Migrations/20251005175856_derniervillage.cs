using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIMonstre.Migrations
{
    /// <inheritdoc />
    public partial class derniervillage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DernierVillageX",
                table: "Personnage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DernierVillageY",
                table: "Personnage",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DernierVillageX",
                table: "Personnage");

            migrationBuilder.DropColumn(
                name: "DernierVillageY",
                table: "Personnage");
        }
    }
}
