using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Persistence.Configurations;

public class LeaveConfiguration : 
    IEntityTypeConfiguration<LeaveType>,
    IEntityTypeConfiguration<LeaveBalance>,
    IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique();
    }

    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalDays).HasPrecision(18, 2);
        builder.Property(x => x.UsedDays).HasPrecision(18, 2);
        
        builder.HasIndex(x => new { x.EmployeeId, x.LeaveTypeId, x.Year }).IsUnique();

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LeaveType)
            .WithMany(x => x.LeaveBalances)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalDays).HasPrecision(18, 2);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeaveType)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedBy)
            .WithMany()
            .HasForeignKey(x => x.ApprovedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.EmployeeId, x.Status });
        builder.HasIndex(x => new { x.StartDate, x.EndDate });
    }
}
