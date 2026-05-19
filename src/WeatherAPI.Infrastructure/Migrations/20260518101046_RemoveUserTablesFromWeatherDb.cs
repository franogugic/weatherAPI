using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserTablesFromWeatherDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS [UserFavoriteLocation];
                DROP TABLE IF EXISTS [UserPreference];
                DROP TABLE IF EXISTS [UserSession];
                DROP TABLE IF EXISTS [AppUser];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUser",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    first_name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    role = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "User"),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUser", x => x.id);
                });

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

            migrationBuilder.CreateTable(
                name: "UserPreference",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    cloudiness_unit = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    precipitation_unit = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    pressure_unit = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    temperature_unit = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    wind_speed_unit = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "UserSession",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    token_hash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSession", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserSession_AppUser_UserId",
                        column: x => x.user_id,
                        principalTable: "AppUser",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_AppUser_Email",
                table: "AppUser",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteLocation_location_id",
                table: "UserFavoriteLocation",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSession_UserId_ExpiresAt",
                table: "UserSession",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "UQ_UserSession_TokenHash",
                table: "UserSession",
                column: "token_hash",
                unique: true);
        }
    }
}
