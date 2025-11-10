using BLL.DTO.User;
using BLL.DTO.UserBankAccount;
using DAL.Data;

namespace BLL.DTO.Wallet;

public class WalletCashoutRequestResponseDTO
{
    public ulong Id { get; set; }

    // public ulong VendorId { get; set; }
    public UserResponseDTO User { get; set; } = null!;

    // public ulong? TransactionId { get; set; }

    // public ulong BankAccountId { get; set; }
    public UserBankAccountResponseDTO BankAccount { get; set; } = null!;
    
    public decimal Amount { get; set; }

    public CashoutStatus Status { get; set; } = CashoutStatus.Processing;

    public CashoutReferenceType? ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    public string? Notes { get; set; }

    // public ulong? ProcessedBy { get; set; }
    
    // public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    // public DateTime UpdatedAt { get; set; }
}