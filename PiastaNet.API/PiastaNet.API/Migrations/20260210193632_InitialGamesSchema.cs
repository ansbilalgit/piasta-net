using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiastaNet.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialGamesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Length = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "No description provided"),
                    Thumbnail = table.Column<string>(type: "nvarchar(max)", nullable: true, defaultValue: "https://i.imgur.com/OJhoTqu.png"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Copies = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "boardgames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    MinPlayers = table.Column<int>(type: "int", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    BggId = table.Column<int>(type: "int", nullable: true),
                    BggRating = table.Column<double>(type: "float", nullable: true),
                    BggAverageRating = table.Column<double>(type: "float", nullable: true),
                    BggRank = table.Column<int>(type: "int", nullable: true),
                    LearnDifficulty = table.Column<int>(type: "int", nullable: true),
                    PlayDifficulty = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boardgames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_boardgames_items_Id",
                        column: x => x.Id,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    category = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => new { x.id, x.category });
                    table.ForeignKey(
                        name: "FK_categories_items_id",
                        column: x => x.id,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "videogames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    MinPlayers = table.Column<int>(type: "int", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    PlayingTime = table.Column<int>(type: "int", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: true),
                    Platform = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_videogames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_videogames_items_Id",
                        column: x => x.Id,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_items_Name",
                table: "items",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "boardgames");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "videogames");

            migrationBuilder.DropTable(
                name: "items");
        }
    }
}
