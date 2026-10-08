using CivilConstruction.Application.Interfaces;
using CivilConstruction.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.Infrastructure.Messaging;

public sealed class IntegrationEventInbox(AppDbContext db) : IIntegrationEventInbox
{
    public async Task<bool> ProcessOnceAsync(
        string consumerName,
        Guid messageId,
        Func<CancellationToken, Task> process,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerName);
        ArgumentNullException.ThrowIfNull(process);
        if (messageId == Guid.Empty)
            throw new ArgumentException("A mensagem precisa de um identificador estável.", nameof(messageId));

        if (await db.InboxMessages.AnyAsync(
                message => message.ConsumerName == consumerName && message.MessageId == messageId,
                cancellationToken))
            return false;

        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;

        db.InboxMessages.Add(new InboxMessage
        {
            ConsumerName = consumerName,
            MessageId = messageId,
            ProcessedAt = DateTimeOffset.UtcNow
        });

        await process(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
