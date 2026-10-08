using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Interfaces;
using CivilConstruction.Infrastructure.Context;
using CivilConstruction.Infrastructure.EventSourcing;
using CivilConstruction.Infrastructure.Messaging;
using CivilConstruction.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CivilConstruction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'DefaultConnection' must be configured.");
        }

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        services.AddScoped<IReceitaRepository, ReceitaRepository>();
        services.AddScoped<IDespesaRepository, DespesaRepository>();
        services.AddScoped<ICaixaRepository, CaixaRepository>();
        services.AddScoped<IGestaoConstrucaoRepository, GestaoConstrucaoRepository>();
        services.AddScoped<IGestaoEmpresaRepository, GestaoEmpresaRepository>();
        services.AddScoped<IUnitOfWork, CivilConstruction.Infrastructure.UnitOfWork.UnitOfWork>();
        services.AddScoped<IIntegrationEventOutbox, IntegrationEventOutbox>();
        services.AddScoped<IIntegrationEventInbox, IntegrationEventInbox>();
        services.AddScoped<IEventStore, EfEventStore>();
        services.AddSingleton<RabbitMQProducer>();

        return services;
    }
}
