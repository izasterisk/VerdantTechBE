namespace BLL.DTO.ProductRegistration;

public class ProductRegistrationImportResponseDTO
{
    public int TotalRows { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public List<ProductRegistrationImportRowResultDTO> Results { get; set; } = new();
}

public class ProductRegistrationImportRowResultDTO
{
    public int RowNumber { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public ulong? ProductRegistrationId { get; set; }
    public string? ProposedProductName { get; set; }
}

