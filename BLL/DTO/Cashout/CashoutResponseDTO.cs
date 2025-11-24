using DAL.Data;

namespace BLL.DTO.Cashout;

public class CashoutResponseDTO
{
    public ulong Id { get; set; }

    // public ulong TransactionId { get; set; }

    public CashoutReferenceType ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}