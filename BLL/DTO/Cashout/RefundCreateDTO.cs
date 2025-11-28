using System.ComponentModel.DataAnnotations;
using BLL.DTO.Order;
using BLL.DTO.UserBankAccount;

namespace BLL.DTO.Cashout;

public class RefundCreateDTO
{
    [Required(ErrorMessage = "Danh sách OrderDetailId không được để trống.")]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 OrderDetail.")]
    public List<OrderDetailsExportDTO> OrderDetails { get; set; } = null!;

    [Required(ErrorMessage = "RefundAmount là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "RefundAmount phải lớn hơn 0")]
    public int RefundAmount { get; set; }
    
    [Required(ErrorMessage = "Tài khoản nhận tiền là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "BankAccountId phải lớn hơn 0")]
    public ulong BankAccountId { get; set; }
    
    [StringLength(255, ErrorMessage = "GatewayPaymentId không được vượt quá 255 ký tự")]
    public string? GatewayPaymentId { get; set; }
}