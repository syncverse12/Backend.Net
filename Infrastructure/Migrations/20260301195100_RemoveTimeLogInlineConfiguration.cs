using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncVerse.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTimeLogInlineConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId",
                table: "TimeLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId1",
                table: "TimeLogs");

            migrationBuilder.DropIndex(
                name: "IX_TimeLogs_UserId1",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "TimeLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "TimeLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId",
                table: "TimeLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId",
                table: "TimeLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "TimeLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "TimeLogs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeLogs_UserId1",
                table: "TimeLogs",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId",
                table: "TimeLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId1",
                table: "TimeLogs",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
