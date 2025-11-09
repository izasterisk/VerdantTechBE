using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.Wallet;

public class WalletProcessCreateDTO
{
    [EnumDataType(typeof(CashoutStatus), ErrorMessage = "Trạng thái phải là 1 trong 'completed','failed','cancelled'.")]
    public CashoutStatus Status { get; set; }

    public string? Reason { get; set; }
}