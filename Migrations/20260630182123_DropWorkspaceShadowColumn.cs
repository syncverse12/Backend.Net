using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncVerse.Migrations
{
    /// <inheritdoc />
    public partial class DropWorkspaceShadowColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Workspaces_WorkspaceId1",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_WorkspaceId1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkspaceId1",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkspaceId1",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_WorkspaceId1",
                table: "AspNetUsers",
                column: "WorkspaceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Workspaces_WorkspaceId1",
                table: "AspNetUsers",
                column: "WorkspaceId1",
                principalTable: "Workspaces",
                principalColumn: "Id");
        }
    }
}
