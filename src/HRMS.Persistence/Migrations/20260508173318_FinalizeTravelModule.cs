using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeTravelModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TravelRequests_Employees_EmployeeId",
                table: "TravelRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TravelRequests_Employees_ReviewedById",
                table: "TravelRequests");

            migrationBuilder.DropIndex(
                name: "IX_TravelRequests_EmployeeId",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "ModeOfTravel",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "ReviewerComment",
                table: "TravelRequests");

            migrationBuilder.RenameColumn(
                name: "ReviewedById",
                table: "TravelRequests",
                newName: "ApprovedById");

            migrationBuilder.RenameColumn(
                name: "ReviewedAt",
                table: "TravelRequests",
                newName: "ApprovedAt");

            migrationBuilder.RenameColumn(
                name: "ReturnDate",
                table: "TravelRequests",
                newName: "ToDate");

            migrationBuilder.RenameColumn(
                name: "DepartureDate",
                table: "TravelRequests",
                newName: "FromDate");

            migrationBuilder.RenameIndex(
                name: "IX_TravelRequests_ReviewedById",
                table: "TravelRequests",
                newName: "IX_TravelRequests_ApprovedById");

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "TravelRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Destination",
                table: "TravelRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedBudget",
                table: "TravelRequests",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "TravelRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TravelRequests_EmployeeId_Status",
                table: "TravelRequests",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TravelRequests_FromDate_ToDate",
                table: "TravelRequests",
                columns: new[] { "FromDate", "ToDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TravelRequests_TenantId",
                table: "TravelRequests",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_TravelRequests_Employees_ApprovedById",
                table: "TravelRequests",
                column: "ApprovedById",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TravelRequests_Employees_EmployeeId",
                table: "TravelRequests",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TravelRequests_Employees_ApprovedById",
                table: "TravelRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TravelRequests_Employees_EmployeeId",
                table: "TravelRequests");

            migrationBuilder.DropIndex(
                name: "IX_TravelRequests_EmployeeId_Status",
                table: "TravelRequests");

            migrationBuilder.DropIndex(
                name: "IX_TravelRequests_FromDate_ToDate",
                table: "TravelRequests");

            migrationBuilder.DropIndex(
                name: "IX_TravelRequests_TenantId",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "EstimatedBudget",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "TravelRequests");

            migrationBuilder.RenameColumn(
                name: "ToDate",
                table: "TravelRequests",
                newName: "ReturnDate");

            migrationBuilder.RenameColumn(
                name: "FromDate",
                table: "TravelRequests",
                newName: "DepartureDate");

            migrationBuilder.RenameColumn(
                name: "ApprovedById",
                table: "TravelRequests",
                newName: "ReviewedById");

            migrationBuilder.RenameColumn(
                name: "ApprovedAt",
                table: "TravelRequests",
                newName: "ReviewedAt");

            migrationBuilder.RenameIndex(
                name: "IX_TravelRequests_ApprovedById",
                table: "TravelRequests",
                newName: "IX_TravelRequests_ReviewedById");

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "TravelRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Destination",
                table: "TravelRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "TravelRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "TravelRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModeOfTravel",
                table: "TravelRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TravelRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerComment",
                table: "TravelRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TravelRequests_EmployeeId",
                table: "TravelRequests",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TravelRequests_Employees_EmployeeId",
                table: "TravelRequests",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TravelRequests_Employees_ReviewedById",
                table: "TravelRequests",
                column: "ReviewedById",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
