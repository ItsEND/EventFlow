using EventFlow.Events.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Events.Infrastructure.DataAccess.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        
        builder.Property(user => user.Login).IsRequired().HasMaxLength(100);
        builder.HasIndex(user => user.Login).IsUnique();

        builder.Property(user => user.PasswordHash).IsRequired().HasMaxLength(64);

        builder.Property(user => user.Role).HasConversion<string>().IsRequired();

    }
}
