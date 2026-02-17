using FMS.Domain.Entities;
using FMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FMS.Infrastructure.Persistence.Configurations;

public class FightResultConfiguration : IEntityTypeConfiguration<FightResult>
{
    public void Configure(EntityTypeBuilder<FightResult> builder)
    {
        builder.ToTable("fight_results");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Outcome)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Method)
            .HasMaxLength(50);

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        builder.HasIndex(r => r.FightId)
            .IsUnique();
    }
}
