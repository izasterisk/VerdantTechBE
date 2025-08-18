using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Customer orders
/// </summary>
public partial class Order
{
    public ulong Id { get; set; }

    [Required]
    [StringLength(50)]
    public string OrderNumber { get; set; } = null!;

    public ulong CustomerId { get; set; }

    public ulong VendorId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; } = 0.00m;

    public decimal ShippingFee { get; set; } = 0.00m;

    public decimal DiscountAmount { get; set; } = 0.00m;

    public decimal TotalAmount { get; set; }

    [StringLength(3)]
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Shipping address (JSON)
    /// </summary>
    public Dictionary<string, object> ShippingAddress { get; set; } = new();

    /// <summary>
    /// Billing address (JSON)
    /// </summary>
    public Dictionary<string, object>? BillingAddress { get; set; }

    [StringLength(100)]
    public string? ShippingMethod { get; set; }

    [StringLength(100)]
    public string? TrackingNumber { get; set; }

    public string? Notes { get; set; }

    public string? CancelledReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User Customer { get; set; } = null!;
    public virtual User Vendor { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
}
