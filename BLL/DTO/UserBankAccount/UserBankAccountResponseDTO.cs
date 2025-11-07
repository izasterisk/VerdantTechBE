namespace BLL.DTO.UserBankAccount;

public class UserBankAccountResponseDTO
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public string BankCode { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string AccountHolder { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
