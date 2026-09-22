using System.Net.Http.Json;
using DistributedOrderApi.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace DistributedOrderApi.Infrastructure.Services;

public class VendorCatalogClient : IVendorCatalogClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VendorCatalogClient> _logger;

    public VendorCatalogClient(HttpClient httpClient, ILogger<VendorCatalogClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VendorInventoryItemDto>> FetchVendorInventoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Requesting external vendor catalog inventory...");
            var items = await _httpClient.GetFromJsonAsync<List<VendorInventoryItemDto>>("/api/v1/vendor/inventory", cancellationToken);
            return items ?? new List<VendorInventoryItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch vendor catalog inventory. Returning fallback mock catalog.");
            return GetFallbackInventory();
        }
    }

    public async Task<bool> ConfirmVendorStockAsync(string productId, int quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Verifying stock for Product {ProductId}, Quantity {Quantity} with vendor...", productId, quantity);
            var response = await _httpClient.PostAsJsonAsync("/api/v1/vendor/verify-stock", new { ProductId = productId, Quantity = quantity }, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Vendor stock verification endpoint unavailable for Product {ProductId}. Degraded stock check fallback applied.", productId);
            return true; // Fallback optimistic verification for resilience demo
        }
    }

    private static List<VendorInventoryItemDto> GetFallbackInventory() => new()
    {
        new("PROD-001", "SKU-LAPTOP-01", 50, 1299.99m),
        new("PROD-002", "SKU-HEADPHONES-02", 150, 199.99m),
        new("PROD-003", "SKU-MONITOR-03", 30, 449.99m),
    };
}
