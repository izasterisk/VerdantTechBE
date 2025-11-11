using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.UserBankAccount;

public class UserBankAccountCreateDTO
{
    [Required(ErrorMessage = "Mã ngân hàng là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã ngân hàng không được vượt quá 20 ký tự")]
    public string BankCode { get; set; } = null!;

    [Required(ErrorMessage = "Số tài khoản là bắt buộc")]
    [StringLength(50, ErrorMessage = "Số tài khoản không được vượt quá 50 ký tự")]
    public string AccountNumber { get; set; } = null!;
}
