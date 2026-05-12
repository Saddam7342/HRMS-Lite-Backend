using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Persistence.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.TotalHours).HasPrecision(18, 2);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.AttendanceRecords)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // One record per employee per day
        builder.HasIndex(x => new { x.EmployeeId, x.Date }).IsUnique();
        
        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.Status);
    }
}
