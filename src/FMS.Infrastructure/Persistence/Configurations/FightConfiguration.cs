using FMS.Domain.Entities;
using FMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FMS.Infrastructure.Persistence.Configurations;

public class FightConfiguration : IEntityTypeConfiguration<Fight>
{
    public void Configure(EntityTypeBuilder<Fight> builder)
    {
        builder.ToTable("fights");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.OrderNumber)
            .IsRequired();

        builder.Property(f => f.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(f => f.ParticipantA)
            .WithMany()
            .HasForeignKey(f => f.ParticipantAId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.ParticipantB)
            .WithMany()
            .HasForeignKey(f => f.ParticipantBId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Result)
            .WithOne(r => r.Fight)
            .HasForeignKey<FightResult>(r => r.FightId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: order within an event
        builder.HasIndex(f => new { f.EventId, f.OrderNumber })
            .IsUnique();

        builder.HasIndex(f => f.EventId);
    }
}
