using DAL.Data;

namespace BLL.DTO.Transaction;

public class TransactionCreateDTO
{
    public ulong Id { get; set; }

    public TransactionType TransactionType { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";
    
    public ulong UserId { get; set; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

    public string Note { get; set; } = null!;

    public string? GatewayPaymentId { get; set; }

    public ulong? CreatedBy { get; set; }
    
    public ulong? ProcessedBy { get; set; }
    
    // public DateTime CreatedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    // public DateTime UpdatedAt { get; set; }
}