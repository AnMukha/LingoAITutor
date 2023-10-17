using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LingoAITutor.Host.Migrations
{
    /// <inheritdoc />
    public partial class irregular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Irregulars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    V1 = table.Column<string>(type: "text", nullable: false),
                    V2 = table.Column<string>(type: "text", nullable: false),
                    V3 = table.Column<string>(type: "text", nullable: false),
                    Optional = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Irregulars", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Irregulars");
        }
    }
}
