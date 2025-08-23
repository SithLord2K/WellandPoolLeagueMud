using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellandPoolLeagueMud.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wiley");

            migrationBuilder.CreateTable(
                name: "WPL_Teams",
                schema: "wiley",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaptainPlayerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WPL_Teams", x => x.TeamId);
                });

            migrationBuilder.CreateTable(
                name: "WPL_Players",
                schema: "wiley",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GamesPlayed = table.Column<int>(type: "int", nullable: false),
                    IsCaptain = table.Column<bool>(type: "bit", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WPL_Players", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_WPL_Players_WPL_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "wiley",
                        principalTable: "WPL_Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WPL_Schedules",
                schema: "wiley",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    GameDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    Playoffs = table.Column<bool>(type: "bit", nullable: false),
                    TableNumber = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WinningTeamId = table.Column<int>(type: "int", nullable: true),
                    Forfeit = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WPL_Schedules", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_WPL_Schedules_WPL_Teams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalSchema: "wiley",
                        principalTable: "WPL_Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WPL_Schedules_WPL_Teams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalSchema: "wiley",
                        principalTable: "WPL_Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WPL_Schedules_WPL_Teams_WinningTeamId",
                        column: x => x.WinningTeamId,
                        principalSchema: "wiley",
                        principalTable: "WPL_Teams",
                        principalColumn: "TeamId");
                });

            migrationBuilder.CreateTable(
                name: "WPL_WeeklyWinners",
                schema: "wiley",
                columns: table => new
                {
                    WeeklyWinnerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    WinningTeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WPL_WeeklyWinners", x => x.WeeklyWinnerId);
                    table.ForeignKey(
                        name: "FK_WPL_WeeklyWinners_WPL_Teams_WinningTeamId",
                        column: x => x.WinningTeamId,
                        principalSchema: "wiley",
                        principalTable: "WPL_Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WPL_PlayerGames",
                schema: "wiley",
                columns: table => new
                {
                    PlayerGameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    FramesWon = table.Column<int>(type: "int", nullable: false),
                    FramesLost = table.Column<int>(type: "int", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WPL_PlayerGames", x => x.PlayerGameId);
                    table.ForeignKey(
                        name: "FK_WPL_PlayerGames_WPL_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "wiley",
                        principalTable: "WPL_Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WPL_PlayerGames_WPL_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "wiley",
                        principalTable: "WPL_Schedules",
                        principalColumn: "ScheduleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WPL_PlayerGames_PlayerId",
                schema: "wiley",
                table: "WPL_PlayerGames",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_WPL_PlayerGames_ScheduleId",
                schema: "wiley",
                table: "WPL_PlayerGames",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WPL_Players_TeamId",
                schema: "wiley",
                table: "WPL_Players",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_WPL_Schedules_AwayTeamId",
                schema: "wiley",
                table: "WPL_Schedules",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_WPL_Schedules_HomeTeamId",
                schema: "wiley",
                table: "WPL_Schedules",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_WPL_Schedules_WinningTeamId",
                schema: "wiley",
                table: "WPL_Schedules",
                column: "WinningTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_WPL_WeeklyWinners_WinningTeamId",
                schema: "wiley",
                table: "WPL_WeeklyWinners",
                column: "WinningTeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WPL_PlayerGames",
                schema: "wiley");

            migrationBuilder.DropTable(
                name: "WPL_WeeklyWinners",
                schema: "wiley");

            migrationBuilder.DropTable(
                name: "WPL_Players",
                schema: "wiley");

            migrationBuilder.DropTable(
                name: "WPL_Schedules",
                schema: "wiley");

            migrationBuilder.DropTable(
                name: "WPL_Teams",
                schema: "wiley");
        }
    }
}
