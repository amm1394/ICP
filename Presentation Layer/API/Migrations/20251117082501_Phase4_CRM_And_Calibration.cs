using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_CRM_And_Calibration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalibrationCurves_Elements_ElementId",
                table: "CalibrationCurves");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMValues_Elements_ElementId",
                table: "CRMValues");

            migrationBuilder.DropIndex(
                name: "IX_CRMValue_CRM_Element",
                table: "CRMValues");

            migrationBuilder.DropIndex(
                name: "IX_CRM_IsActive",
                table: "CRMs");

            migrationBuilder.DropIndex(
                name: "IX_CalibrationPoint_Curve_Order",
                table: "CalibrationPoints");

            migrationBuilder.DropIndex(
                name: "IX_CalibrationCurve_Date",
                table: "CalibrationCurves");

            migrationBuilder.DropColumn(
                name: "PointOrder",
                table: "CalibrationPoints");

            migrationBuilder.DropColumn(
                name: "CalibrationDate",
                table: "CalibrationCurves");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CalibrationCurves");

            migrationBuilder.RenameIndex(
                name: "IX_CRMValue_ElementId",
                table: "CRMValues",
                newName: "IX_CRMValues_ElementId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMValue_CRMId",
                table: "CRMValues",
                newName: "IX_CRMValues_CRMId");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "CRMs",
                newName: "Description");

            migrationBuilder.RenameIndex(
                name: "IX_CRM_CRMId",
                table: "CRMs",
                newName: "IX_CRMs_CRMId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationPoint_CurveId",
                table: "CalibrationPoints",
                newName: "IX_CalibrationPoints_CalibrationCurveId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationCurve_ProjectId",
                table: "CalibrationCurves",
                newName: "IX_CalibrationCurves_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationCurve_ElementId",
                table: "CalibrationCurves",
                newName: "IX_CalibrationCurves_ElementId");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "CRMValues",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "CRMValues",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "CRMValues",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CRMValues",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxAcceptable",
                table: "CRMValues",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinAcceptable",
                table: "CRMValues",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "CRMs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CRMs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Manufacturer",
                table: "CRMs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LotNumber",
                table: "CRMs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CRMs",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "CRMs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CRMId",
                table: "CRMs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Matrix",
                table: "CRMs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "CalibrationPoints",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "CalibrationPoints",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsedInFit",
                table: "CalibrationPoints",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "CalibrationPoints",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "CalibrationPoints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PointType",
                table: "CalibrationPoints",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "CalibrationCurves",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "CalibrationCurves",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Degree",
                table: "CalibrationCurves",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "FitType",
                table: "CalibrationCurves",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CalibrationCurves",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "SettingsJson",
                table: "CalibrationCurves",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CalibrationCurves_Elements_ElementId",
                table: "CalibrationCurves",
                column: "ElementId",
                principalTable: "Elements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMValues_Elements_ElementId",
                table: "CRMValues",
                column: "ElementId",
                principalTable: "Elements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalibrationCurves_Elements_ElementId",
                table: "CalibrationCurves");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMValues_Elements_ElementId",
                table: "CRMValues");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CRMValues");

            migrationBuilder.DropColumn(
                name: "MaxAcceptable",
                table: "CRMValues");

            migrationBuilder.DropColumn(
                name: "MinAcceptable",
                table: "CRMValues");

            migrationBuilder.DropColumn(
                name: "Matrix",
                table: "CRMs");

            migrationBuilder.DropColumn(
                name: "IsUsedInFit",
                table: "CalibrationPoints");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "CalibrationPoints");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "CalibrationPoints");

            migrationBuilder.DropColumn(
                name: "PointType",
                table: "CalibrationPoints");

            migrationBuilder.DropColumn(
                name: "Degree",
                table: "CalibrationCurves");

            migrationBuilder.DropColumn(
                name: "FitType",
                table: "CalibrationCurves");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CalibrationCurves");

            migrationBuilder.DropColumn(
                name: "SettingsJson",
                table: "CalibrationCurves");

            migrationBuilder.RenameIndex(
                name: "IX_CRMValues_ElementId",
                table: "CRMValues",
                newName: "IX_CRMValue_ElementId");

            migrationBuilder.RenameIndex(
                name: "IX_CRMValues_CRMId",
                table: "CRMValues",
                newName: "IX_CRMValue_CRMId");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "CRMs",
                newName: "Notes");

            migrationBuilder.RenameIndex(
                name: "IX_CRMs_CRMId",
                table: "CRMs",
                newName: "IX_CRM_CRMId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationPoints_CalibrationCurveId",
                table: "CalibrationPoints",
                newName: "IX_CalibrationPoint_CurveId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationCurves_ProjectId",
                table: "CalibrationCurves",
                newName: "IX_CalibrationCurve_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationCurves_ElementId",
                table: "CalibrationCurves",
                newName: "IX_CalibrationCurve_ElementId");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "CRMValues",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "CRMValues",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "CRMValues",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "CRMs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CRMs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Manufacturer",
                table: "CRMs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LotNumber",
                table: "CRMs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CRMs",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "CRMs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CRMId",
                table: "CRMs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "CalibrationPoints",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "CalibrationPoints",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PointOrder",
                table: "CalibrationPoints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "CalibrationCurves",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "CalibrationCurves",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CalibrationDate",
                table: "CalibrationCurves",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CalibrationCurves",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CRMValue_CRM_Element",
                table: "CRMValues",
                columns: new[] { "CRMId", "ElementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CRM_IsActive",
                table: "CRMs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationPoint_Curve_Order",
                table: "CalibrationPoints",
                columns: new[] { "CalibrationCurveId", "PointOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationCurve_Date",
                table: "CalibrationCurves",
                column: "CalibrationDate");

            migrationBuilder.AddForeignKey(
                name: "FK_CalibrationCurves_Elements_ElementId",
                table: "CalibrationCurves",
                column: "ElementId",
                principalTable: "Elements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMValues_Elements_ElementId",
                table: "CRMValues",
                column: "ElementId",
                principalTable: "Elements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
