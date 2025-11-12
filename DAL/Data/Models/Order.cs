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

    public ulong AddressId { get; set; }

    public OrderPaymentMethod OrderPaymentMethod { get; set; }

    [StringLength(100)]
    public string? ShippingMethod { get; set; }

    [StringLength(100)]
    public string? TrackingNumber { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public int CourierId { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int Length { get; set; } = 0;

    public int Weight { get; set; } = 0;

    [StringLength(500)]
    public string? CancelledReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User Customer { get; set; } = null!;
    public virtual Address Address { get; set; } = null!;
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<ExportInventory> ExportInventories { get; set; } = new List<ExportInventory>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
