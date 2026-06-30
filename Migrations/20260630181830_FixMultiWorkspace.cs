using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncVerse.Migrations
{
    /// <inheritdoc />
    public partial class FixMultiWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Workspaces_WorkspaceId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "AspNetUsers",
                newName: "WorkspaceId1");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_WorkspaceId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_WorkspaceId1");

            migrationBuilder.AddColumn<string>(
                name: "CurrentWorkspaceId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserWorkspace",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkspaceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWorkspace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWorkspace_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWorkspace_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkspace_UserId",
                table: "UserWorkspace",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkspace_WorkspaceId",
                table: "UserWorkspace",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Workspaces_WorkspaceId1",
                table: "AspNetUsers",
                column: "WorkspaceId1",
                principalTable: "Workspaces",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Workspaces_WorkspaceId1",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "UserWorkspace");

            migrationBuilder.DropColumn(
                name: "CurrentWorkspaceId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId1",
                table: "AspNetUsers",
                newName: "WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_WorkspaceId1",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Workspaces_WorkspaceId",
                table: "AspNetUsers",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");
        }
    }
}
