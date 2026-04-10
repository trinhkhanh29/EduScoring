using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduScoring.Migrations
{
    /// <inheritdoc />
    public partial class BaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== ADD SOFT DELETE COLUMNS =====

            void AddSoftDelete(string table)
            {
                migrationBuilder.AddColumn<DateTimeOffset>(
                    name: "DeletedAt",
                    table: table,
                    type: "timestamp with time zone",
                    nullable: true);

                migrationBuilder.AddColumn<Guid>(
                    name: "DeletedBy",
                    table: table,
                    type: "uuid",
                    nullable: true);

                migrationBuilder.AddColumn<bool>(
                    name: "IsDeleted",
                    table: table,
                    type: "boolean",
                    nullable: false,
                    defaultValue: false);
            }

            AddSoftDelete("Users");
            AddSoftDelete("Exams");
            AddSoftDelete("Submissions");
            AddSoftDelete("SubmissionImages");
            AddSoftDelete("Rubrics");
            AddSoftDelete("Appeals");

            // ===== FIX USER TOKEN INDEX =====
            //migrationBuilder.CreateIndex(
            //    name: "IX_UserTokens_UserId_LoginProvider_Name",
            //    table: "UserTokens",
            //    columns: new[] { "UserId", "LoginProvider", "Name" },
            //    unique: true);//

            //// ===== FIX SUBMISSION INDEX =====
            //migrationBuilder.CreateIndex(
            //    name: "IX_Submissions_ExamId_StudentId",
            //    table: "Submissions",
            //    columns: new[] { "ExamId", "StudentId" },
            //    unique: true);//
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            void RemoveSoftDelete(string table)
            {
                migrationBuilder.DropColumn(name: "DeletedAt", table: table);
                migrationBuilder.DropColumn(name: "DeletedBy", table: table);
                migrationBuilder.DropColumn(name: "IsDeleted", table: table);
            }

            RemoveSoftDelete("Users");
            RemoveSoftDelete("Exams");
            RemoveSoftDelete("Submissions");
            RemoveSoftDelete("SubmissionImages");
            RemoveSoftDelete("Rubrics");
            RemoveSoftDelete("Appeals");

            migrationBuilder.DropIndex(
                name: "IX_UserTokens_UserId_LoginProvider_Name",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ExamId_StudentId",
                table: "Submissions");
        }
    }
}
