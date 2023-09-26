using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LingoAITutor.Host.Migrations
{
    /// <inheritdoc />
    public partial class progressDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MasteryLevel",
                table: "UserWordProgresses",
                newName: "ReplacedBySynonyms");

            migrationBuilder.AddColumn<int>(
                name: "CorrectUses",
                table: "UserWordProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstimationExerciseNumber",
                table: "UserWordProgresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstimationExerciseResult",
                table: "UserWordProgresses",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FailedToUseFlag",
                table: "UserWordProgresses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NonUses",
                table: "UserWordProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkOutStart",
                table: "UserWordProgresses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserProgresses",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseNumber = table.Column<int>(type: "int", nullable: false),
                    EstimationNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgresses", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "RangeProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProgressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Progress = table.Column<double>(type: "float", nullable: true),
                    WordsCount = table.Column<int>(type: "int", nullable: false),
                    StartPosition = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RangeProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RangeProgresses_UserProgresses_UserProgressId",
                        column: x => x.UserProgressId,
                        principalTable: "UserProgresses",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RangeProgresses_UserProgressId",
                table: "RangeProgresses",
                column: "UserProgressId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RangeProgresses");

            migrationBuilder.DropTable(
                name: "UserProgresses");

            migrationBuilder.DropColumn(
                name: "CorrectUses",
                table: "UserWordProgresses");

            migrationBuilder.DropColumn(
                name: "EstimationExerciseNumber",
                table: "UserWordProgresses");

            migrationBuilder.DropColumn(
                name: "EstimationExerciseResult",
                table: "UserWordProgresses");

            migrationBuilder.DropColumn(
                name: "FailedToUseFlag",
                table: "UserWordProgresses");

            migrationBuilder.DropColumn(
                name: "NonUses",
                table: "UserWordProgresses");

            migrationBuilder.DropColumn(
                name: "WorkOutStart",
                table: "UserWordProgresses");

            migrationBuilder.RenameColumn(
                name: "ReplacedBySynonyms",
                table: "UserWordProgresses",
                newName: "MasteryLevel");
        }
    }
}
