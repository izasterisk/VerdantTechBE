namespace BLL.DTO.VendorBankAccount;

public class VendorBankAccountResponseDTO
{
    public ulong Id { get; set; }

    public ulong VendorId { get; set; }

    public string BankCode { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string AccountHolder { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
