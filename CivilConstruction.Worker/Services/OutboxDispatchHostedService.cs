using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Events;
using CivilConstruction.Infrastructure.Context;
using CivilConstruction.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CivilConstruction.Worker.Services;

public sealed class OutboxDispatchHostedService(
    ILogger<OutboxDispatchHostedService> logger,
    IServiceScopeFactory scopeFactory,
    RabbitMQProducer producer) : BackgroundService
{
    private const int BatchSize = 25;
    private const int MaximumAttempts = 10;
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Dispatcher da outbox iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeSpan.FromSeconds(15);
            try
            {
                var dispatched = await DispatchBatchAsync(stoppingToken);
                if (dispatched > 0)
                    delay = TimeSpan.FromSeconds(1);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Falha ao processar a outbox");
                delay = TimeSpan.FromSeconds(5);
            }

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task<int> DispatchBatchAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTimeOffset.UtcNow;
        var candidateIds = await db.OutboxMessages
            .AsNoTracking()
            .Where(message =>
                (message.Status == OutboxMessageStatus.Pending && message.NextAttemptAt <= now) ||
                (message.Status == OutboxMessageStatus.Processing && message.LeaseExpiresAt <= now))
            .OrderBy(message => message.NextAttemptAt)
            .Select(message => message.Id)
            .Take(BatchSize)
            .ToArrayAsync(ct);

        var dispatched = 0;
        foreach (var id in candidateIds)
        {
            ct.ThrowIfCancellationRequested();
            var claimedAt = DateTimeOffset.UtcNow;
            var claimed = await db.OutboxMessages
                .Where(message => message.Id == id &&
                    ((message.Status == OutboxMessageStatus.Pending && message.NextAttemptAt <= claimedAt) ||
                     (message.Status == OutboxMessageStatus.Processing &&
                      message.LeaseExpiresAt <= claimedAt)))
                .ExecuteUpdateAsync(update => update
                    .SetProperty(message => message.Status, OutboxMessageStatus.Processing)
                    .SetProperty(message => message.LeaseExpiresAt, claimedAt.Add(LeaseDuration))
                    .SetProperty(message => message.Attempts, message => message.Attempts + 1), ct);
            if (claimed == 0)
                continue;

            var message = await db.OutboxMessages.SingleAsync(item => item.Id == id, ct);
            try
            {
                await PublishAsync(message, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                await ScheduleRetryAsync(db, message, exception, ct);
                continue;
            }

            message.Status = OutboxMessageStatus.Published;
            message.PublishedAt = DateTimeOffset.UtcNow;
            message.LeaseExpiresAt = null;
            message.LastError = null;

            if (message.EventType == IntegrationEventTypes.ScheduledPaymentCreated)
            {
                var pagamento = await db.PagamentosFuncionarios
                    .SingleOrDefaultAsync(item => item.Id == message.Id, ct);
                if (pagamento is not null)
                    pagamento.EventoPublicado = true;
            }

            await db.SaveChangesAsync(ct);
            dispatched++;
            logger.LogInformation(
                "Mensagem outbox {MessageId} publicada no RabbitMQ após {Attempts} tentativa(s)",
                message.Id,
                message.Attempts);
        }

        return dispatched;
    }

    private Task PublishAsync(OutboxMessage message, CancellationToken ct)
    {
        if (message.SchemaVersion != 1)
            throw new InvalidOperationException(
                $"Tipo ou versão de evento da outbox não suportado: {message.EventType} v{message.SchemaVersion}.");

        return message.EventType switch
        {
            IntegrationEventTypes.ScheduledPaymentCreated =>
                producer.PublicarPagamentoProcessadoJsonAsync(
                    message.Id, message.Payload, message.SchemaVersion, ct),
            IntegrationEventTypes.EmployeeRegistered =>
                producer.PublicarFuncionarioCadastradoJsonAsync(
                    message.Id, message.Payload, message.SchemaVersion, ct),
            _ => throw new InvalidOperationException(
                $"Tipo de evento da outbox não suportado: {message.EventType}.")
        };
    }

    private async Task ScheduleRetryAsync(
        AppDbContext db,
        OutboxMessage message,
        Exception exception,
        CancellationToken ct)
    {
        message.LastError = $"{exception.GetType().Name}: publicação falhou";
        message.LeaseExpiresAt = null;

        if (message.Attempts >= MaximumAttempts)
        {
            message.Status = OutboxMessageStatus.DeadLetter;
            logger.LogError(
                exception,
                "Mensagem outbox {MessageId} enviada para dead letter após {Attempts} tentativas",
                message.Id,
                message.Attempts);
        }
        else
        {
            message.Status = OutboxMessageStatus.Pending;
            var exponentialDelaySeconds = Math.Min(3600, 5 * Math.Pow(2, message.Attempts - 1));
            var delaySeconds = exponentialDelaySeconds * (0.8 + Random.Shared.NextDouble() * 0.4);
            message.NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
            logger.LogWarning(
                exception,
                "Falha ao publicar mensagem outbox {MessageId}; nova tentativa em {DelaySeconds} segundos",
                message.Id,
                delaySeconds);
        }

        await db.SaveChangesAsync(ct);
    }
}
