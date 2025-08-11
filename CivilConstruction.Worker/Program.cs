using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using CivilConstruction.Worker.Services;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient("FuncionariosClient", client =>
        {
            client.BaseAddress = new Uri(context.Configuration["FuncionariosApi:BaseUrl"]);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHostedService<PaymentHostedService>();
        services.AddLogging(cfg => cfg.AddConsole());
    })
    .Build();

await host.RunAsync();
