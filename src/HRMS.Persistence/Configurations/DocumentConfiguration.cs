using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(x => x.FileType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Category).IsRequired().HasMaxLength(100);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UploadedBy)
            .WithMany()
            .HasForeignKey(x => x.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.DocumentType);
        builder.HasIndex(x => x.Category);
    }
}
