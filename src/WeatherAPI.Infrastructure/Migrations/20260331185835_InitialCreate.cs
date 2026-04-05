using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    latitude = table.Column<decimal>(type: "decimal(8,6)", precision: 8, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    altitude = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Metric",
                columns: table => new
                {
                    id = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metric", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Unit",
                columns: table => new
                {
                    id = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    value = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    desc = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unit", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherSymbol",
                columns: table => new
                {
                    id = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherSymbol", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ForecastFetch",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    location_id = table.Column<short>(type: "smallint", nullable: false),
                    response_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    fetched_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastFetch", x => x.id);
                    table.ForeignKey(
                        name: "FK_ForecastFetch_Location",
                        column: x => x.location_id,
                        principalTable: "Location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FetchLog",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    forecast_fetch_id = table.Column<int>(type: "int", nullable: false),
                    status_code = table.Column<short>(type: "smallint", nullable: false),
                    error_message = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FetchLog", x => x.id);
                    table.ForeignKey(
                        name: "FK_FetchLog_ForecastFetch",
                        column: x => x.forecast_fetch_id,
                        principalTable: "ForecastFetch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForecastFetchUnit",
                columns: table => new
                {
                    forecast_fetch_id = table.Column<int>(type: "int", nullable: false),
                    metric_id = table.Column<byte>(type: "tinyint", nullable: false),
                    unit_id = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastFetchUnit", x => new { x.forecast_fetch_id, x.metric_id });
                    table.ForeignKey(
                        name: "FK_ForecastFetchUnit_Fetch",
                        column: x => x.forecast_fetch_id,
                        principalTable: "ForecastFetch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ForecastFetchUnit_Metric",
                        column: x => x.metric_id,
                        principalTable: "Metric",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ForecastFetchUnit_Unit",
                        column: x => x.unit_id,
                        principalTable: "Unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HourlyForecast",
                columns: table => new
                {
                    forecast_fetch_id = table.Column<int>(type: "int", nullable: false),
                    forecast_time = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    air_temperature = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    air_pressure_at_sea_level = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: true),
                    cloudiness = table.Column<byte>(type: "tinyint", nullable: true),
                    humidity = table.Column<byte>(type: "tinyint", nullable: true),
                    wind_direction = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    wind_speed = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    weather_symbol_id = table.Column<byte>(type: "tinyint", nullable: true),
                    precipitation_amount = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlyForecast", x => new { x.forecast_fetch_id, x.forecast_time });
                    table.ForeignKey(
                        name: "FK_HourlyForecast_Fetch",
                        column: x => x.forecast_fetch_id,
                        principalTable: "ForecastFetch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HourlyForecast_Symbol",
                        column: x => x.weather_symbol_id,
                        principalTable: "WeatherSymbol",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_FetchLog_ForecastFetch",
                table: "FetchLog",
                column: "forecast_fetch_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForecastFetch_location_id",
                table: "ForecastFetch",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastFetchUnit_metric_id",
                table: "ForecastFetchUnit",
                column: "metric_id");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastFetchUnit_unit_id",
                table: "ForecastFetchUnit",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_HourlyForecast_weather_symbol_id",
                table: "HourlyForecast",
                column: "weather_symbol_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Unit_Value",
                table: "Unit",
                column: "value",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_WeatherSymbol_Code",
                table: "WeatherSymbol",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FetchLog");

            migrationBuilder.DropTable(
                name: "ForecastFetchUnit");

            migrationBuilder.DropTable(
                name: "HourlyForecast");

            migrationBuilder.DropTable(
                name: "Metric");

            migrationBuilder.DropTable(
                name: "Unit");

            migrationBuilder.DropTable(
                name: "ForecastFetch");

            migrationBuilder.DropTable(
                name: "WeatherSymbol");

            migrationBuilder.DropTable(
                name: "Location");
        }
    }
}
