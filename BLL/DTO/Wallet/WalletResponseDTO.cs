namespace BLL.DTO.Wallet;

public class WalletResponseDTO
{
    public ulong Id { get; set; }
    public ulong VendorId { get; set; }
    public decimal Balance { get; set; } = 0.00m;
    public ulong? LastUpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}