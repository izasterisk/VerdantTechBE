using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

public class Cart
{
    [Key]
    public ulong Id { get; set; }
    
    [Required]
    public ulong CustomerId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual User Customer { get; set; } = null!;
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
