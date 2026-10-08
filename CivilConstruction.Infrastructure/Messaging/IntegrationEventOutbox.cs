using CivilConstruction.Application.Interfaces;
using CivilConstruction.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.Infrastructure.Messaging;

public sealed class IntegrationEventOutbox(AppDbContext db) : IIntegrationEventOutbox
{
    public Task<bool> ContainsAsync(Guid messageId, CancellationToken cancellationToken) =>
        db.OutboxMessages.AnyAsync(message => message.Id == messageId, cancellationToken);

    public void Add(
        Guid messageId,
        string eventType,
        int schemaVersion,
        string payload,
        DateTimeOffset occurredAt) =>
        db.OutboxMessages.Add(new OutboxMessage
        {
            Id = messageId,
            EventType = eventType,
            SchemaVersion = schemaVersion,
            Payload = payload,
            OccurredAt = occurredAt,
            NextAttemptAt = occurredAt,
            Status = OutboxMessageStatus.Pending
        });
}
