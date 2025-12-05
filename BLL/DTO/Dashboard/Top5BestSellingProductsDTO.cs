using BLL.DTO.Order;

namespace BLL.DTO.Dashboard;

public class Top5BestSellingProductsResponseDTO
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public List<Top5BestSellingProductsDTO> Products { get; set; } = new();
}

public class Top5BestSellingProductsDTO
{
    public int SoldQuantity { get; set; }
    public ProductResponseDTO Product { get; set; } = null!;
}