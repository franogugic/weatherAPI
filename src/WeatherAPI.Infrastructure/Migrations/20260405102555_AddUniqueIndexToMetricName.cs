using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToMetricName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UQ_Metric_Name",
                table: "Metric",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Metric_Name",
                table: "Metric");
        }
    }
}
