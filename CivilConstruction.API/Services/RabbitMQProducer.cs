using CivilConstruction.Domain.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CivilConstruction.API.Services
{
    public class RabbitMQProducer
    {
        private readonly string _host;
        private readonly string _queueFuncionarios;
        private readonly string _queuePagamentos;

        public RabbitMQProducer(IConfiguration config)
        {
            _host = config["RabbitMQ:Host"] ?? "localhost";
            _queueFuncionarios = config["RabbitMQ:QueueFuncionarios"] ?? "funcionarios.created";
            _queuePagamentos = config["RabbitMQ:QueuePagamentos"] ?? "pagamentos.processed";
        }

        public void PublicarFuncionarioCadastrado(FuncionarioCadastradoEvent ev)
        {
            Publish(_queueFuncionarios, ev);
        }

        public void PublicarPagamentoProcessado(PagamentoRealizadoEvent ev)
        {
            Publish(_queuePagamentos, ev);
        }

        private void Publish<T>(string queueName, T message)
        {
            var factory = new ConnectionFactory() { HostName = _host };
            using var conn = factory.CreateConnection();
            using var ch = conn.CreateModel();

            ch.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            var props = ch.CreateBasicProperties();
            props.Persistent = true;

            ch.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);
        }
    }
}
