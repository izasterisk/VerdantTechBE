using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.Models;

[Index(nameof(EnvironmentalDataId), IsUnique = true)]
public class Fertilizer
{
    [Key]
    public ulong Id { get; set; }
    
    [Required]
    public ulong EnvironmentalDataId { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal OrganicFertilizer { get; set; } = 0.00m;
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal NpkFertilizer { get; set; } = 0.00m;
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal UreaFertilizer { get; set; } = 0.00m;
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal PhosphateFertilizer { get; set; } = 0.00m;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual EnvironmentalDatum EnvironmentalData { get; set; } = null!;
}
