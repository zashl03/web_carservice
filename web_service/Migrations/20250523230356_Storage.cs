using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_service.Migrations
{
    /// <inheritdoc />
    public partial class Storage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Warehouses_WarehouseId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PartNumber",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_WarehouseId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Parts");

            migrationBuilder.RenameColumn(
                name: "PartNumber",
                table: "Parts",
                newName: "ServicePn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Parts",
                newName: "PartName");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Parts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerPn",
                table: "Parts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OEMNumber",
                table: "Parts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Parts",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "StorageLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumberPlace = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Room = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Rack = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Shelf = table.Column<int>(type: "integer", nullable: false),
                    Cell = table.Column<int>(type: "integer", nullable: false),
                    StorekeeperId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageLocations_EmployeeProfiles_StorekeeperId",
                        column: x => x.StorekeeperId,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartInStorages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasureUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartInStorages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartInStorages_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartInStorages_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartInStorages_PartId_StorageLocationId",
                table: "PartInStorages",
                columns: new[] { "PartId", "StorageLocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartInStorages_StorageLocationId",
                table: "PartInStorages",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_NumberPlace",
                table: "StorageLocations",
                column: "NumberPlace",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_StorekeeperId",
                table: "StorageLocations",
                column: "StorekeeperId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartInStorages");

            migrationBuilder.DropTable(
                name: "StorageLocations");

            migrationBuilder.DropColumn(
                name: "ManufacturerPn",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "OEMNumber",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Parts");

            migrationBuilder.RenameColumn(
                name: "ServicePn",
                table: "Parts",
                newName: "PartNumber");

            migrationBuilder.RenameColumn(
                name: "PartName",
                table: "Parts",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Parts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "Parts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Parts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "Parts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartNumber",
                table: "Parts",
                column: "PartNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parts_WarehouseId",
                table: "Parts",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Warehouses_WarehouseId",
                table: "Parts",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
