using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIMonstre.Migrations
{
    /// <inheritdoc />
    public partial class Pokedex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HuntedMonster",
                columns: table => new
                {
                    IdPersonnage = table.Column<int>(type: "int", nullable: false),
                    IdMonster = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HuntedMonster", x => new { x.IdPersonnage, x.IdMonster });
                    table.ForeignKey(
                        name: "FK_HuntedMonster_Monster_IdMonster",
                        column: x => x.IdMonster,
                        principalTable: "Monster",
                        principalColumn: "IdMonster",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HuntedMonster_Personnage_IdPersonnage",
                        column: x => x.IdPersonnage,
                        principalTable: "Personnage",
                        principalColumn: "IdPersonnage",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_HuntedMonster_IdMonster",
                table: "HuntedMonster",
                column: "IdMonster");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HuntedMonster");
        }
    }
}
