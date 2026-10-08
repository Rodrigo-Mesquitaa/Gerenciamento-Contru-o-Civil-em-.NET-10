using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Events;
using CivilConstruction.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CivilConstruction.Infrastructure.EventSourcing;

public sealed class EfEventStore(AppDbContext db) : IEventStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<StoredDomainEvent>> ReadStreamAsync(
        Guid streamId,
        CancellationToken cancellationToken)
    {
        var rows = await db.DomainEventRecords
            .AsNoTracking()
            .Where(record => record.StreamId == streamId)
            .OrderBy(record => record.Version)
            .ToArrayAsync(cancellationToken);

        return rows.Select(record => new StoredDomainEvent(
            record.EventId,
            record.StreamId,
            record.Version,
            record.EventType,
            record.SchemaVersion,
            record.OccurredAt,
            Deserialize(record.EventType, record.SchemaVersion, record.Payload)))
            .ToArray();
    }

    public async Task AppendAsync(
        Guid streamId,
        long expectedVersion,
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken cancellationToken)
    {
        if (streamId == Guid.Empty)
            throw new ArgumentException("Stream identifier is required.", nameof(streamId));
        if (expectedVersion < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedVersion));
        ArgumentNullException.ThrowIfNull(events);
        if (events.Count == 0)
            throw new ArgumentException("At least one domain event is required.", nameof(events));

        var stream = await db.EventStreams
            .SingleOrDefaultAsync(item => item.Id == streamId, cancellationToken);

        if (stream is null)
        {
            if (expectedVersion != 0)
                throw new EventStreamConcurrencyException(streamId, expectedVersion);

            stream = new EventStream { Id = streamId, Version = 0 };
            db.EventStreams.Add(stream);
        }
        else if (stream.Version != expectedVersion)
        {
            throw new EventStreamConcurrencyException(streamId, expectedVersion);
        }

        var now = DateTimeOffset.UtcNow;
        var nextVersion = expectedVersion;
        foreach (var domainEvent in events)
        {
            var serialized = Serialize(domainEvent);
            nextVersion++;
            db.DomainEventRecords.Add(new DomainEventRecord
            {
                EventId = Guid.NewGuid(),
                StreamId = streamId,
                Version = nextVersion,
                EventType = serialized.EventType,
                SchemaVersion = serialized.SchemaVersion,
                OccurredAt = now,
                Payload = serialized.Payload
            });
        }

        stream.Version = nextVersion;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new EventStreamConcurrencyException(streamId, expectedVersion);
        }
    }

    private static (string EventType, int SchemaVersion, string Payload) Serialize(IDomainEvent domainEvent) =>
        domainEvent switch
        {
            AprovacaoPagamentoSolicitada payload =>
                ("payment-approval.requested", 1, JsonSerializer.Serialize(payload, JsonOptions)),
            AprovacaoPagamentoAprovada payload =>
                ("payment-approval.approved", 1, JsonSerializer.Serialize(payload, JsonOptions)),
            AprovacaoPagamentoRejeitada payload =>
                ("payment-approval.rejected", 1, JsonSerializer.Serialize(payload, JsonOptions)),
            PagamentoLiberado payload =>
                ("payment-approval.released", 1, JsonSerializer.Serialize(payload, JsonOptions)),
            _ => throw new InvalidOperationException(
                $"Domain event type is not registered in the event store: {domainEvent.GetType().Name}.")
        };

    private static IDomainEvent Deserialize(string eventType, int schemaVersion, string payload)
    {
        if (schemaVersion != 1)
            throw new InvalidOperationException($"Unsupported event schema version: {schemaVersion}.");

        IDomainEvent? domainEvent = eventType switch
        {
            "payment-approval.requested" =>
                JsonSerializer.Deserialize<AprovacaoPagamentoSolicitada>(payload, JsonOptions),
            "payment-approval.approved" =>
                JsonSerializer.Deserialize<AprovacaoPagamentoAprovada>(payload, JsonOptions),
            "payment-approval.rejected" =>
                JsonSerializer.Deserialize<AprovacaoPagamentoRejeitada>(payload, JsonOptions),
            "payment-approval.released" =>
                JsonSerializer.Deserialize<PagamentoLiberado>(payload, JsonOptions),
            _ => throw new InvalidOperationException($"Unknown domain event type: {eventType}.")
        };

        return domainEvent
            ?? throw new InvalidOperationException($"Event payload was empty for event type {eventType}.");
    }
}
