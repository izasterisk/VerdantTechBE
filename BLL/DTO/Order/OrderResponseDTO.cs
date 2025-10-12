namespace BLL.DTO.Order;

public class OrderResponseDTO
{
    
}

public class ProductResponseDTO
{
    public ulong Id { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int WarrantyMonths { get; set; }
    public Dictionary<string, object> Specifications { get; set; } = new();
    public Dictionary<string, decimal> DimensionsCm { get; set; } = new();
}