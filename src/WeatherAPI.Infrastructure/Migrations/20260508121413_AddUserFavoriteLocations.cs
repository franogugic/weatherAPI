using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFavoriteLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFavoriteLocation",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    location_id = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteLocation", x => new { x.user_id, x.location_id });
                    table.ForeignKey(
                        name: "FK_UserFavoriteLocation_AppUser_UserId",
                        column: x => x.user_id,
                        principalTable: "AppUser",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteLocation_Location_LocationId",
                        column: x => x.location_id,
                        principalTable: "Location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteLocation_location_id",
                table: "UserFavoriteLocation",
                column: "location_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavoriteLocation");
        }
    }
}
