using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Persistence.Configurations;

public class TravelRequestConfiguration : IEntityTypeConfiguration<TravelRequest>
{
    public void Configure(EntityTypeBuilder<TravelRequest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Destination).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Purpose).IsRequired().HasMaxLength(500);
        builder.Property(x => x.EstimatedBudget).HasPrecision(18, 2);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.TravelRequests)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedBy)
            .WithMany()
            .HasForeignKey(x => x.ApprovedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.EmployeeId, x.Status });
        builder.HasIndex(x => new { x.FromDate, x.ToDate });
        builder.HasIndex(x => x.TenantId);
    }
}
