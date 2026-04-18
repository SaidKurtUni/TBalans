using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBalans.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupAssignmentSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "FaqData",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "ImportantNotes",
                table: "Assignments");

            migrationBuilder.AddColumn<string>(
                name: "FaqData",
                table: "GroupAssignments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportantNotes",
                table: "GroupAssignments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GroupAssignmentSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupAssignmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MethodDescription = table.Column<string>(type: "TEXT", nullable: false),
                    ToolsUsed = table.Column<string>(type: "TEXT", nullable: false),
                    ResultSummary = table.Column<string>(type: "TEXT", nullable: false),
                    FileUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAssignmentSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupAssignmentSubmissions_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupAssignmentSubmissions_GroupAssignments_GroupAssignmentId",
                        column: x => x.GroupAssignmentId,
                        principalTable: "GroupAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupAssignmentSubmissions_GroupAssignmentId",
                table: "GroupAssignmentSubmissions",
                column: "GroupAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAssignmentSubmissions_StudentId",
                table: "GroupAssignmentSubmissions",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupAssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "FaqData",
                table: "GroupAssignments");

            migrationBuilder.DropColumn(
                name: "ImportantNotes",
                table: "GroupAssignments");

            migrationBuilder.AddColumn<string>(
                name: "FaqData",
                table: "Assignments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportantNotes",
                table: "Assignments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssignmentSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FileUrl = table.Column<string>(type: "TEXT", nullable: true),
                    MethodDescription = table.Column<string>(type: "TEXT", nullable: false),
                    ResultSummary = table.Column<string>(type: "TEXT", nullable: false),
                    ToolsUsed = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_AssignmentId",
                table: "AssignmentSubmissions",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_StudentId",
                table: "AssignmentSubmissions",
                column: "StudentId");
        }
    }
}
