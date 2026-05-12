using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EmployeeCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.Designation).HasMaxLength(100);
        builder.Property(x => x.ProfileImageUrl).HasMaxLength(500);

        builder.HasIndex(x => x.EmployeeCode).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();

        // One-to-one with AppUser
        builder.HasOne(x => x.User)
            .WithOne(x => x.Employee)
            .HasForeignKey<Employee>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing relationship for Manager
        builder.HasOne(x => x.Manager)
            .WithMany(x => x.DirectReports)
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Department relationship
        builder.HasOne(x => x.Department)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
