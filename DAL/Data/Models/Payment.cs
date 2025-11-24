using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Payments table - stores payment gateway specific information only
/// amount, status, gateway_payment_id are in transactions table
/// </summary>
public partial class Payment
{
    public ulong Id { get; set; }

    public ulong TransactionId { get; set; }

    public ulong OrderId { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentGateway PaymentGateway { get; set; }

    /// <summary>
    /// Raw response from payment gateway (JSON)
    /// </summary>
    public Dictionary<string, object> GatewayResponse { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Transaction Transaction { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
}
