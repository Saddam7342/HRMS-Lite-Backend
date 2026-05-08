using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Persistence.Configurations;

public class SalaryStructureConfiguration : IEntityTypeConfiguration<SalaryStructure>
{
    public void Configure(EntityTypeBuilder<SalaryStructure> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.BasicSalary).HasPrecision(18, 2);
        builder.Property(x => x.OvertimeRatePerHour).HasPrecision(18, 2);
        builder.Property(x => x.Allowances).IsRequired();
        builder.Property(x => x.Deductions).IsRequired();

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
    }
}

public class PayrollConfiguration : IEntityTypeConfiguration<Payroll>
{
    public void Configure(EntityTypeBuilder<Payroll> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BasicSalary).HasPrecision(18, 2);
        builder.Property(x => x.TotalAllowances).HasPrecision(18, 2);
        builder.Property(x => x.TotalDeductions).HasPrecision(18, 2);
        builder.Property(x => x.NetSalary).HasPrecision(18, 2);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedBy)
            .WithMany()
            .HasForeignKey(x => x.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.Month, x.Year }).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
