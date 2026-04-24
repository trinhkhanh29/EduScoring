using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduScoring.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAiPipelineEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwardedScore",
                table: "AiEvaluations");

            migrationBuilder.RenameColumn(
                name: "AiFeedback",
                table: "AiEvaluations",
                newName: "OverallFeedback");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalScore",
                table: "Submissions",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CombinedOcrText",
                table: "Submissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EvaluationCount",
                table: "Submissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalScore",
                table: "Submissions",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HumanScore",
                table: "Submissions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Submissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Submissions",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastEvaluationTrigger",
                table: "Submissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "LatestAiScore",
                table: "Submissions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OcrCleanedText",
                table: "SubmissionImages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OcrConfidence",
                table: "SubmissionImages",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OcrEngine",
                table: "SubmissionImages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ConfidenceScore",
                table: "AiEvaluations",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "AiEvaluations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PromptVersion",
                table: "AiEvaluations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "RawResponse",
                table: "AiEvaluations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AiEvaluations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "TotalScore",
                table: "AiEvaluations",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "AiEvaluationDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AiEvaluationId = table.Column<int>(type: "integer", nullable: false),
                    RubricId = table.Column<int>(type: "integer", nullable: true),
                    CriteriaKey = table.Column<string>(type: "text", nullable: false),
                    CriteriaName = table.Column<string>(type: "text", nullable: false),
                    CriteriaGroup = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    MaxScore = table.Column<double>(type: "double precision", nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: false),
                    Reasoning = table.Column<string>(type: "text", nullable: false),
                    Evidence = table.Column<string>(type: "text", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiEvaluationDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiEvaluationDetails_AiEvaluations_AiEvaluationId",
                        column: x => x.AiEvaluationId,
                        principalTable: "AiEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiEvaluationDetails_Rubrics_RubricId",
                        column: x => x.RubricId,
                        principalTable: "Rubrics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HumanEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TeacherFeedback = table.Column<string>(type: "text", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HumanEvaluations_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiEvaluationDetails_AiEvaluationId",
                table: "AiEvaluationDetails",
                column: "AiEvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_AiEvaluationDetails_RubricId",
                table: "AiEvaluationDetails",
                column: "RubricId");

            migrationBuilder.CreateIndex(
                name: "IX_HumanEvaluations_SubmissionId",
                table: "HumanEvaluations",
                column: "SubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiEvaluationDetails");

            migrationBuilder.DropTable(
                name: "HumanEvaluations");

            migrationBuilder.DropColumn(
                name: "CombinedOcrText",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "EvaluationCount",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FinalScore",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "HumanScore",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "LastEvaluationTrigger",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "LatestAiScore",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "OcrCleanedText",
                table: "SubmissionImages");

            migrationBuilder.DropColumn(
                name: "OcrConfidence",
                table: "SubmissionImages");

            migrationBuilder.DropColumn(
                name: "OcrEngine",
                table: "SubmissionImages");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "AiEvaluations");

            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "AiEvaluations");

            migrationBuilder.DropColumn(
                name: "PromptVersion",
                table: "AiEvaluations");

            migrationBuilder.DropColumn(
                name: "RawResponse",
                table: "AiEvaluations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AiEvaluations");

            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "AiEvaluations");

            migrationBuilder.RenameColumn(
                name: "OverallFeedback",
                table: "AiEvaluations",
                newName: "AiFeedback");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalScore",
                table: "Submissions",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AwardedScore",
                table: "AiEvaluations",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
