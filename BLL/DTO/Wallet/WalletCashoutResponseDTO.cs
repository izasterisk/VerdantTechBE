using BLL.DTO.Cashout;
using BLL.DTO.Transaction;
using BLL.DTO.User;
using BLL.DTO.UserBankAccount;
using DAL.Data;

namespace BLL.DTO.Wallet;

public class WalletCashoutResponseDTO
{
    public ulong Id { get; set; }

    // public ulong VendorId { get; set; }
    public UserResponseDTO Vendor { get; set; } = null!;
    
    // public ulong? TransactionId { get; set; }
    public WalletTransactionResponseDTO Transaction { get; set; } = null!;

    // public ulong BankAccountId { get; set; }
    public UserBankAccountResponseDTO BankAccount { get; set; } = null!;

    // public decimal Amount { get; set; }

    public CashoutStatus Status { get; set; } = CashoutStatus.Processing;

    public string? Reason { get; set; }

    // public CashoutReferenceType? ReferenceType { get; set; }

    // public ulong? ReferenceId { get; set; }

    public string? Notes { get; set; }

    // public ulong? ProcessedBy { get; set; }
    public UserResponseDTO ProcessedBy { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class WalletTransactionResponseDTO
{
    public ulong Id { get; set; }

    public TransactionType TransactionType { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";

    // public ulong UserId { get; set; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

    public string Note { get; set; } = null!;

    public string? GatewayPaymentId { get; set; }

    // public ulong? CreatedBy { get; set; }
    // public ulong? ProcessedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}