using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Persistence.Configurations;

public class NotificationConfiguration : 
    IEntityTypeConfiguration<Notification>,
    IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.RelatedEntityType).HasMaxLength(100);
        builder.Property(x => x.RelatedEntityId).HasMaxLength(100);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.IsRead });
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.TenantId);
    }

    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<NotificationPreferences>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.TenantId);
    }
}
