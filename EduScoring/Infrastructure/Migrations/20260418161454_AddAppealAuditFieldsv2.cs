using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduScoring.Migrations
{
    /// <inheritdoc />
    public partial class AddAppealAuditFieldsv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiEvaluationDetails_Rubrics_RubricId",
                table: "AiEvaluationDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AiEvaluations_Submissions_SubmissionId",
                table: "AiEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Appeals_Submissions_SubmissionId",
                table: "Appeals");

            migrationBuilder.DropForeignKey(
                name: "FK_HumanEvaluations_Submissions_SubmissionId",
                table: "HumanEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionImages_Submissions_SubmissionId",
                table: "SubmissionImages");

            migrationBuilder.AlterColumn<decimal>(
                name: "LatestAiScore",
                table: "Submissions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HumanScore",
                table: "Submissions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FinalScore",
                table: "Submissions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TeacherScore",
                table: "HumanEvaluations",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "TeacherFeedback",
                table: "HumanEvaluations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TeacherResponse",
                table: "Appeals",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StudentReason",
                table: "Appeals",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appeals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Open",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<decimal>(
                name: "NewScore",
                table: "Appeals",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousScore",
                table: "Appeals",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionType",
                table: "Appeals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResolvedAt",
                table: "Appeals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResolvedBy",
                table: "Appeals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AiEvaluations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CriteriaName",
                table: "AiEvaluationDetails",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CriteriaKey",
                table: "AiEvaluationDetails",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CriteriaGroup",
                table: "AiEvaluationDetails",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_AiEvaluationDetails_Rubrics_RubricId",
                table: "AiEvaluationDetails",
                column: "RubricId",
                principalTable: "Rubrics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AiEvaluations_Submissions_SubmissionId",
                table: "AiEvaluations",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appeals_Submissions_SubmissionId",
                table: "Appeals",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HumanEvaluations_Submissions_SubmissionId",
                table: "HumanEvaluations",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionImages_Submissions_SubmissionId",
                table: "SubmissionImages",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiEvaluationDetails_Rubrics_RubricId",
                table: "AiEvaluationDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AiEvaluations_Submissions_SubmissionId",
                table: "AiEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Appeals_Submissions_SubmissionId",
                table: "Appeals");

            migrationBuilder.DropForeignKey(
                name: "FK_HumanEvaluations_Submissions_SubmissionId",
                table: "HumanEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionImages_Submissions_SubmissionId",
                table: "SubmissionImages");

            migrationBuilder.DropColumn(
                name: "NewScore",
                table: "Appeals");

            migrationBuilder.DropColumn(
                name: "PreviousScore",
                table: "Appeals");

            migrationBuilder.DropColumn(
                name: "ResolutionType",
                table: "Appeals");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "Appeals");

            migrationBuilder.DropColumn(
                name: "ResolvedBy",
                table: "Appeals");

            migrationBuilder.AlterColumn<decimal>(
                name: "LatestAiScore",
                table: "Submissions",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HumanScore",
                table: "Submissions",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FinalScore",
                table: "Submissions",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TeacherScore",
                table: "HumanEvaluations",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "TeacherFeedback",
                table: "HumanEvaluations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "TeacherResponse",
                table: "Appeals",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StudentReason",
                table: "Appeals",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appeals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Open");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AiEvaluations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "CriteriaName",
                table: "AiEvaluationDetails",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "CriteriaKey",
                table: "AiEvaluationDetails",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CriteriaGroup",
                table: "AiEvaluationDetails",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_AiEvaluationDetails_Rubrics_RubricId",
                table: "AiEvaluationDetails",
                column: "RubricId",
                principalTable: "Rubrics",
                principalColumn: "Id");

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
                name: "FK_HumanEvaluations_Submissions_SubmissionId",
                table: "HumanEvaluations",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionImages_Submissions_SubmissionId",
                table: "SubmissionImages",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
