using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_service.Migrations
{
    /// <inheritdoc />
    public partial class RecreateSchemaWithNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_EmployeeProfiles_StorekeeperId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_StorekeeperId",
                table: "Warehouses");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "Records",
                newName: "RejectReason");

            migrationBuilder.RenameColumn(
                name: "BookingDate",
                table: "Records",
                newName: "DateAppointment");

            migrationBuilder.AddColumn<string>(
                name: "AdministratorId",
                table: "Records",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientComment",
                table: "Records",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Records",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TypeServiceId",
                table: "Records",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Cars",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicencePlate",
                table: "Cars",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Mileage",
                table: "Cars",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Cars",
                type: "integer",
                maxLength: 4,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_StorekeeperId",
                table: "Warehouses",
                column: "StorekeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_Records_AdministratorId",
                table: "Records",
                column: "AdministratorId");

            migrationBuilder.CreateIndex(
                name: "IX_Records_TypeServiceId",
                table: "Records",
                column: "TypeServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Records_EmployeeProfiles_AdministratorId",
                table: "Records",
                column: "AdministratorId",
                principalTable: "EmployeeProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Records_TypeServices_TypeServiceId",
                table: "Records",
                column: "TypeServiceId",
                principalTable: "TypeServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_EmployeeProfiles_StorekeeperId",
                table: "Warehouses",
                column: "StorekeeperId",
                principalTable: "EmployeeProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Records_EmployeeProfiles_AdministratorId",
                table: "Records");

            migrationBuilder.DropForeignKey(
                name: "FK_Records_TypeServices_TypeServiceId",
                table: "Records");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_EmployeeProfiles_StorekeeperId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_StorekeeperId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Records_AdministratorId",
                table: "Records");

            migrationBuilder.DropIndex(
                name: "IX_Records_TypeServiceId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "AdministratorId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "ClientComment",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "TypeServiceId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "LicencePlate",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Mileage",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Cars");

            migrationBuilder.RenameColumn(
                name: "RejectReason",
                table: "Records",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "DateAppointment",
                table: "Records",
                newName: "BookingDate");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_StorekeeperId",
                table: "Warehouses",
                column: "StorekeeperId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_EmployeeProfiles_StorekeeperId",
                table: "Warehouses",
                column: "StorekeeperId",
                principalTable: "EmployeeProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
