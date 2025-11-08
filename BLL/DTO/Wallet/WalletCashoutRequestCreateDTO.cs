using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.Wallet;

public class WalletCashoutRequestCreateDTO
{
    // [Required(ErrorMessage = "Vendor ID is required")]
    // public ulong VendorId { get; set; }

    // public ulong? TransactionId { get; set; }

    [Required(ErrorMessage = "Bank account ID is required")]
    public ulong BankAccountId { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    // public CashoutStatus Status { get; set; } = CashoutStatus.Pending;

    [StringLength(255, ErrorMessage = "Reason cannot exceed 255 characters")]
    public string? Reason { get; set; }

    // [StringLength(255, ErrorMessage = "Gateway transaction ID cannot exceed 255 characters")]
    // public string? GatewayTransactionId { get; set; }

    // public CashoutReferenceType ReferenceType { get; set; }
    
    // public ulong? ReferenceId { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    // public ulong? ProcessedBy { get; set; }
    
    // public DateTime? ProcessedAt { get; set; }

    // public DateTime CreatedAt { get; set; }

    // public DateTime UpdatedAt { get; set; }
}