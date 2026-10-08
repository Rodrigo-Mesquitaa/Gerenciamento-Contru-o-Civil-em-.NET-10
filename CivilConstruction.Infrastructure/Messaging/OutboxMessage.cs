namespace CivilConstruction.Infrastructure.Messaging;

public enum OutboxMessageStatus
{
    Pending,
    Processing,
    Published,
    DeadLetter
}

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = "";
    public int SchemaVersion { get; set; }
    public string Payload { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset NextAttemptAt { get; set; }
    public DateTimeOffset? LeaseExpiresAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public OutboxMessageStatus Status { get; set; }
}
