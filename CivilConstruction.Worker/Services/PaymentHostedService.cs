using CivilConstruction.Domain.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CivilConstruction.Worker.Services
{
    public class PaymentHostedService : BackgroundService
    {
        private readonly ILogger<PaymentHostedService> _logger;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;
        private readonly string _rabbitHost;
        private readonly string _queueName;

        public PaymentHostedService(ILogger<PaymentHostedService> logger, IConfiguration config, IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _config = config;
            _httpFactory = httpFactory;
            _rabbitHost = config["RabbitMQ:Host"] ?? "localhost";
            _queueName = config["RabbitMQ:QueuePagamentos"] ?? "pagamentos.processed";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PaymentHostedService iniciado");

            // Executa imediatamente na inicialização (útil para testes) e depois diariamente verifica dia 1/16
            await TryProcessPayment();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    await TryProcessPayment();
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex) { _logger.LogError(ex, "Erro no loop do worker"); }
            }
        }

        private async Task TryProcessPayment()
        {
            var hoje = DateTime.UtcNow;
            _logger.LogInformation("Verificando agenda de pagamento em {data}", hoje.ToString("yyyy-MM-dd"));

            // Para produção: dias 1 e 16; para testes remova a condição
            if (hoje.Day == 1 || hoje.Day == 16)
            {
                await ProcessarPagamentoQuinzenal();
            }
        }

        private async Task ProcessarPagamentoQuinzenal()
        {
            try
            {
                _logger.LogInformation("Calculando pagamento quinzenal...");
                var client = _httpFactory.CreateClient("FuncionariosClient");
                var resp = await client.GetAsync("/api/funcionarios");
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync();

                var funcionarios = JsonSerializer.Deserialize<List<FuncionarioDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<FuncionarioDto>();

                decimal total = 0m;
                foreach (var f in funcionarios) total += f.ValorDiaria * 15m;

                var evento = new PagamentoRealizadoEvent
                {
                    Id = Guid.NewGuid(),
                    ValorTotal = total,
                    QuantidadeFuncionarios = funcionarios.Count,
                    DataPagamento = DateTime.UtcNow,
                    Observacao = $"Pagamento quinzenal automático - {DateTime.UtcNow:yyyy-MM-dd}"
                };

                var factory = new ConnectionFactory() { HostName = _rabbitHost };
                using var conn = factory.CreateConnection();
                using var ch = conn.CreateModel();
                ch.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evento));
                var props = ch.CreateBasicProperties(); props.Persistent = true;
                ch.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: props, body: body);

                _logger.LogInformation("Pagamento publicado: {valor} para {count} funcionarios", evento.ValorTotal, evento.QuantidadeFuncionarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro processando pagamento quinzenal");
            }
        }

        private class FuncionarioDto { public Guid Id { get; set; } public string Nome { get; set; } = string.Empty; public decimal ValorDiaria { get; set; } }
    }
}