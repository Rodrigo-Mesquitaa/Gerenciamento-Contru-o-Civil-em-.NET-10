using CivilConstruction.Domain.Events;

namespace CivilConstruction.Application.Interfaces;

public sealed record StoredDomainEvent(
    Guid EventId,
    Guid StreamId,
    long Version,
    string EventType,
    int SchemaVersion,
    DateTimeOffset OccurredAt,
    IDomainEvent Payload);

public interface IEventStore
{
    Task<IReadOnlyList<StoredDomainEvent>> ReadStreamAsync(
        Guid streamId,
        CancellationToken cancellationToken);

    Task AppendAsync(
        Guid streamId,
        long expectedVersion,
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken cancellationToken);
}

public sealed class EventStreamConcurrencyException(Guid streamId, long expectedVersion)
    : InvalidOperationException(
        $"Event stream {streamId} changed since expected version {expectedVersion}.")
{
    public Guid StreamId { get; } = streamId;
    public long ExpectedVersion { get; } = expectedVersion;
}
