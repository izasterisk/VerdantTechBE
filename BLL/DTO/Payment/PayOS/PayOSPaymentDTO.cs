namespace BLL.DTO.Payment.PayOS;

/// <summary>
/// Item data cho payment link
/// </summary>
public class PayOSItemDTO
{
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public int Price { get; set; }
}

/// <summary>
/// Request để tạo payment link (đầy đủ tất cả fields của PaymentData)
/// </summary>
public class CreatePaymentLinkRequestDTO
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = null!;
    public List<PayOSItemDTO> Items { get; set; } = new();
    public string CancelUrl { get; set; } = null!;
    public string ReturnUrl { get; set; } = null!;
    
    // Optional fields
    public string? Signature { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }
    public string? BuyerAddress { get; set; }
    public long? ExpiredAt { get; set; }
}

/// <summary>
/// Response khi tạo payment link
/// </summary>
public class CreatePaymentLinkResponseDTO
{
    public string CheckoutUrl { get; set; } = null!;
    public string PaymentLinkId { get; set; } = null!;
    public long OrderCode { get; set; }
    public string QrCode { get; set; } = null!;
}

/// <summary>
/// Thông tin chi tiết payment link
/// </summary>
public class PaymentLinkInfoDTO
{
    public string Id { get; set; } = null!;
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public int AmountPaid { get; set; }
    public int AmountRemaining { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<PaymentTransactionDTO>? Transactions { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CanceledAt { get; set; }
}

/// <summary>
/// Transaction info trong payment link
/// </summary>
public class PaymentTransactionDTO
{
    public string Reference { get; set; } = null!;
    public int Amount { get; set; }
    public string AccountNumber { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime TransactionDateTime { get; set; }
    public string? VirtualAccountName { get; set; }
    public string? VirtualAccountNumber { get; set; }
    public string? CounterAccountBankId { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountName { get; set; }
    public string? CounterAccountNumber { get; set; }
}

/// <summary>
/// Webhook data từ PayOS
/// </summary>
public class PayOSWebhookDataDTO
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string Reference { get; set; } = null!;
    public string TransactionDateTime { get; set; } = null!;
    public string Currency { get; set; } = "VND";
    public string PaymentLinkId { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Desc { get; set; } = null!;
    public string? CounterAccountBankId { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountName { get; set; }
    public string? CounterAccountNumber { get; set; }
    public string? VirtualAccountName { get; set; }
    public string? VirtualAccountNumber { get; set; }
}
