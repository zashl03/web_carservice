using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_service.Migrations
{
    /// <inheritdoc />
    public partial class addWorkOrderDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_RecordId",
                table: "WorkOrders");

            migrationBuilder.CreateTable(
                name: "WorkTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MeasureUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FactCost = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    MechanicId = table.Column<string>(type: "text", nullable: false),
                    WorkId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTasks_EmployeeProfiles_MechanicId",
                        column: x => x.MechanicId,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkTasks_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkTasks_Works_WorkId",
                        column: x => x.WorkId,
                        principalTable: "Works",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartInWorks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorekeeperId = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartInWorks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartInWorks_EmployeeProfiles_StorekeeperId",
                        column: x => x.StorekeeperId,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartInWorks_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartInWorks_WorkTasks_WorkTaskId",
                        column: x => x.WorkTaskId,
                        principalTable: "WorkTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_RecordId",
                table: "WorkOrders",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartInWorks_PartId",
                table: "PartInWorks",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInWorks_StorekeeperId",
                table: "PartInWorks",
                column: "StorekeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInWorks_WorkTaskId",
                table: "PartInWorks",
                column: "WorkTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_MechanicId",
                table: "WorkTasks",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_WorkId",
                table: "WorkTasks",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_WorkOrderId",
                table: "WorkTasks",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartInWorks");

            migrationBuilder.DropTable(
                name: "WorkTasks");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_RecordId",
                table: "WorkOrders");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_RecordId",
                table: "WorkOrders",
                column: "RecordId");
        }
    }
}
