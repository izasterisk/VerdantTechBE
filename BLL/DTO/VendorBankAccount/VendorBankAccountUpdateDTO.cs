using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.VendorBankAccount;

public class VendorBankAccountUpdateDTO
{
    [StringLength(20, ErrorMessage = "Mã ngân hàng không được vượt quá 20 ký tự")]
    public string? BankCode { get; set; }

    [StringLength(50, ErrorMessage = "Số tài khoản không được vượt quá 50 ký tự")]
    public string? AccountNumber { get; set; }

    [StringLength(255, ErrorMessage = "Tên chủ tài khoản không được vượt quá 255 ký tự")]
    public string? AccountHolder { get; set; }
}
