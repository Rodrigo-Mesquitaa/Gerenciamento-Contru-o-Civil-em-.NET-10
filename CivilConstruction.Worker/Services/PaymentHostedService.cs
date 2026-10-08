using CivilConstruction.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CivilConstruction.Worker.Services;

public sealed class PaymentHostedService(
    ILogger<PaymentHostedService> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker de pagamentos programados iniciado");
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextCheck = TimeSpan.FromHours(1);
            try
            {
                await ProcessarPagamentosAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception,
                    "Falha ao processar pagamentos programados; nova tentativa em cinco minutos");
                nextCheck = TimeSpan.FromMinutes(5);
            }

            try
            {
                await Task.Delay(nextCheck, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessarPagamentosAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGestaoConstrucaoService>();
        var criados = await service.GerarPagamentosVencidosAsync(DateTimeOffset.UtcNow, ct);
        if (criados.Count > 0)
            logger.LogInformation("Criadas {Count} competências de pagamento vencidas", criados.Count);

        var pendentes = await service.ListarPagamentosPendentesAsync(ct);
        foreach (var pagamento in pendentes)
        {
            await service.EnfileirarPagamentoAsync(pagamento.Id, ct);
            logger.LogInformation("Pagamento {PaymentId} enfileirado para publicação via outbox",
                pagamento.Id);
        }
    }
}
