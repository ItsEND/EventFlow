using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformTemplate.ServiceName.Infrastructure.Messaging.Inbox;

internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");
        builder.HasKey(message => message.MessageId);
        builder.Property(message => message.EventType).HasMaxLength(512).IsRequired();
        builder.Property(message => message.Payload).IsRequired();
        builder.Property(message => message.Error).HasMaxLength(2000);
        builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(message => message.ReceivedAt);
    }
}
