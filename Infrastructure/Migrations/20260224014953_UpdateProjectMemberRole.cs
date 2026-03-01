using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncVerse.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectMemberRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "ProjectMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "ProjectInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ProjectInvitations");
        }
    }
}
