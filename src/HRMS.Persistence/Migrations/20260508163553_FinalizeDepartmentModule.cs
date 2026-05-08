using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeDepartmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_HeadEmployeeId",
                table: "Departments");

            migrationBuilder.RenameColumn(
                name: "HeadEmployeeId",
                table: "Departments",
                newName: "ParentDepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Departments_HeadEmployeeId",
                table: "Departments",
                newName: "IX_Departments_ParentDepartmentId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Departments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Departments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentHeadId",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code_TenantId",
                table: "Departments",
                columns: new[] { "Code", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentHeadId",
                table: "Departments",
                column: "DepartmentHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name_TenantId",
                table: "Departments",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_DepartmentHeadId",
                table: "Departments",
                column: "DepartmentHeadId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Departments_ParentDepartmentId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_DepartmentHeadId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Code_TenantId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DepartmentHeadId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Name_TenantId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DepartmentHeadId",
                table: "Departments");

            migrationBuilder.RenameColumn(
                name: "ParentDepartmentId",
                table: "Departments",
                newName: "HeadEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                newName: "IX_Departments_HeadEmployeeId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Departments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_HeadEmployeeId",
                table: "Departments",
                column: "HeadEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
