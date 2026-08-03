using EventFlow.Bookings.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Bookings.Infrastructure.DataAccess.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.Property(message => message.Topic).IsRequired().HasMaxLength(200);
        builder.Property(message => message.MessageKey).IsRequired().HasMaxLength(200);
        builder.Property(message => message.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(message => message.OccurredAt).IsRequired();
        builder.Property(message => message.PublishedAt);
        builder.Property(message => message.Attempts).IsRequired();
        builder.Property(message => message.LastError).HasMaxLength(2000);

        builder.HasIndex(message => new
        {
            message.PublishedAt,
            message.OccurredAt
        });
    }
}
