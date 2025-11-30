using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_CrmData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrmData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrmId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AnalysisMethod = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ElementValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsOurOreas = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrmData_AnalysisMethod",
                table: "CrmData",
                column: "AnalysisMethod");

            migrationBuilder.CreateIndex(
                name: "IX_CrmData_CrmId",
                table: "CrmData",
                column: "CrmId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmData_CrmId_AnalysisMethod",
                table: "CrmData",
                columns: new[] { "CrmId", "AnalysisMethod" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmData_IsOurOreas",
                table: "CrmData",
                column: "IsOurOreas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrmData");
        }
    }
}
