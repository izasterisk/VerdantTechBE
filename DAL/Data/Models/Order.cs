using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Customer orders
/// </summary>
public partial class Order
{
    public ulong Id { get; set; }

    public ulong CustomerId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; } = 0.00m;

    public decimal ShippingFee { get; set; } = 0.00m;

    public decimal DiscountAmount { get; set; } = 0.00m;

    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Shipping address (JSON)
    /// </summary>
    public Dictionary<string, object> ShippingAddress { get; set; } = new();

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
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<SalesInventory> SalesInventories { get; set; } = new List<SalesInventory>();
}
