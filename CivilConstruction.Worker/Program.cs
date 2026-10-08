using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CivilConstruction.Worker.Services;
using CivilConstruction.Application;
using CivilConstruction.Infrastructure;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);
        services.AddHostedService<PaymentHostedService>();
        services.AddHostedService<OutboxDispatchHostedService>();
    })
    .Build();

await host.RunAsync();
