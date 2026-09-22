using Asp.Versioning;
using DistributedOrderApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DistributedOrderApi.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class VendorSyncController : ControllerBase
{
    private readonly IVendorCatalogClient _vendorCatalogClient;

    public VendorSyncController(IVendorCatalogClient vendorCatalogClient)
    {
        _vendorCatalogClient = vendorCatalogClient;
    }

    /// <summary>
    /// Manually triggers an outbound inventory sync from external vendor services (demonstrating Polly HTTP resiliency).
    /// </summary>
    [HttpPost("trigger")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorInventoryItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TriggerInventorySync(CancellationToken cancellationToken)
    {
        var inventory = await _vendorCatalogClient.FetchVendorInventoryAsync(cancellationToken);
        return Ok(new
        {
            Message = "Vendor inventory synchronization executed.",
            TotalItemsSynced = inventory.Count,
            Items = inventory
        });
    }
}
