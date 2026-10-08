using CivilConstruction.Domain.Events;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace CivilConstruction.Infrastructure.Messaging;

public sealed class RabbitMQProducer : IAsyncDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly string _queueFuncionarios;
    private readonly string _queuePagamentos;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;

    public RabbitMQProducer(IConfiguration configuration)
    {
        var host = configuration["RabbitMQ:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException("RabbitMQ:Host must be configured.");
        }

        var userName = configuration["RabbitMQ:UserName"];
        if (string.IsNullOrWhiteSpace(userName) ||
            string.Equals(userName, "guest", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "RabbitMQ:UserName must be configured with a non-guest account.");
        }

        var password = configuration["RabbitMQ:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("RabbitMQ:Password must be configured.");
        }

        _connectionFactory = new ConnectionFactory
        {
            HostName = host,
            UserName = userName,
            Password = password
        };
        _queueFuncionarios = configuration["RabbitMQ:QueueFuncionarios"] ?? "funcionarios.created";
        _queuePagamentos = configuration["RabbitMQ:QueuePagamentos"] ?? "pagamentos.processed";
    }

    public Task PublicarPagamentoProcessadoJsonAsync(
        Guid messageId,
        string payload,
        int schemaVersion,
        CancellationToken cancellationToken = default) =>
        PublishJsonAsync(
            _queuePagamentos,
            messageId,
            payload,
            IntegrationEventTypes.ScheduledPaymentCreated,
            schemaVersion,
            cancellationToken);

    public Task PublicarFuncionarioCadastradoJsonAsync(
        Guid messageId,
        string payload,
        int schemaVersion,
        CancellationToken cancellationToken = default) =>
        PublishJsonAsync(
            _queueFuncionarios,
            messageId,
            payload,
            IntegrationEventTypes.EmployeeRegistered,
            schemaVersion,
            cancellationToken);

    private async Task PublishJsonAsync(
        string queueName,
        Guid messageId,
        string payload,
        string eventType,
        int schemaVersion,
        CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);
        await using var channel = await connection.CreateChannelAsync(channelOptions, cancellationToken);

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = messageId.ToString("N"),
            Headers = new Dictionary<string, object?>
            {
                ["event-type"] = eventType,
                ["schema-version"] = schemaVersion
            }
        };
        var body = Encoding.UTF8.GetBytes(payload);
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true }) return _connection;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is not { IsOpen: true })
            {
                if (_connection is not null)
                {
                    await _connection.DisposeAsync();
                }

                _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            }

            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _connectionLock.Dispose();
    }
}
