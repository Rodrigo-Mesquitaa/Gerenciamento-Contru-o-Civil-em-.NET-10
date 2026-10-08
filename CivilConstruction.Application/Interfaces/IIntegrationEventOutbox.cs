namespace CivilConstruction.Application.Interfaces;

public interface IIntegrationEventOutbox
{
    Task<bool> ContainsAsync(Guid messageId, CancellationToken cancellationToken);

    void Add(
        Guid messageId,
        string eventType,
        int schemaVersion,
        string payload,
        DateTimeOffset occurredAt);
}
