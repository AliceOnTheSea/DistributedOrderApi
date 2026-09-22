using DistributedOrderApi.Domain.Entities;
using DistributedOrderApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedOrderApi.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(o => o.Customer, customer =>
        {
            customer.Property(c => c.CustomerId).HasColumnName("CustomerId").HasMaxLength(50).IsRequired();
            customer.Property(c => c.FullName).HasColumnName("CustomerName").HasMaxLength(100).IsRequired();
            customer.Property(c => c.Email).HasColumnName("CustomerEmail").HasMaxLength(150).IsRequired();
        });

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(200).IsRequired();
            address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(100).IsRequired();
            address.Property(a => a.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20).IsRequired();
            address.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(100).IsRequired();
        });

        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
        });

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(o => o.DomainEvents);
    }
}
