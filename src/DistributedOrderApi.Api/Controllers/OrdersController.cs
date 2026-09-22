using Asp.Versioning;
using DistributedOrderApi.Application.Dtos;
using DistributedOrderApi.Application.Orders.Commands;
using DistributedOrderApi.Application.Orders.Queries;
using DistributedOrderApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DistributedOrderApi.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Submits a new e-commerce order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.CustomerName,
            request.CustomerEmail,
            request.ShippingAddress,
            request.Currency,
            request.Items);

        var orderId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { id = orderId, version = "1.0" }, new { id = orderId });
    }

    /// <summary>
    /// Retrieves a specific order by ID (Dapper high-performance read query).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Order with ID {id} was not found." });

        return Ok(result);
    }

    /// <summary>
    /// Retrieves all orders associated with a customer ID.
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByCustomer([FromRoute] string customerId, CancellationToken cancellationToken)
    {
        var query = new GetOrdersByCustomerQuery(customerId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets aggregate order metrics summary.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(OrderSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderSummary(CancellationToken cancellationToken)
    {
        var query = new GetOrderSummaryQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the status of an active order.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus([FromRoute] Guid id, [FromBody] OrderStatus targetStatus, CancellationToken cancellationToken)
    {
        var command = new UpdateOrderStatusCommand(id, targetStatus);
        await _mediator.Send(command, cancellationToken);
        return Ok(new { message = $"Order {id} status updated to {targetStatus} successfully." });
    }

    /// <summary>
    /// Cancels an order.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid id, [FromBody] string reason, CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand(id, reason);
        await _mediator.Send(command, cancellationToken);
        return Ok(new { message = $"Order {id} cancelled successfully." });
    }
}
