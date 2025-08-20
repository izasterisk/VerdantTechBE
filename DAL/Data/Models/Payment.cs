using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Payment transactions
/// </summary>
public partial class Payment
{
    public ulong Id { get; set; }

    public ulong OrderId { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentGateway PaymentGateway { get; set; }

    [StringLength(255)]
    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Raw response from payment gateway (JSON)
    /// </summary>
    public Dictionary<string, object> GatewayResponse { get; set; } = new();

    public decimal RefundAmount { get; set; } = 0.00m;

    public string? RefundReason { get; set; }

    public DateTime? RefundedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
}
