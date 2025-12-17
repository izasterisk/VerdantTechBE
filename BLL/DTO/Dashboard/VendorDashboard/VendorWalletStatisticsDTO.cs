namespace BLL.DTO.Dashboard.VendorDashboard;

/// <summary>
/// Thống kê ví và giao dịch của vendor
/// </summary>
public class VendorWalletStatisticsDTO
{
    public decimal CurrentBalance { get; set; }
    public decimal PendingCashout { get; set; }
    public decimal AvailableBalance { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public VendorTransactionSummaryDTO TransactionSummary { get; set; } = new();
    public VendorPendingCreditsDTO PendingCredits { get; set; } = new();
    public List<VendorRecentTransactionDTO> RecentTransactions { get; set; } = new();
}

public class VendorTransactionSummaryDTO
{
    public decimal TotalTopup { get; set; }
    public decimal TotalCashout { get; set; }
    public int TopupCount { get; set; }
    public int CashoutCount { get; set; }
}

public class VendorPendingCreditsDTO
{
    public decimal Amount { get; set; }
    public int OrderCount { get; set; }
}

public class VendorRecentTransactionDTO
{
    public ulong TransactionId { get; set; }
    public string TransactionType { get; set; } = null!;
    public decimal Amount { get; set; }
    public ulong? OrderId { get; set; }
    public string Status { get; set; } = null!;
    public string Note { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

