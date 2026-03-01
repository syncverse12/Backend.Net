using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncVerse.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTimeLogSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId",
                table: "TimeLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_TaskEmployees_TaskId",
                table: "TimeLogs");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TimeLogs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaskId",
                table: "TimeLogs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TimeLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskEmployeeId",
                table: "TimeLogs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeLogs_TaskEmployeeId",
                table: "TimeLogs",
                column: "TaskEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId",
                table: "TimeLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLogs_TaskEmployees_TaskEmployeeId",
                table: "TimeLogs",
                column: "TaskEmployeeId",
                principalTable: "TaskEmployees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLogs_Tasks_TaskId",
                table: "TimeLogs",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId",
                table: "TimeLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_TaskEmployees_TaskEmployeeId",
                table: "TimeLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_Tasks_TaskId",
                table: "TimeLogs");

            migrationBuilder.DropIndex(
                name: "IX_TimeLogs_TaskEmployeeId",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "TaskEmployeeId",
                table: "TimeLogs");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TimeLogs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "TaskId",
                table: "TimeLogs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId",
                table: "TimeLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLogs_TaskEmployees_TaskId",
                table: "TimeLogs",
                column: "TaskId",
                principalTable: "TaskEmployees",
                principalColumn: "Id");
        }
    }
}
