using CivilConstruction.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;

namespace CivilConstruction.Tests;

public sealed class RabbitMQProducerConfigurationTests
{
    [Theory]
    [InlineData(null, "configured-password")]
    [InlineData("guest", "guest")]
    [InlineData("configured-user", null)]
    public void Constructor_RequiresExplicitNonGuestCredentials(string? userName, string? password)
    {
        var settings = new Dictionary<string, string?>
        {
            ["RabbitMQ:Host"] = "localhost",
            ["RabbitMQ:UserName"] = userName,
            ["RabbitMQ:Password"] = password
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        Assert.Throws<InvalidOperationException>(() => new RabbitMQProducer(configuration));
    }
}
