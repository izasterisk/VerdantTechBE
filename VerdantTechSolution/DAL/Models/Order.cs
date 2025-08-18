using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Customer orders
/// </summary>
public partial class Order
{
    public ulong Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public ulong CustomerId { get; set; }

    public ulong VendorId { get; set; }

    public string? Status { get; set; }

    public decimal Subtotal { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? ShippingFee { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string? CurrencyCode { get; set; }

    public string ShippingAddress { get; set; } = null!;

    public string? BillingAddress { get; set; }

    public string? ShippingMethod { get; set; }

    public string? TrackingNumber { get; set; }

    public string? Notes { get; set; }

    public string? CancelledReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual User Vendor { get; set; } = null!;
}
