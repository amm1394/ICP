using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCalibration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalibrationCurves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElementName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slope = table.Column<double>(type: "float", nullable: false),
                    Intercept = table.Column<double>(type: "float", nullable: false),
                    RSquared = table.Column<double>(type: "float", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationCurves", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalibrationPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalibrationCurveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Concentration = table.Column<double>(type: "float", nullable: false),
                    Intensity = table.Column<double>(type: "float", nullable: false),
                    IsExcluded = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalibrationPoints_CalibrationCurves_CalibrationCurveId",
                        column: x => x.CalibrationCurveId,
                        principalTable: "CalibrationCurves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationPoints_CalibrationCurveId",
                table: "CalibrationPoints",
                column: "CalibrationCurveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalibrationPoints");

            migrationBuilder.DropTable(
                name: "CalibrationCurves");
        }
    }
}
