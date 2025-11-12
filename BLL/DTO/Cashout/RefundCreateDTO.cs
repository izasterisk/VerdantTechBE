using System.ComponentModel.DataAnnotations;
using BLL.DTO.UserBankAccount;

namespace BLL.DTO.Cashout;

public class RefundCreateDTO
{
    [Required(ErrorMessage = "Danh sách OrderDetailId không được để trống.")]
    [MinLength(1, ErrorMessage = "Phải có ít nhất một OrderDetailId")]
    public List<ulong> OrderDetailId { get; set; } = new();

    [Required(ErrorMessage = "RefundAmount là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "RefundAmount phải lớn hơn 0")]
    public int RefundAmount { get; set; }
    
    [Required(ErrorMessage = "Tài khoản nhận tiền là bắt buộc.")]
    public UserBankAccountCreateDTO UserBankAccount { get; set; } = null!;
}