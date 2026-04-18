using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBalans.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFileFieldsToAssignmentComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "AssignmentComments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "AssignmentComments",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "AssignmentComments");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "AssignmentComments");
        }
    }
}
