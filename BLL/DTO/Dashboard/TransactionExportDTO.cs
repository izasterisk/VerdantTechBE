namespace BLL.DTO.Dashboard;

public class TransactionExportDTO
{
    public ulong TransactionId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
    public string? ReferenceCode { get; set; }
}