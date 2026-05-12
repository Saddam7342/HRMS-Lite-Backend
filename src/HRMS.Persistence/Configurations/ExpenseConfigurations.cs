using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Persistence.Configurations;

public class ExpenseConfiguration : 
    IEntityTypeConfiguration<ExpenseCategory>,
    IEntityTypeConfiguration<ExpenseClaim>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique();
    }

    public void Configure(EntityTypeBuilder<ExpenseClaim> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.ReceiptFileUrl).HasMaxLength(1000);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.ExpenseClaims)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.ExpenseClaims)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedBy)
            .WithMany()
            .HasForeignKey(x => x.ApprovedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.EmployeeId, x.Status });
        builder.HasIndex(x => x.ExpenseDate);
    }
}
