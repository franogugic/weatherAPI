using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppUserSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSession",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token_hash = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
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
                name: "IX_UserSession_UserId_ExpiresAt",
                table: "UserSession",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "UQ_UserSession_TokenHash",
                table: "UserSession",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSession");
        }
    }
}
