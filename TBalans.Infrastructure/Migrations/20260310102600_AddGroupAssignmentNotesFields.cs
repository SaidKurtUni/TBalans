using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBalans.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupAssignmentNotesFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "EstimatedHours",
                table: "GroupAssignments",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentNotes",
                table: "GroupAssignments",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "GroupAssignments");

            migrationBuilder.DropColumn(
                name: "StudentNotes",
                table: "GroupAssignments");
        }
    }
}
