using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_service.Migrations
{
    /// <inheritdoc />
    public partial class CarParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NumberPlace",
                table: "StorageLocations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Parts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CategoryParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryParts_CategoryParts_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CategoryParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_CategoryId",
                table: "Parts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryParts_CategoryName_ParentId",
                table: "CategoryParts",
                columns: new[] { "CategoryName", "ParentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryParts_ParentId",
                table: "CategoryParts",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_CategoryParts_CategoryId",
                table: "Parts",
                column: "CategoryId",
                principalTable: "CategoryParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_CategoryParts_CategoryId",
                table: "Parts");

            migrationBuilder.DropTable(
                name: "CategoryParts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_CategoryId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Parts");

            migrationBuilder.AlterColumn<string>(
                name: "NumberPlace",
                table: "StorageLocations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
