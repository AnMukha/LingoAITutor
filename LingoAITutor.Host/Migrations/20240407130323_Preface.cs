using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LingoAITutor.Host.Migrations
{
    /// <inheritdoc />
    public partial class Preface : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scenario",
                table: "Lessons");

            migrationBuilder.AddColumn<string>(
                name: "Preface",
                table: "ScenarioTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_ScenarioId",
                table: "Lessons",
                column: "ScenarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_ScenarioTemplates_ScenarioId",
                table: "Lessons",
                column: "ScenarioId",
                principalTable: "ScenarioTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_ScenarioTemplates_ScenarioId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_ScenarioId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Preface",
                table: "ScenarioTemplates");

            migrationBuilder.AddColumn<string>(
                name: "Scenario",
                table: "Lessons",
                type: "text",
                nullable: true);
        }
    }
}
