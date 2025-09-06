using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellandPoolLeagueMud.Migrations
{
    /// <inheritdoc />
    public partial class RemovePhoneAddTeamId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "WPLMud_Players");

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "WPLMud_Players",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WPLMud_Players_TeamId",
                table: "WPLMud_Players",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_WPLMud_Players_WPLMud_Teams_TeamId",
                table: "WPLMud_Players",
                column: "TeamId",
                principalTable: "WPLMud_Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WPLMud_Players_WPLMud_Teams_TeamId",
                table: "WPLMud_Players");

            migrationBuilder.DropIndex(
                name: "IX_WPLMud_Players_TeamId",
                table: "WPLMud_Players");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "WPLMud_Players");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "WPLMud_Players",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);
        }
    }
}
