using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.Wallet;

public class WalletCashoutRequestCreateDTO
{
    // [Required(ErrorMessage = "Vendor ID is required")]
    // public ulong VendorId { get; set; }

    // public ulong? TransactionId { get; set; }

    [Required(ErrorMessage = "ID tài khoản ngân hàng không được để trống.")]
    public ulong BankAccountId { get; set; }

    [Required(ErrorMessage = "Sô tiền cần rút không được để trống.")]
    [Range(1000, int.MaxValue, ErrorMessage = "Số tiền rút tối thiểu là 1000.")]
    public int Amount { get; set; }
    
    // public CashoutStatus Status { get; set; } = CashoutStatus.Pending;

    // [StringLength(255, ErrorMessage = "Gateway transaction ID cannot exceed 255 characters")]
    // public string? GatewayTransactionId { get; set; }

    // public CashoutReferenceType ReferenceType { get; set; }
    
    // public ulong? ReferenceId { get; set; }

    [StringLength(500, ErrorMessage = "Notes không được quá 500 ký tự.")]
    public string? Notes { get; set; }

    // public ulong? ProcessedBy { get; set; }
    
    // public DateTime? ProcessedAt { get; set; }

    // public DateTime CreatedAt { get; set; }

    // public DateTime UpdatedAt { get; set; }
}