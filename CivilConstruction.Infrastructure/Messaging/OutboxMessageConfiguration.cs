using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivilConstruction.Infrastructure.Messaging;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", table =>
        {
            table.HasCheckConstraint(
                "CK_OutboxMessages_SchemaVersion_Positive", "[SchemaVersion] > 0");
            table.HasCheckConstraint(
                "CK_OutboxMessages_Attempts_NonNegative", "[Attempts] >= 0");
        });
        builder.HasKey(message => message.Id);
        builder.Property(message => message.EventType).HasMaxLength(200).IsRequired();
        builder.Property(message => message.Payload).IsRequired();
        builder.Property(message => message.LastError).HasMaxLength(1000);
        builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(message => new
        {
            message.Status,
            message.NextAttemptAt,
            message.LeaseExpiresAt
        });
    }
}
