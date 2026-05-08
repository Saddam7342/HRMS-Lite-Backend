using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeExpenseModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseClaims_Employees_EmployeeId",
                table: "ExpenseClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseClaims_Employees_ReviewedById",
                table: "ExpenseClaims");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseClaims_EmployeeId",
                table: "ExpenseClaims");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ExpenseClaims");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "ExpenseClaims");

            migrationBuilder.DropColumn(
                name: "ReceiptUrl",
                table: "ExpenseClaims");

            migrationBuilder.DropColumn(
                name: "ReviewerComment",
                table: "ExpenseClaims");

            migrationBuilder.RenameColumn(
                name: "ReviewedById",
                table: "ExpenseClaims",
                newName: "ApprovedById");

            migrationBuilder.RenameColumn(
                name: "ReviewedAt",
                table: "ExpenseClaims",
                newName: "SubmittedAt");

            migrationBuilder.RenameColumn(
                name: "ClaimDate",
                table: "ExpenseClaims",
                newName: "ExpenseDate");

            migrationBuilder.RenameIndex(
                name: "IX_ExpenseClaims_ReviewedById",
                table: "ExpenseClaims",
                newName: "IX_ExpenseClaims_ApprovedById");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ExpenseClaims",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ExpenseClaims",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "ExpenseClaims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "ExpenseClaims",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ReceiptFileUrl",
                table: "ExpenseClaims",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ExpenseClaims",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseClaims_CategoryId",
                table: "ExpenseClaims",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseClaims_EmployeeId_Status",
                table: "ExpenseClaims",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseClaims_ExpenseDate",
                table: "ExpenseClaims",
                column: "ExpenseDate");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseClaims_TenantId",
                table: "ExpenseClaims",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_Code_TenantId",
                table: "ExpenseCategories",
                columns: new[] { "Code", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_TenantId",
                table: "ExpenseCategories",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseClaims_Employees_ApprovedById",
                table: "ExpenseClaims",
                column: "ApprovedById",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseClaims_Employees_EmployeeId",
                table: "ExpenseClaims",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseClaims_ExpenseCategories_CategoryId",
                table: "ExpenseClaims",
                column: "CategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseClaims_Employees_ApprovedById",
                table: "ExpenseClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseClaims_Employees_EmployeeId",
                table: "ExpenseClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseClaims_ExpenseCategories_CategoryId",
                table: "ExpenseClaims");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseClaims_CategoryId",
                table: "ExpenseClaims");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseClaims_EmployeeId_Status",
                table: "ExpenseClaims");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseClaims_ExpenseDate",
                table: "ExpenseClaims");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseClaims_TenantId",
                table: "ExpenseClaims");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "ExpenseClaims");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "ExpenseClaims");

            migrationBuilder.DropColumn(
                name: "ReceiptFileUrl",
                table: "ExpenseClaims");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ExpenseClaims");

            migrationBuilder.RenameColumn(
                name: "SubmittedAt",
                table: "ExpenseClaims",
                newName: "ReviewedAt");

            migrationBuilder.RenameColumn(
                name: "ExpenseDate",
                table: "ExpenseClaims",
                newName: "ClaimDate");

            migrationBuilder.RenameColumn(
                name: "ApprovedById",
                table: "ExpenseClaims",
                newName: "ReviewedById");

            migrationBuilder.RenameIndex(
                name: "IX_ExpenseClaims_ApprovedById",
                table: "ExpenseClaims",
                newName: "IX_ExpenseClaims_ReviewedById");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ExpenseClaims",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ExpenseClaims",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ExpenseClaims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "ExpenseClaims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptUrl",
                table: "ExpenseClaims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerComment",
                table: "ExpenseClaims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseClaims_EmployeeId",
                table: "ExpenseClaims",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseClaims_Employees_EmployeeId",
                table: "ExpenseClaims",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseClaims_Employees_ReviewedById",
                table: "ExpenseClaims",
                column: "ReviewedById",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
