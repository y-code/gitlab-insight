using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouTrackInsight.Migrations.YTInsight
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "yt_issue",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    projectid = table.Column<string>(name: "project_id", type: "text", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_yt_issue", x => new { x.id, x.projectid });
                });

            migrationBuilder.CreateTable(
                name: "yt_issue_link",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: true),
                    target = table.Column<string>(type: "text", nullable: true),
                    YTIssueModelId = table.Column<string>(type: "text", nullable: true),
                    YTIssueModelProjectId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_yt_issue_link", x => x.id);
                    table.ForeignKey(
                        name: "FK_yt_issue_link_yt_issue_YTIssueModelId_YTIssueModelProjectId",
                        columns: x => new { x.YTIssueModelId, x.YTIssueModelProjectId },
                        principalTable: "yt_issue",
                        principalColumns: new[] { "id", "project_id" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_yt_issue_link_YTIssueModelId_YTIssueModelProjectId",
                table: "yt_issue_link",
                columns: new[] { "YTIssueModelId", "YTIssueModelProjectId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "yt_issue_link");

            migrationBuilder.DropTable(
                name: "yt_issue");
        }
    }
}
