using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIMonstre.Migrations
{
    /// <inheritdoc />
    public partial class QuetesUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nom",
                table: "RandonneQuetes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "XpRecompense",
                table: "RandonneQuetes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Nom",
                table: "LevelUpQuetes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "XpRecompense",
                table: "LevelUpQuetes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Nom",
                table: "ChasseQuetes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "XpRecompense",
                table: "ChasseQuetes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nom",
                table: "RandonneQuetes");

            migrationBuilder.DropColumn(
                name: "XpRecompense",
                table: "RandonneQuetes");

            migrationBuilder.DropColumn(
                name: "Nom",
                table: "LevelUpQuetes");

            migrationBuilder.DropColumn(
                name: "XpRecompense",
                table: "LevelUpQuetes");

            migrationBuilder.DropColumn(
                name: "Nom",
                table: "ChasseQuetes");

            migrationBuilder.DropColumn(
                name: "XpRecompense",
                table: "ChasseQuetes");
        }
    }
}
