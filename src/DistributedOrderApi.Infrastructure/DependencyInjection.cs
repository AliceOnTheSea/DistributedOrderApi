using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Infrastructure.Persistence;
using DistributedOrderApi.Infrastructure.Persistence.Repositories;
using DistributedOrderApi.Infrastructure.Services;
using DistributedOrderApi.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace DistributedOrderApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost,1433;Database=DistributedOrderDb;User Id=sa;Password=Your_password123!;TrustServerCertificate=True;";

        // EF Core Registration
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<OrderDbContext>());

        // Dapper Repository Registration
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();

        // Domain Event Dispatcher Registration
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        // Polly Resilient HTTP Client for Vendor Catalog
        services.AddHttpClient<IVendorCatalogClient, VendorCatalogClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["VendorApi:BaseUrl"] ?? "https://api.vendor-mock.com");
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Background Inventory Synchronization Worker
        services.AddHostedService<VendorInventorySyncWorker>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
