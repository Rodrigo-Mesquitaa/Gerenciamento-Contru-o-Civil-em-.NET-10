namespace CivilConstruction.Application.Interfaces;

public interface IIntegrationEventInbox
{
    Task<bool> ProcessOnceAsync(
        string consumerName,
        Guid messageId,
        Func<CancellationToken, Task> process,
        CancellationToken cancellationToken);
}
