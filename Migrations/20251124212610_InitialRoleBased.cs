using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Graduation_Project.Migrations
{
    /// <inheritdoc />
    public partial class InitialRoleBased : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                type: "text",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3f4c2b89-8ad1-4dfe-b61e-e3cbdf9a9d5c", null, "The Manager Role For The User", "Manager", "MANAGER" },
                    { "8e91d7bb-5c44-4c0a-9cd1-2730d1baf6a4", null, "The Admin Role For The User", "Admin", "ADMIN" },
                    { "c4a8f0c1-3be2-4e35-9b7f-2ef45a6cb912", null, "The Employee Role For The User", "Employee", "EMPLOYEE" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3f4c2b89-8ad1-4dfe-b61e-e3cbdf9a9d5c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8e91d7bb-5c44-4c0a-9cd1-2730d1baf6a4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c4a8f0c1-3be2-4e35-9b7f-2ef45a6cb912");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AspNetRoles");
        }
    }
}
