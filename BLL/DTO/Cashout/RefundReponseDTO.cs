using BLL.DTO.Order;
using BLL.DTO.UserBankAccount;

namespace BLL.DTO.Cashout;

public class RefundReponseDTO
{
    public List<OrderDetailsResponseDTO> OrderDetails { get; set; } = new();
    public UserBankAccountResponseDTO UserBankAccount { get; set; } = new();
}