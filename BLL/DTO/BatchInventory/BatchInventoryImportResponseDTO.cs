namespace BLL.DTO.BatchInventory;

public class BatchInventoryImportResponseDTO
{
    public int TotalRows { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public List<BatchInventoryImportRowResultDTO> Results { get; set; } = new();
}

public class BatchInventoryImportRowResultDTO
{
    public int RowNumber { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public ulong? BatchInventoryId { get; set; }
    public string? BatchNumber { get; set; }
    public string? Sku { get; set; }
}

