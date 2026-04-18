using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBalans.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupAssignmentComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignmentComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupAssignmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentComments_GroupAssignments_GroupAssignmentId",
                        column: x => x.GroupAssignmentId,
                        principalTable: "GroupAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentComments_GroupAssignmentId",
                table: "AssignmentComments",
                column: "GroupAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentComments_UserId",
                table: "AssignmentComments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentComments");
        }
    }
}
