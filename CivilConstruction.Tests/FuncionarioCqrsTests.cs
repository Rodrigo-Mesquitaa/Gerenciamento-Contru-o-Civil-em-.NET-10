using CivilConstruction.Application;
using CivilConstruction.Application.Features.Funcionarios;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Events;
using CivilConstruction.Domain.Interfaces;
using CivilConstruction.Infrastructure.Context;
using CivilConstruction.Infrastructure.Messaging;
using CivilConstruction.Infrastructure.Repositories;
using CivilConstruction.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;

namespace CivilConstruction.Tests;

public sealed class FuncionarioCqrsTests
{
    [Fact]
    public async Task EmployeeCommandsAndQueries_RunThroughMediatRAndPersistOutboxAtomically()
    {
        await using var provider = CreateProvider();
        await using var scope = provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var created = await sender.Send(new CriarFuncionarioRequest
        {
            Nome = "  Engenheira Civil ",
            ValorDiaria = 450m
        });

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Engenheira Civil", created.Nome);
        Assert.Contains(await sender.Send(new ListarFuncionariosQuery()), item => item.Id == created.Id);
        Assert.Equal(created, await sender.Send(new ObterFuncionarioQuery(created.Id)));

        var storedEvent = await db.OutboxMessages.SingleAsync();
        Assert.Equal(created.Id, storedEvent.Id);
        Assert.Equal(IntegrationEventTypes.EmployeeRegistered, storedEvent.EventType);
        Assert.Equal(OutboxMessageStatus.Pending, storedEvent.Status);

        Assert.True(await sender.Send(new AtualizarFuncionarioCommand(created.Id, "Mestre de obras", 500m)));
        Assert.Equal(
            "Mestre de obras",
            (await sender.Send(new ObterFuncionarioQuery(created.Id)))!.Nome);

        Assert.True(await sender.Send(new RemoverFuncionarioCommand(created.Id)));
        Assert.Null(await sender.Send(new ObterFuncionarioQuery(created.Id)));
        Assert.False(await sender.Send(new RemoverFuncionarioCommand(created.Id)));
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIntegrationEventOutbox, IntegrationEventOutbox>();
        return services.BuildServiceProvider(validateScopes: true);
    }
}
