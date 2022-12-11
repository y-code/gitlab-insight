using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouTrackInsight.Migrations.YTInsight
{
    /// <inheritdoc />
    public partial class addrowversiontoissuetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "yt_issue",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "yt_issue");
        }
    }
}
