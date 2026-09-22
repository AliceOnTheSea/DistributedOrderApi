using DistributedOrderApi.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DistributedOrderApi.Infrastructure.Workers;

public class VendorInventorySyncWorker : BackgroundService
{
    private readonly IVendorCatalogClient _vendorCatalogClient;
    private readonly ILogger<VendorInventorySyncWorker> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);

    public VendorInventorySyncWorker(
        IVendorCatalogClient vendorCatalogClient,
        ILogger<VendorInventorySyncWorker> logger)
    {
        _vendorCatalogClient = vendorCatalogClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Vendor Inventory Synchronization Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Executing vendor catalog inventory synchronization loop...");
                var items = await _vendorCatalogClient.FetchVendorInventoryAsync(stoppingToken);

                _logger.LogInformation("Vendor inventory sync complete. Total items synced: {Count}", items.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during vendor inventory synchronization execution.");
            }

            await Task.Delay(_syncInterval, stoppingToken);
        }

        _logger.LogInformation("Vendor Inventory Synchronization Worker stopping.");
    }
}
