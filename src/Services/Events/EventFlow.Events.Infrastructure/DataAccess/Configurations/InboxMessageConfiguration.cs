using EventFlow.Events.Infrastructure.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Events.Infrastructure.DataAccess.Configurations;

public sealed class InboxMessageConfiguration
    : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");
        builder.HasKey(message => message.MessageId);

        builder.Property(message => message.MessageId).ValueGeneratedNever();
        builder.Property(message => message.ReceivedAt).IsRequired();
        builder.Property(message => message.ProcessedAt);
        builder.Property(message => message.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(message => message.Details).HasMaxLength(1000);

        builder.HasIndex(message => new
        {
            message.Status,
            message.ReceivedAt
        });
    }
}