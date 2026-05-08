using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Persistence.Configurations;

public class OrganizationSettingConfiguration : IEntityTypeConfiguration<OrganizationSetting>
{
    public void Configure(EntityTypeBuilder<OrganizationSetting> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Value).IsRequired();
        builder.Property(x => x.DataType).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        builder.HasIndex(x => x.Key);
    }
}
