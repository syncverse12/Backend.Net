using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncVerse.Migrations
{
    /// <inheritdoc />
    public partial class RenameCategoryToTaskCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TaskCategories_CategoryId",
                table: "Tasks");

            migrationBuilder.RenameTable(
                name: "TaskCategories",
                newName: "Category");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Category_CategoryId",
                table: "Tasks",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id");
        }
    }
}
