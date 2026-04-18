using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBalans.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Assignments",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Assignments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Assignments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Assignments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_UserId",
                table: "Assignments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_AspNetUsers_UserId",
                table: "Assignments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_AspNetUsers_UserId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_UserId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Assignments");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Assignments",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
