using FMS.Domain.Entities;
using FMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FMS.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Owned entity: DisciplineConfig stored as columns in the events table
        builder.OwnsOne(e => e.Discipline, discipline =>
        {
            discipline.Property(d => d.DisciplineName)
                .HasColumnName("discipline_name")
                .HasMaxLength(100)
                .IsRequired();

            discipline.Property(d => d.TimerMode)
                .HasColumnName("timer_mode")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            discipline.Property(d => d.RoundCount)
                .HasColumnName("round_count");

            discipline.Property(d => d.RoundDurationSeconds)
                .HasColumnName("round_duration_seconds");

            discipline.Property(d => d.RestDurationSeconds)
                .HasColumnName("rest_duration_seconds");

            // Nested owned: ScoringConfig
            discipline.OwnsOne(d => d.Scoring, scoring =>
            {
                scoring.Property(s => s.VictoryPoints)
                    .HasColumnName("victory_points")
                    .IsRequired();

                scoring.Property(s => s.DrawPoints)
                    .HasColumnName("draw_points")
                    .IsRequired();

                scoring.Property(s => s.DefeatPoints)
                    .HasColumnName("defeat_points")
                    .IsRequired();

                scoring.Property(s => s.AllowKo)
                    .HasColumnName("allow_ko")
                    .IsRequired();

                scoring.Property(s => s.AllowTko)
                    .HasColumnName("allow_tko")
                    .IsRequired();
            });
        });

        builder.HasMany(e => e.Teams)
            .WithOne(t => t.Event)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Participants)
            .WithOne(p => p.Event)
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Fights)
            .WithOne(f => f.Event)
            .HasForeignKey(f => f.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
