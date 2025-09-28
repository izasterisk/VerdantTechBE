using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

public class CartItem
{
    [Key]
    public ulong Id { get; set; }
    
    [Required]
    public ulong CartId { get; set; }
    
    [Required]
    public ulong ProductId { get; set; }
    
    [Required]
    public int Quantity { get; set; } = 1;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Cart Cart { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
