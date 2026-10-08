using CivilConstruction.Application;
using CivilConstruction.Infrastructure;
using CivilConstruction.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token de acesso JWT emitido pelo provedor de identidade da organização."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        if (!builder.Environment.IsDevelopment())
        {
            options.Authority = builder.Configuration["Authentication:Authority"];
            options.Audience = builder.Configuration["Authentication:Audience"];
        }
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("finance.approval.read", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "finance.approval.read"))
    .AddPolicy("finance.approval.request", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "finance.approval.request"))
    .AddPolicy("finance.approval.approve", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "finance.approval.approve"))
    .AddPolicy("finance.approval.release", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "finance.approval.release"));
if (!builder.Environment.IsDevelopment())
{
    var authority = builder.Configuration["Authentication:Authority"];
    var audience = builder.Configuration["Authentication:Audience"];
    if (string.IsNullOrWhiteSpace(authority) || string.IsNullOrWhiteSpace(audience))
        throw new InvalidOperationException(
            "Authentication:Authority and Authentication:Audience are required outside Development.");

    builder.Services.AddAuthorization(options =>
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build());
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var url = builder.Configuration.GetValue<string>("Kestrel:Endpoints:Http:Url") ?? "http://localhost:5000";
app.Urls.Clear();
app.Urls.Add(url);

app.Run();
