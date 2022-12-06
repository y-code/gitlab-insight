using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bakhoo.Entity.Test.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Bakhoo");

            migrationBuilder.CreateTable(
                name: "job",
                schema: "Bakhoo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    submitted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    iscancelling = table.Column<bool>(name: "is_cancelling", type: "boolean", nullable: false),
                    cancelrequested = table.Column<DateTimeOffset>(name: "cancel_requested", type: "timestamp with time zone", nullable: true),
                    iscancelled = table.Column<bool>(name: "is_cancelled", type: "boolean", nullable: false),
                    haserror = table.Column<bool>(name: "has_error", type: "boolean", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job",
                schema: "Bakhoo");
        }
    }
}
