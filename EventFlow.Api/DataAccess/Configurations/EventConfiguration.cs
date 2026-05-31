using EventFlow.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventFlow.Api.DataAccess.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("event", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Event_EndDate_AfterStartDate",
                "\"EndAt\" > \"StartAt\"");

            tableBuilder.HasCheckConstraint(
                "CK_Event_TotalSeats_Positive",
                "\"TotalSeats\" > 0");

            tableBuilder.HasCheckConstraint(
                "CK_Event_AvailableSeats_Range",
                "\"AvailableSeats\" >= 0 AND \"AvailableSeats\" <= \"TotalSeats\"");
        });

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(400);

        builder.Property(e => e.StartAt)
            .IsRequired();

        builder.Property(e => e.EndAt)
            .IsRequired();

        builder.Property(e => e.TotalSeats)
            .IsRequired();

        builder.Property(e => e.AvailableSeats)
            .IsRequired();
    }
}
