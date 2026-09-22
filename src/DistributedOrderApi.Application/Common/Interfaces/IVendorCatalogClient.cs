namespace DistributedOrderApi.Application.Common.Interfaces;

public record VendorInventoryItemDto(string ProductId, string SKU, int AvailableQuantity, decimal UnitPrice);

public interface IVendorCatalogClient
{
    Task<IReadOnlyList<VendorInventoryItemDto>> FetchVendorInventoryAsync(CancellationToken cancellationToken = default);
    Task<bool> ConfirmVendorStockAsync(string productId, int quantity, CancellationToken cancellationToken = default);
}
