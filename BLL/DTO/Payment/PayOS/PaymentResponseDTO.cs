using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.Services.Payment;

public class PaymentResponseDTO
{
    public ulong Id { get; set; }
    public ulong OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentGateway PaymentGateway { get; set; }
    public string? GatewayPaymentId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public Dictionary<string, object> GatewayResponse { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}