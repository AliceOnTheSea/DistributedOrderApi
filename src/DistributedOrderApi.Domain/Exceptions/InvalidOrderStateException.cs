using DistributedOrderApi.Domain.Enums;

namespace DistributedOrderApi.Domain.Exceptions;

public class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException(OrderStatus currentStatus, OrderStatus targetStatus)
        : base($"Cannot transition order from '{currentStatus}' to '{targetStatus}'.") { }

    public InvalidOrderStateException(string message) : base(message) { }
}
