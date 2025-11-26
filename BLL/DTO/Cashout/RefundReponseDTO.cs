using BLL.DTO.Order;
using BLL.DTO.Transaction;
using BLL.DTO.UserBankAccount;
using BLL.DTO.Wallet;

namespace BLL.DTO.Cashout;

public class RefundReponseDTO
{
    public List<OrderDetailsResponseDTO> OrderDetails { get; set; } = new();
    public TransactionResponseDTO TransactionInfo { get; set; } = new();
}