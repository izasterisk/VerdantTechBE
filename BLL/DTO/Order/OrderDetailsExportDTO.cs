using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Order;

public class OrderDetailsExportDTO
{
    [Required(ErrorMessage = "Mã đơn hàng là bắt buộc.")]
    [Range(1, ulong.MaxValue, ErrorMessage = "Mã sản phẩm ít nhất là 1.")]
    public ulong OrderDetailId { get; set; }
    
    [Required(ErrorMessage = "Số lô/sê-ri là bắt buộc.")]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 số lô/sê-ri.")]
    public List<ExportIdentityNumbersDTO> IdentityNumbers { get; set; } = new();
}

public class ExportIdentityNumbersDTO
{
    [StringLength(50, ErrorMessage = "Số sê-ri không được vượt quá 50 ký tự.")]
    public string? SerialNumber { get; set; }
    
    [Required(ErrorMessage = "Số lô là bắt buộc")]
    [StringLength(50, ErrorMessage = "Số lô không được vượt quá 50 ký tự.")]
    public string LotNumber { get; set; } = null!;
    
    [Required(ErrorMessage = "Số lượng xuất là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải ít nhất là 1.")]
    public int Quantity { get; set; }
}