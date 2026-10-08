namespace CivilConstruction.Infrastructure.EventSourcing;

public sealed class DomainEventRecord
{
    public Guid EventId { get; set; }
    public Guid StreamId { get; set; }
    public long Version { get; set; }
    public string EventType { get; set; } = "";
    public int SchemaVersion { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string Payload { get; set; } = "";
}
