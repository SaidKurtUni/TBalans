using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBalans.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSemesterEndAndMakeupDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MakeupExamsEndDate",
                table: "Groups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MakeupExamsStartDate",
                table: "Groups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SemesterEndDate",
                table: "Groups",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MakeupExamsEndDate",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "MakeupExamsStartDate",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "SemesterEndDate",
                table: "Groups");
        }
    }
}
