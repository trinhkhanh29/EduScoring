using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduScoring.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExamDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "Submissions");

            migrationBuilder.AddColumn<string>(
                name: "CreatedSource",
                table: "Submissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubmissionMode",
                table: "Submissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "AllowAppeal",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowStudentSubmission",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireTeacherReview",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedSource",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SubmissionMode",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "AllowAppeal",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "AllowStudentSubmission",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "RequireTeacherReview",
                table: "Exams");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalScore",
                table: "Submissions",
                type: "numeric",
                nullable: true);
        }
    }
}
