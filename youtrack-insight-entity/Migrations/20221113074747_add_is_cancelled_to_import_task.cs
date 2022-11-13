using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouTrackInsight.Entity.Migrations
{
    /// <inheritdoc />
    public partial class addiscancelledtoimporttask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_cancelled",
                table: "issue_import_task",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_cancelled",
                table: "issue_import_task");
        }
    }
}
