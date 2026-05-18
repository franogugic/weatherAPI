using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherAPI.Infrastructure.Migrations.UserDb
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppUser",
                columns: new[] { "id", "created_at", "email", "first_name", "last_name", "password_hash", "role", "updated_at" },
                values: new object[] { 1, new DateTime(2026, 5, 18, 0, 0, 0, 0, DateTimeKind.Utc), "admin@admin.com", "Admin", "Admin", "$2y$10$DbMeboK6y32CFyXkf5dIx.PWWaBvqvWScrHHPtj9BG1Ur.cB3k85W", "Admin", new DateTime(2026, 5, 18, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppUser",
                keyColumn: "id",
                keyValue: 1);
        }
    }
}
