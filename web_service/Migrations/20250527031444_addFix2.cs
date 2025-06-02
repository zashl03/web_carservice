using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_service.Migrations
{
    /// <inheritdoc />
    public partial class addFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkTasks_EmployeeProfiles_MechanicId",
                table: "WorkTasks");

            migrationBuilder.AlterColumn<string>(
                name: "MechanicId",
                table: "WorkTasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTasks_EmployeeProfiles_MechanicId",
                table: "WorkTasks",
                column: "MechanicId",
                principalTable: "EmployeeProfiles",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkTasks_EmployeeProfiles_MechanicId",
                table: "WorkTasks");

            migrationBuilder.AlterColumn<string>(
                name: "MechanicId",
                table: "WorkTasks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTasks_EmployeeProfiles_MechanicId",
                table: "WorkTasks",
                column: "MechanicId",
                principalTable: "EmployeeProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
