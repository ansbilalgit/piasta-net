using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiastaNet.API.Migrations
{
    /// <inheritdoc />
    public partial class FixGameEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameEvent_items_GameId",
                table: "GameEvent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameEvent",
                table: "GameEvent");

            migrationBuilder.RenameTable(
                name: "GameEvent",
                newName: "GameEvents");

            migrationBuilder.RenameIndex(
                name: "IX_GameEvent_GameId",
                table: "GameEvents",
                newName: "IX_GameEvents_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameEvents",
                table: "GameEvents",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameEvents_items_GameId",
                table: "GameEvents",
                column: "GameId",
                principalTable: "items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameEvents_items_GameId",
                table: "GameEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameEvents",
                table: "GameEvents");

            migrationBuilder.RenameTable(
                name: "GameEvents",
                newName: "GameEvent");

            migrationBuilder.RenameIndex(
                name: "IX_GameEvents_GameId",
                table: "GameEvent",
                newName: "IX_GameEvent_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameEvent",
                table: "GameEvent",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameEvent_items_GameId",
                table: "GameEvent",
                column: "GameId",
                principalTable: "items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
