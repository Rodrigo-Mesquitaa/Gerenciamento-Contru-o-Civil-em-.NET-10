using CivilConstruction.Infrastructure.Context;
using CivilConstruction.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.Tests;

public sealed class IntegrationEventInboxTests
{
    [Fact]
    public async Task ProcessOnceAsync_DeduplicatesRepeatedMessageId()
    {
        await using var db = CreateDbContext();
        var inbox = new IntegrationEventInbox(db);
        var messageId = Guid.NewGuid();
        var appliedCount = 0;

        var first = await inbox.ProcessOnceAsync(
            "inventory-projection",
            messageId,
            _ =>
            {
                appliedCount++;
                db.Funcionarios.Add(new CivilConstruction.Domain.Entities.Funcionario
                {
                    Nome = "Projection side effect",
                    ValorDiaria = 1m
                });
                return Task.CompletedTask;
            },
            CancellationToken.None);
        var duplicate = await inbox.ProcessOnceAsync(
            "inventory-projection",
            messageId,
            _ =>
            {
                appliedCount++;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.True(first);
        Assert.False(duplicate);
        Assert.Equal(1, appliedCount);
        Assert.Single(await db.Funcionarios.ToListAsync());
        Assert.Single(await db.InboxMessages.ToListAsync());
    }

    [Fact]
    public async Task ProcessOnceAsync_DoesNotPersistInboxWhenHandlerFails()
    {
        await using var db = CreateDbContext();
        var inbox = new IntegrationEventInbox(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            inbox.ProcessOnceAsync(
                "inventory-projection",
                Guid.NewGuid(),
                _ => throw new InvalidOperationException("expected test failure"),
                CancellationToken.None));

        Assert.Empty(await db.InboxMessages.ToListAsync());
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
