namespace BLL.DTO.Dashboard.VendorDashboard;

/// <summary>
/// Danh sách các yêu cầu đang chờ xử lý của vendor
/// </summary>
public class VendorPendingItemsDTO
{
    public VendorPendingProductRegistrationsDTO ProductRegistrations { get; set; } = new();
    public VendorPendingProductUpdatesDTO ProductUpdateRequests { get; set; } = new();
    public VendorPendingCertificatesDTO VendorCertificates { get; set; } = new();
    public VendorPendingProductCertificatesDTO ProductCertificates { get; set; } = new();
    public VendorPendingCashoutDTO CashoutRequests { get; set; } = new();
}

public class VendorPendingProductRegistrationsDTO
{
    public int Count { get; set; }
    public List<VendorPendingProductRegistrationItemDTO> Items { get; set; } = new();
}

public class VendorPendingProductRegistrationItemDTO
{
    public ulong Id { get; set; }
    public string ProposedProductName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class VendorPendingProductUpdatesDTO
{
    public int Count { get; set; }
    public List<VendorPendingProductUpdateItemDTO> Items { get; set; } = new();
}

public class VendorPendingProductUpdateItemDTO
{
    public ulong Id { get; set; }
    public ulong ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class VendorPendingCertificatesDTO
{
    public int Count { get; set; }
    public List<VendorPendingCertificateItemDTO> Items { get; set; } = new();
}

public class VendorPendingCertificateItemDTO
{
    public ulong Id { get; set; }
    public string CertificationName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
}

public class VendorPendingProductCertificatesDTO
{
    public int Count { get; set; }
    public List<VendorPendingProductCertificateItemDTO> Items { get; set; } = new();
}

public class VendorPendingProductCertificateItemDTO
{
    public ulong Id { get; set; }
    public ulong ProductId { get; set; }
    public string CertificationName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
}

public class VendorPendingCashoutDTO
{
    public int Count { get; set; }
    public List<VendorPendingCashoutItemDTO> Items { get; set; } = new();
}

public class VendorPendingCashoutItemDTO
{
    public ulong TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string BankAccountNumber { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

