using DistributedOrderApi.Application.Common.Interfaces;
using DistributedOrderApi.Infrastructure.Persistence;
using DistributedOrderApi.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedOrderApi.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real SQL Server DbContext if present
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<OrderDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestOrderDb");
            });

            // Ensure IOrderReadRepository uses InMemoryOrderReadRepository
            var readRepoDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IOrderReadRepository));
            if (readRepoDescriptor != null)
            {
                services.Remove(readRepoDescriptor);
            }

            services.AddScoped<IOrderReadRepository, InMemoryOrderReadRepository>();

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<OrderDbContext>();

            db.Database.EnsureCreated();
            var seeder = scopedServices.GetRequiredService<DemoDataSeeder>();
            seeder.SeedAsync().GetAwaiter().GetResult();
        });

        builder.UseEnvironment("Development");
    }
}
