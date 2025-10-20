using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.Models;

[Table("addresses")]
public class Address
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }

    [Column("location_address")]
    public string? LocationAddress { get; set; }

    [Column("province")]
    [StringLength(100)]
    [Required]
    public string Province { get; set; } = string.Empty;

    [Column("district")]
    [StringLength(100)]
    [Required]
    public string District { get; set; } = string.Empty;

    [Column("commune")]
    [StringLength(100)]
    [Required]
    public string Commune { get; set; } = string.Empty;

    [Column("province_code")]
    [Required]
    [StringLength(20)]
    public string ProvinceCode { get; set; } = string.Empty;

    [Column("district_code")]
    [Required]
    [StringLength(20)]
    public string DistrictCode { get; set; } = string.Empty;

    [Column("commune_code")]
    [Required]
    [StringLength(20)]
    public string CommuneCode { get; set; } = string.Empty;

    [Column("latitude")]
    [Precision(10, 8)]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    [Precision(11, 8)]
    public decimal? Longitude { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
    public virtual ICollection<FarmProfile> FarmProfiles { get; set; } = new List<FarmProfile>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
