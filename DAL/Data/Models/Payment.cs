using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Payments table (money coming in)
/// </summary>
public partial class Payment
{
    public ulong Id { get; set; }

    public ulong OrderId { get; set; }


    public PaymentMethod PaymentMethod { get; set; }

    public PaymentGateway PaymentGateway { get; set; }

    [StringLength(255)]
    public string? GatewayPaymentId { get; set; }

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Raw response from payment gateway (JSON)
    /// </summary>
    public Dictionary<string, object> GatewayResponse { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
}
