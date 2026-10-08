namespace CivilConstruction.Infrastructure.Messaging;

public sealed class InboxMessage
{
    public string ConsumerName { get; set; } = "";
    public Guid MessageId { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
}
