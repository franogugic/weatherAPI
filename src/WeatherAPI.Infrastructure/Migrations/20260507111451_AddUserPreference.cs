using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPreference",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    temperature_unit = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    wind_speed_unit = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    pressure_unit = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    cloudiness_unit = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    precipitation_unit = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreference", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_UserPreference_AppUser_UserId",
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
                name: "UserPreference");
        }
    }
}
