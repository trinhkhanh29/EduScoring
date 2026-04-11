using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduScoring.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_Users_UserId",
                table: "ActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AiEvaluations_Rubrics_RubricId",
                table: "AiEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_AiEvaluations_Submissions_SubmissionId",
                table: "AiEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Appeals_Submissions_SubmissionId",
                table: "Appeals");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Users_TeacherId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Rubrics_Exams_ExamId",
                table: "Rubrics");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionImages_Submissions_SubmissionId",
                table: "SubmissionImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Exams_ExamId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Users_StudentId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

            //migrationBuilder.DropIndex(
            //    name: "IX_Submissions_ExamId",
            //    table: "Submissions");//

            // ĐÃ XÓA TOÀN BỘ CÁC LỆNH DROP/ADD CỦA UserId1
            // ĐÃ XÓA LỆNH TẠO INDEX IX_Submissions_ExamId_StudentId BỊ TRÙNG

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_Users_UserId",
                table: "ActivityLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AiEvaluations_Rubrics_RubricId",
                table: "AiEvaluations",
                column: "RubricId",
                principalTable: "Rubrics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AiEvaluations_Submissions_SubmissionId",
                table: "AiEvaluations",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appeals_Submissions_SubmissionId",
                table: "Appeals",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Users_TeacherId",
                table: "Exams",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Rubrics_Exams_ExamId",
                table: "Rubrics",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionImages_Submissions_SubmissionId",
                table: "SubmissionImages",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Exams_ExamId",
                table: "Submissions",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Users_StudentId",
                table: "Submissions",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_Users_UserId",
                table: "ActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AiEvaluations_Rubrics_RubricId",
                table: "AiEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_AiEvaluations_Submissions_SubmissionId",
                table: "AiEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Appeals_Submissions_SubmissionId",
                table: "Appeals");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Users_TeacherId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Rubrics_Exams_ExamId",
                table: "Rubrics");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionImages_Submissions_SubmissionId",
                table: "SubmissionImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Exams_ExamId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Users_StudentId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ExamId",
                table: "Submissions",
                column: "ExamId");

            // ĐÃ DỌN SẠCH UserId1 VÀ INDEX TRONG HÀM DOWN LUÔN

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_Users_UserId",
                table: "ActivityLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AiEvaluations_Rubrics_RubricId",
                table: "AiEvaluations",
                column: "RubricId",
                principalTable: "Rubrics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AiEvaluations_Submissions_SubmissionId",
                table: "AiEvaluations",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appeals_Submissions_SubmissionId",
                table: "Appeals",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Users_TeacherId",
                table: "Exams",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rubrics_Exams_ExamId",
                table: "Rubrics",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionImages_Submissions_SubmissionId",
                table: "SubmissionImages",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Exams_ExamId",
                table: "Submissions",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Users_StudentId",
                table: "Submissions",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}