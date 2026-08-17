using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlatformTemplate.ServiceName.Domain.Models;

namespace PlatformTemplate.ServiceName.Infrastructure.DataAccess.Configurations;

internal sealed class ServiceNameItemConfiguration : IEntityTypeConfiguration<ServiceNameItem>
{
    public void Configure(EntityTypeBuilder<ServiceNameItem> builder)
    {
        builder.ToTable("items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Name)
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(item => item.CreatedAt).IsRequired();
    }
}
