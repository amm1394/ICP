using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Samples_SampleId",
                table: "Samples");

            migrationBuilder.CreateIndex(
                name: "IX_Samples_SampleId",
                table: "Samples",
                column: "SampleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Samples_SampleId",
                table: "Samples");

            migrationBuilder.CreateIndex(
                name: "IX_Samples_SampleId",
                table: "Samples",
                column: "SampleId",
                unique: true);
        }
    }
}
