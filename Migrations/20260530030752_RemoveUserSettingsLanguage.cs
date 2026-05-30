using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncVerse.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserSettingsLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "UserSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "UserSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
