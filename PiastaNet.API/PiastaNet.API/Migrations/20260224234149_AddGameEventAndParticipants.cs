using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiastaNet.API.Migrations
{
    /// <inheritdoc />
    public partial class AddGameEventAndParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameEventParticipant",
                columns: table => new
                {
                    GameEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEventParticipant", x => new { x.GameEventId, x.ParticipantUserId, x.RequestedByUserId });
                    table.ForeignKey(
                        name: "FK_GameEventParticipant_GameEvents_GameEventId",
                        column: x => x.GameEventId,
                        principalTable: "GameEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameEventParticipant");
        }
    }
}
