using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivilConstruction.Infrastructure.EventSourcing;

internal sealed class EventStreamConfiguration : IEntityTypeConfiguration<EventStream>
{
    public void Configure(EntityTypeBuilder<EventStream> builder)
    {
        builder.ToTable("EventStreams");
        builder.HasKey(stream => stream.Id);
        builder.Property(stream => stream.Version).IsConcurrencyToken();
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_EventStreams_Version_NonNegative", "[Version] >= 0"));
    }
}

internal sealed class DomainEventRecordConfiguration : IEntityTypeConfiguration<DomainEventRecord>
{
    public void Configure(EntityTypeBuilder<DomainEventRecord> builder)
    {
        builder.ToTable("DomainEventRecords", table =>
        {
            table.HasCheckConstraint(
                "CK_DomainEventRecords_Version_Positive", "[Version] > 0");
            table.HasCheckConstraint(
                "CK_DomainEventRecords_SchemaVersion_Positive", "[SchemaVersion] > 0");
        });
        builder.HasKey(record => new { record.StreamId, record.Version });
        builder.HasIndex(record => record.EventId).IsUnique();
        builder.Property(record => record.EventType).HasMaxLength(200).IsRequired();
        builder.Property(record => record.Payload).IsRequired();
        builder.HasOne<EventStream>()
            .WithMany()
            .HasForeignKey(record => record.StreamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
