using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouTrackInsight.Entity.Migrations
{
    /// <inheritdoc />
    public partial class addsubmittedtimestamptoimporttask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "submitted",
                table: "issue_import_task",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "submitted",
                table: "issue_import_task");
        }
    }
}
