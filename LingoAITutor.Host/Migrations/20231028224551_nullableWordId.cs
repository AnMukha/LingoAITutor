using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LingoAITutor.Host.Migrations
{
    /// <inheritdoc />
    public partial class nullableWordId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWordProgresses_Words_WordID",
                table: "UserWordProgresses");

            migrationBuilder.AlterColumn<Guid>(
                name: "WordID",
                table: "UserWordProgresses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_UserWordProgresses_Words_WordID",
                table: "UserWordProgresses",
                column: "WordID",
                principalTable: "Words",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWordProgresses_Words_WordID",
                table: "UserWordProgresses");

            migrationBuilder.AlterColumn<Guid>(
                name: "WordID",
                table: "UserWordProgresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserWordProgresses_Words_WordID",
                table: "UserWordProgresses",
                column: "WordID",
                principalTable: "Words",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
