using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LingoAITutor.Host.Migrations
{
    /// <inheritdoc />
    public partial class Corrected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FixedContent",
                table: "Messages",
                newName: "Correctinos");

            migrationBuilder.AddColumn<string>(
                name: "CorrectedContent",
                table: "Messages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectedContent",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "Correctinos",
                table: "Messages",
                newName: "FixedContent");
        }
    }
}
