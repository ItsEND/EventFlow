using EventFlow.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Api.DataAccess.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("booking");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .ValueGeneratedNever();

            builder.Property(b => b.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(b => b.CreatedAt)
                .IsRequired();

            builder.HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
