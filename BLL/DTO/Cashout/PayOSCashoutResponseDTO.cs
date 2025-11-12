namespace BLL.DTO.Cashout;

public class PayOSCashoutResponseDTO
{
    public string Id { get; set; } = null!;
    
    public string ReferenceId { get; set; } = null!;
    
    public long Amount { get; set; }
    
    public string Description { get; set; } = null!;
    
    public string ToBin { get; set; } = null!;
    
    public string ToAccountNumber { get; set; } = null!;
    
    public string ToAccountName { get; set; } = null!;
    
    public string? Reference { get; set; }
    
    public string? TransactionDatetime { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public string? ErrorCode { get; set; }
    
    public string State { get; set; } = null!;
}
