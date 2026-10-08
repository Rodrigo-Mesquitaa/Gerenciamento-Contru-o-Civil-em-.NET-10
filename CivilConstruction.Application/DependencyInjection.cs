using CivilConstruction.Application.Interfaces;
using CivilConstruction.Application.Services;
using CivilConstruction.Application.Features.Funcionarios;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CivilConstruction.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(CriarFuncionarioHandler).Assembly));
        services.AddScoped<IServicoFinanceiro, ServicoFinanceiro>();
        services.AddScoped<IGestaoConstrucaoService, GestaoConstrucaoService>();
        services.AddScoped<IGestaoEmpresaService, GestaoEmpresaService>();
        return services;
    }
}
