using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LingoAITutor.Host.Migrations
{
    /// <inheritdoc />
    public partial class LessonMessageLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Lessons_LessonId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Lessons",
                newName: "SectionNumber");

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "Messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectionNumber",
                table: "Messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "SectionNumber",
                table: "Lessons",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Lessons_LessonId",
                table: "Messages",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "LessonId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Lessons_LessonId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SectionNumber",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "SectionNumber",
                table: "Lessons",
                newName: "Number");

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "Messages",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "Lessons",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Lessons_LessonId",
                table: "Messages",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "LessonId");
        }
    }
}
