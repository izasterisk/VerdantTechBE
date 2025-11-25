using BLL.DTO.Cashout;
using BLL.DTO.Payment.PayOS;
using BLL.DTO.User;
using BLL.DTO.UserBankAccount;
using DAL.Data;

namespace BLL.DTO.Transaction;

public class TransactionResponseDTO
{
    public ulong Id { get; set; }
    
    public PaymentResponseDTO? Payment { get; set; }
    
    public CashoutResponseDTO? Cashout { get; set; }

    public TransactionType TransactionType { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";

    // public ulong UserId { get; set; }
    public UserResponseDTO User { get; set; } = null!;

    public ulong? OrderId { get; set; }

    // public ulong? BankAccountId { get; set; }
    public UserBankAccountResponseDTO? BankAccount { get; set; }
    
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    public string Note { get; set; } = null!;

    public string? GatewayPaymentId { get; set; }

    // public ulong? CreatedBy { get; set; }
    public UserResponseDTO? CreatedBy { get; set; }
    // public ulong? ProcessedBy { get; set; }
    public UserResponseDTO? ProcessedBy { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}