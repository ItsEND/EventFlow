using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformTemplate.ServiceName.Infrastructure.Messaging.Outbox;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Topic).HasMaxLength(256).IsRequired();
        builder.Property(message => message.Key).HasMaxLength(256).IsRequired();
        builder.Property(message => message.Payload).IsRequired();
        builder.Property(message => message.LastError).HasMaxLength(2000);
        builder.HasIndex(message => new { message.PublishedAt, message.OccurredAt });
    }
}
