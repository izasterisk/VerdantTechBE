namespace BLL.DTO.Dashboard.Dashboard;

/// <summary>
/// Thống kê giao dịch tài chính toàn hệ thống
/// </summary>
public class AdminTransactionStatisticsDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public AdminTransactionSummaryDTO Summary { get; set; } = new();
    public AdminTransactionByTypeDTO ByType { get; set; } = new();
    public List<AdminDailyTransactionTrendDTO> DailyTrend { get; set; } = new();
}

public class AdminTransactionSummaryDTO
{
    public decimal TotalInflow { get; set; }
    public decimal TotalOutflow { get; set; }
    public decimal NetFlow { get; set; }
}

public class AdminTransactionByTypeDTO
{
    public AdminPaymentInTransactionsDTO PaymentIn { get; set; } = new();
    public AdminWalletTopupTransactionsDTO WalletTopup { get; set; } = new();
    public AdminWalletCashoutTransactionsDTO WalletCashout { get; set; } = new();
    public AdminRefundTransactionsDTO Refund { get; set; } = new();
}

public class AdminPaymentInTransactionsDTO
{
    public int Count { get; set; }
    public decimal CompletedAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public int FailedCount { get; set; }
}

public class AdminWalletTopupTransactionsDTO
{
    public int Count { get; set; }
    public decimal CompletedAmount { get; set; }
}

public class AdminWalletCashoutTransactionsDTO
{
    public int Count { get; set; }
    public decimal CompletedAmount { get; set; }
    public int PendingCount { get; set; }
    public decimal PendingAmount { get; set; }
}

public class AdminRefundTransactionsDTO
{
    public int Count { get; set; }
    public decimal CompletedAmount { get; set; }
    public int PendingCount { get; set; }
    public decimal PendingAmount { get; set; }
}

public class AdminDailyTransactionTrendDTO
{
    public DateOnly Date { get; set; }
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
    public int TransactionCount { get; set; }
}

