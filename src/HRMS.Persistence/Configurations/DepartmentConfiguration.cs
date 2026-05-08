using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(500);

        // Unique constraints per tenant
        builder.HasIndex(x => new { x.Name, x.TenantId }).IsUnique();
        builder.HasIndex(x => new { x.Code, x.TenantId }).IsUnique();

        // Hierarchy (Self-reference)
        builder.HasOne(x => x.ParentDepartment)
            .WithMany(x => x.ChildDepartments)
            .HasForeignKey(x => x.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Leadership
        builder.HasOne(x => x.DepartmentHead)
            .WithMany()
            .HasForeignKey(x => x.DepartmentHeadId)
            .OnDelete(DeleteBehavior.SetNull);

        // Tenant relationship
        builder.HasIndex(x => x.TenantId);
    }
}
