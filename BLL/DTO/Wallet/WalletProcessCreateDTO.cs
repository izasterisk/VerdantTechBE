using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.Wallet;

public class WalletProcessCreateDTO
{
    [EnumDataType(typeof(TransactionStatus), ErrorMessage = "Trạng thái phải là 1 trong 'completed','failed','cancelled'.")]
    public TransactionStatus Status { get; set; }
    
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Mã giao dịch phải ít hơn 255 ký tự")]
    public string? GatewayPaymentId { get; set; }
    
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Lý do phải ít hơn 255 ký tự")]
    public string? CancelReason { get; set; }
}