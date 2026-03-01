using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncVerse.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskDurationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TaskCompletedAt",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TaskStartedAt",
                table: "Tasks",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskCompletedAt",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskStartedAt",
                table: "Tasks");
        }
    }
}
