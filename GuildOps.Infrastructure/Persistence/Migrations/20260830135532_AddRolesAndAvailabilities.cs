using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuildOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndAvailabilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Availabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Availabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Availabilities_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameRoles_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterGameRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterGameRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterGameRoles_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterGameRoles_GameRoles_GameRoleId",
                        column: x => x.GameRoleId,
                        principalTable: "GameRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_CharacterId_Day_Slot",
                table: "Availabilities",
                columns: new[] { "CharacterId", "Day", "Slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterGameRoles_CharacterId_GameRoleId",
                table: "CharacterGameRoles",
                columns: new[] { "CharacterId", "GameRoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterGameRoles_GameRoleId",
                table: "CharacterGameRoles",
                column: "GameRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRoles_GameId_Name",
                table: "GameRoles",
                columns: new[] { "GameId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Availabilities");

            migrationBuilder.DropTable(
                name: "CharacterGameRoles");

            migrationBuilder.DropTable(
                name: "GameRoles");
        }
    }
}
