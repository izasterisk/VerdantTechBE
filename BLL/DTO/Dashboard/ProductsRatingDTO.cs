using BLL.DTO.Order;

namespace BLL.DTO.Dashboard;

public class ProductsRatingDTO
{
    public decimal AverageRatingOfVendor { get; set; }
    public TopProductsRatingResponseDTO Top3Highest { get; set; } = null!;
    public WorstProductsRatingResponseDTO Top3Lowest { get; set; } = null!;
}

public class TopProductsRatingResponseDTO
{
    public ProductResponseDTO Top1 { get; set; } = null!;
    public ProductResponseDTO Top2 { get; set; } = null!;
    public ProductResponseDTO Top3 { get; set; } = null!;
}

public class WorstProductsRatingResponseDTO
{
    public ProductResponseDTO Top1 { get; set; } = null!;
    public ProductResponseDTO Top2 { get; set; } = null!;
    public ProductResponseDTO Top3 { get; set; } = null!;
}