using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBalans.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentStudentNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "EstimatedHours",
                table: "Assignments",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AddColumn<string>(
                name: "StudentNotes",
                table: "Assignments",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentNotes",
                table: "Assignments");

            migrationBuilder.AlterColumn<double>(
                name: "EstimatedHours",
                table: "Assignments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);
        }
    }
}
