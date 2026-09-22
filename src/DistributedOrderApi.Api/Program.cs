using Asp.Versioning;
using DistributedOrderApi.Api.Middleware;
using DistributedOrderApi.Application;
using DistributedOrderApi.Infrastructure;
using DistributedOrderApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog structured logging
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter()));

// Register Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// API Versioning Setup
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger / OpenAPI documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DistributedOrderApi - E-Commerce Microservice",
        Version = "v1.0",
        Description = "Production-grade distributed .NET Core 8 order processing microservice built with Clean Architecture, CQRS, EF Core + Dapper, and Polly Resilience.",
        Contact = new OpenApiContact
        {
            Name = "Engineering Team",
            Email = "architecture@ecommerce-platform.com"
        }
    });
});

// Health Checks Setup
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderDbContext>("database_efcore")
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=localhost,1433;Database=DistributedOrderDb;User Id=sa;Password=Your_password123!;TrustServerCertificate=True;",
        name: "sqlserver_connection");

var app = builder.Build();

// Auto-Apply Migrations on Startup for dev environment
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        if (db.Database.IsSqlServer())
        {
            db.Database.EnsureCreated();
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not initialize database on startup (may be running in test or offline environment).");
    }
}

// Request Middleware Pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DistributedOrderApi v1.0");
        c.RoutePrefix = string.Empty; // Serve swagger at root URL
    });
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Health Check Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();

public partial class Program { }
