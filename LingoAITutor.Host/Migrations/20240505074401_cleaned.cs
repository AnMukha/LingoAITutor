using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LingoAITutor.Host.Migrations
{
    /// <inheritdoc />
    public partial class cleaned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RangeProgresses");

            migrationBuilder.DropTable(
                name: "UserTextProgresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RangeProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProgressId = table.Column<Guid>(type: "uuid", nullable: false),
                    Progress = table.Column<double>(type: "double precision", nullable: true),
                    StartPosition = table.Column<int>(type: "integer", nullable: false),
                    WordsCount = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "UserTextProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SentenceNumber = table.Column<int>(type: "integer", nullable: false),
                    TextId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTextProgresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RangeProgresses_UserProgressId",
                table: "RangeProgresses",
                column: "UserProgressId");
        }
    }
}
