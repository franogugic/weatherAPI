using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherAPI.Infrastructure.Migrations.UserDb
{
    /// <inheritdoc />
    public partial class AddUserDashboardLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDashboardLayout",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    layout_json = table.Column<string>(type: "jsonb", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDashboardLayout", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_UserDashboardLayout_AppUser_UserId",
                        column: x => x.user_id,
                        principalTable: "AppUser",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDashboardLayout");
        }
    }
}
