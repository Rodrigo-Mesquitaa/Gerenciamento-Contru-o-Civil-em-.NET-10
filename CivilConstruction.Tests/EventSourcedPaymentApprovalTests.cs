using CivilConstruction.Application;
using CivilConstruction.Application.Features.AprovacoesPagamento;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Infrastructure.Context;
using CivilConstruction.Infrastructure.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;

namespace CivilConstruction.Tests;

public sealed class EventSourcedPaymentApprovalTests
{
    [Fact]
    public async Task ApprovalLifecycle_RehydratesFromAppendOnlyEvents()
    {
        await using var provider = CreateProvider();
        await using var scope = provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var now = DateTimeOffset.UtcNow;
        var requester = Guid.NewGuid();
        var approver = Guid.NewGuid();
        var releaser = Guid.NewGuid();

        var requested = await sender.Send(new SolicitarAprovacaoPagamentoCommand(
            Guid.NewGuid(), Guid.NewGuid(), requester, now));
        await sender.Send(new AprovarPagamentoCommand(
            requested.Id, approver, now.AddMinutes(1)));
        var released = await sender.Send(new LiberarPagamentoCommand(
            requested.Id, releaser, now.AddMinutes(2)));
        var replayed = await sender.Send(new ObterAprovacaoPagamentoQuery(requested.Id));

        Assert.Equal(StatusAprovacaoPagamento.Liberado, released.Status);
        Assert.Equal(StatusAprovacaoPagamento.Liberado, replayed!.Status);
        Assert.Equal(3, replayed.Version);
        Assert.Equal(approver, replayed.AprovadorId);
        Assert.Equal(releaser, replayed.LiberadorId);
        Assert.Equal(3, await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .DomainEventRecords.CountAsync());
    }

    [Fact]
    public async Task Aggregate_EnforcesSegregationOfDutiesAndValidTransitions()
    {
        var requester = Guid.NewGuid();
        var approval = AprovacaoPagamento.Solicitar(
            Guid.NewGuid(), Guid.NewGuid(), requester, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            approval.Aprovar(requester, DateTimeOffset.UtcNow));

        var approver = Guid.NewGuid();
        approval.Aprovar(approver, DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() =>
            approval.Liberar(approver, DateTimeOffset.UtcNow));

        approval.Liberar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() =>
            approval.Rejeitar(Guid.NewGuid(), "tarde", DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task EventStore_RejectsAppendWithStaleExpectedVersion()
    {
        await using var db = CreateDbContext();
        var store = new EfEventStore(db);
        var aggregate = AprovacaoPagamento.Solicitar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        await store.AppendAsync(
            aggregate.Id, aggregate.Version, aggregate.UncommittedEvents, CancellationToken.None);

        await Assert.ThrowsAsync<EventStreamConcurrencyException>(() =>
            store.AppendAsync(
                aggregate.Id,
                expectedVersion: 0,
                [new AprovacaoPagamentoAprovada(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow)],
                CancellationToken.None));
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IEventStore, EfEventStore>();
        return services.BuildServiceProvider(validateScopes: true);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
