using System.ComponentModel.DataAnnotations;
using BLL.DTO.UserBankAccount;

namespace BLL.DTO.Cashout;

public class RefundCreateDTO
{
    [Required(ErrorMessage = "Danh sách OrderDetailId không được để trống.")]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 OrderDetail.")]
    public List<RefundOrderDetailDTO> OrderDetails { get; set; } = null!;

    [Required(ErrorMessage = "RefundAmount là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "RefundAmount phải lớn hơn 0")]
    public int RefundAmount { get; set; }
    
    [Required(ErrorMessage = "Tài khoản nhận tiền là bắt buộc.")]
    public UserBankAccountCreateDTO UserBankAccount { get; set; } = null!;
    
    [StringLength(255, ErrorMessage = "GatewayPaymentId không được vượt quá 255 ký tự")]
    public string? GatewayPaymentId { get; set; }
}

public class RefundOrderDetailDTO
{
    [Required(ErrorMessage = "OrderDetailId là bắt buộc.")]
    [Range(1, ulong.MaxValue, ErrorMessage = "OrderDetailId phải lớn hơn 0")]
    public ulong OrderDetailId { get; set; }
    
    [Required(ErrorMessage = "RefundQuantity là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "RefundQuantity phải lớn hơn 0")]
    public int RefundQuantity { get; set; }
    
    [Required(ErrorMessage = "LotNumber là bắt buộc.")]
    [StringLength(100, ErrorMessage = "LotNumber không được vượt quá 100 ký tự")]
    public string LotNumber { get; set; } = null!;
    
    [StringLength(100, ErrorMessage = "SerialNumber không được vượt quá 100 ký tự")]
    public string? SerialNumber { get; set; }
}