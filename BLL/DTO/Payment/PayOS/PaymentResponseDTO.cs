using System.ComponentModel.DataAnnotations;
using BLL.DTO.Transaction;
using DAL.Data;
using Net.payOS.Types;

namespace BLL.DTO.Payment.PayOS;

public class PaymentResponseDTO
{
    public ulong Id { get; set; }

    // public ulong TransactionId { get; set; }
    // public TransactionResponseDTO Transaction { get; set; } = null!;

    public ulong OrderId { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentGateway PaymentGateway { get; set; }
    
    public Dictionary<string, object> GatewayResponse { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    
    public PaymentLinkInformation? PaymentLinkInformation { get; set; }
}