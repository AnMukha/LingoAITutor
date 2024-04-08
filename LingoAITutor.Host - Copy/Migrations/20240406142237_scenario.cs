using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LingoAITutor.Host.Migrations
{
    /// <inheritdoc />
    public partial class scenario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AIModeInChat",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomContent",
                table: "Lessons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScenarioId",
                table: "Lessons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ScenarioTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ScenarioType = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    AIModeInChat = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioTemplates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScenarioTemplates");

            migrationBuilder.DropColumn(
                name: "AIModeInChat",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "CustomContent",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "ScenarioId",
                table: "Lessons");
        }
    }
}
