using DistributedOrderApi.Domain.Common;
using DistributedOrderApi.Domain.Exceptions;

namespace DistributedOrderApi.Domain.ValueObjects;

public class CustomerInfo : ValueObject
{
    public string CustomerId { get; private set; }
    public string FullName { get; private set; }
    public string Email { get; private set; }

    private CustomerInfo()
    {
        CustomerId = string.Empty;
        FullName = string.Empty;
        Email = string.Empty;
    }

    public CustomerInfo(string customerId, string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(customerId)) throw new DomainException("Customer ID is required.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new DomainException("Full name is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) throw new DomainException("A valid email address is required.");

        CustomerId = customerId.Trim();
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CustomerId;
        yield return FullName;
        yield return Email;
    }
}
