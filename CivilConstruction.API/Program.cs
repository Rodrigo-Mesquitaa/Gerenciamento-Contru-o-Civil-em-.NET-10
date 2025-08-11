using CivilConstruction.API.Services;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Application.Services;
using CivilConstruction.Domain.Interfaces;
using CivilConstruction.Infrastructure.Context;
using CivilConstruction.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext - shared AppDbContext (you can scope to per-service DB)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IFuncionarioRepository, CivilConstruction.Infrastructure.Repositories.FuncionarioRepository>();
builder.Services.AddScoped<IReceitaRepository, CivilConstruction.Infrastructure.Repositories.ReceitaRepository>();
builder.Services.AddScoped<IDespesaRepository, CivilConstruction.Infrastructure.Repositories.DespesaRepository>();
builder.Services.AddScoped<ICaixaRepository, CivilConstruction.Infrastructure.Repositories.CaixaRepository>();

// UnitOfWork
builder.Services.AddScoped<CivilConstruction.Infrastructure.UnitOfWork.IUnitOfWork, UnitOfWork>();

// Application services
builder.Services.AddScoped<IServicoFuncionario, ServicoFuncionario>();
builder.Services.AddScoped<IServicoFinanceiro, ServicoFinanceiro>();

// RabbitMQ producer
builder.Services.AddSingleton<RabbitMQProducer>();

var app = builder.Build();

// Ensure DB (DEV)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

var url = builder.Configuration.GetValue<string>("Kestrel:Endpoints:Http:Url") ?? "http://localhost:5001";
app.Urls.Clear();
app.Urls.Add(url);

app.Run();
