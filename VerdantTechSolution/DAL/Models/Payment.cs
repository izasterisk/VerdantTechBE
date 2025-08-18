using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Payment transactions
/// </summary>
public partial class Payment
{
    public ulong Id { get; set; }

    public ulong OrderId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentGateway { get; set; } = null!;

    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public string? CurrencyCode { get; set; }

    public string? Status { get; set; }

    /// <summary>
    /// Raw response from payment gateway
    /// </summary>
    public string? GatewayResponse { get; set; }

    public decimal? RefundAmount { get; set; }

    public string? RefundReason { get; set; }

    public DateTime? RefundedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
