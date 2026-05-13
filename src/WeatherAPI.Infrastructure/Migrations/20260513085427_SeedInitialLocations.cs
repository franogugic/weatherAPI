using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WeatherAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Location",
                columns: new[] { "id", "altitude", "latitude", "longitude", "name" },
                values: new object[,]
                {
                    { (short)1, (short)122, 45.818611m, 16.016389m, "Maksimir stadion, Zagreb" },
                    { (short)2, (short)272, 43.366700m, 17.623300m, "Dubrava, Siroki Brijeg" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Location",
                keyColumn: "id",
                keyValue: (short)1);

            migrationBuilder.DeleteData(
                table: "Location",
                keyColumn: "id",
                keyValue: (short)2);
        }
    }
}
