namespace CivilConstruction.Infrastructure.EventSourcing;

public sealed class EventStream
{
    public Guid Id { get; set; }
    public long Version { get; set; }
}
