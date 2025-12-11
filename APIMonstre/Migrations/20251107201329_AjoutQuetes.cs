using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIMonstre.Migrations
{
    /// <inheritdoc />
    public partial class AjoutQuetes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChasseQuetes",
                columns: table => new
                {
                    IdChasseQuetes = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NbTue = table.Column<int>(type: "int", nullable: false),
                    ObjectifTue = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstComplete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PersonnageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PrimaryKey_ChasseQuetes", x => x.IdChasseQuetes);
                    table.ForeignKey(
                        name: "FK_ChasseQuetes_Personnage_PersonnageId",
                        column: x => x.PersonnageId,
                        principalTable: "Personnage",
                        principalColumn: "IdPersonnage",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LevelUpQuetes",
                columns: table => new
                {
                    IdLevelUpQuetes = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NiveauDepart = table.Column<int>(type: "int", nullable: false),
                    NiveauObjectif = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstComplete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PersonnageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PrimaryKey_LevelUpQuetes", x => x.IdLevelUpQuetes);
                    table.ForeignKey(
                        name: "FK_LevelUpQuetes_Personnage_PersonnageId",
                        column: x => x.PersonnageId,
                        principalTable: "Personnage",
                        principalColumn: "IdPersonnage",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RandonneQuetes",
                columns: table => new
                {
                    IdRandonneQuetes = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TuileX = table.Column<int>(type: "int", nullable: false),
                    TuileY = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstComplete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PersonnageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PrimaryKey_RandonneQuetes", x => x.IdRandonneQuetes);
                    table.ForeignKey(
                        name: "FK_RandonneQuetes_Personnage_PersonnageId",
                        column: x => x.PersonnageId,
                        principalTable: "Personnage",
                        principalColumn: "IdPersonnage",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RandonneQuetes_Tuile_TuileX_TuileY",
                        columns: x => new { x.TuileX, x.TuileY },
                        principalTable: "Tuile",
                        principalColumns: new[] { "PositionX", "PositionY" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChasseQuetes_PersonnageId",
                table: "ChasseQuetes",
                column: "PersonnageId");

            migrationBuilder.CreateIndex(
                name: "IX_LevelUpQuetes_PersonnageId",
                table: "LevelUpQuetes",
                column: "PersonnageId");

            migrationBuilder.CreateIndex(
                name: "IX_RandonneQuetes_PersonnageId",
                table: "RandonneQuetes",
                column: "PersonnageId");

            migrationBuilder.CreateIndex(
                name: "IX_RandonneQuetes_TuileX_TuileY",
                table: "RandonneQuetes",
                columns: new[] { "TuileX", "TuileY" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChasseQuetes");

            migrationBuilder.DropTable(
                name: "LevelUpQuetes");

            migrationBuilder.DropTable(
                name: "RandonneQuetes");
        }
    }
}
