using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Migrations
{
    /// <inheritdoc />
    public partial class FixTimeLogUserIdForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove the incorrect UserId1 foreign key and index
            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId1",
                table: "TimeLogs");

            migrationBuilder.DropIndex(
                name: "IX_TimeLogs_UserId1",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "TimeLogs");

            // Create the correct foreign key using UserId
            migrationBuilder.CreateIndex(
                name: "IX_TimeLogs_UserId",
                table: "TimeLogs",
                column: "UserId");

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
            // Revert back to UserId1 (if needed for rollback)
            migrationBuilder.DropForeignKey(
                name: "FK_TimeLogs_AspNetUsers_UserId",
                table: "TimeLogs");

            migrationBuilder.DropIndex(
                name: "IX_TimeLogs_UserId",
                table: "TimeLogs");

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
                name: "FK_TimeLogs_AspNetUsers_UserId1",
                table: "TimeLogs",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
