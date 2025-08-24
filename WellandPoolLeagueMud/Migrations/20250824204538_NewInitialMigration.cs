using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellandPoolLeagueMud.Migrations
{
    /// <inheritdoc />
    public partial class NewInitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WPLMud_Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WPLMud_Players", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "WPLMud_Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CaptainPlayerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WPLMud_Teams", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_WPLMud_Teams_WPLMud_Players_CaptainPlayerId",
                        column: x => x.CaptainPlayerId,
                        principalTable: "WPLMud_Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WPLMud_PlayerGames",
                columns: table => new
                {
                    PlayerGameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    IsWin = table.Column<bool>(type: "bit", nullable: false),
                    GameDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WPLMud_PlayerGames", x => x.PlayerGameId);
                    table.ForeignKey(
                        name: "FK_WPLMud_PlayerGames_WPLMud_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "WPLMud_Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WPLMud_PlayerGames_WPLMud_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "WPLMud_Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WPLMud_Schedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    GameDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WinningTeamId = table.Column<int>(type: "int", nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WPLMud_Schedules", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_WPLMud_Schedules_WPLMud_Teams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "WPLMud_Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WPLMud_Schedules_WPLMud_Teams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "WPLMud_Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WPLMud_Schedules_WPLMud_Teams_WinningTeamId",
                        column: x => x.WinningTeamId,
                        principalTable: "WPLMud_Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WPLMud_PlayerGames_PlayerId_WeekNumber_TeamId",
                table: "WPLMud_PlayerGames",
                columns: new[] { "PlayerId", "WeekNumber", "TeamId" });

            migrationBuilder.CreateIndex(
                name: "IX_WPLMud_PlayerGames_TeamId",
                table: "WPLMud_PlayerGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_WPLMud_Players_FirstName_LastName",
                table: "WPLMud_Players",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_WPLMud_Schedules_AwayTeamId",
                table: "WPLMud_Schedules",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_WPLMud_Schedules_HomeTeamId",
                table: "WPLMud_Schedules",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_WPLMud_Schedules_WeekNumber_GameDate",
                table: "WPLMud_Schedules",
                columns: new[] { "WeekNumber", "GameDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WPLMud_Schedules_WinningTeamId",
                table: "WPLMud_Schedules",
                column: "WinningTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_WPLMud_Teams_CaptainPlayerId",
                table: "WPLMud_Teams",
                column: "CaptainPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_WPLMud_Teams_TeamName",
                table: "WPLMud_Teams",
                column: "TeamName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WPLMud_PlayerGames");

            migrationBuilder.DropTable(
                name: "WPLMud_Schedules");

            migrationBuilder.DropTable(
                name: "WPLMud_Teams");

            migrationBuilder.DropTable(
                name: "WPLMud_Players");
        }
    }
}
