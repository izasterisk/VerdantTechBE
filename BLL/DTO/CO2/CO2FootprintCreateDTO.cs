using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.CO2;

public class CO2FootprintCreateDTO
{
    // [Required(ErrorMessage = "ID hồ sơ trang trại là bắt buộc")]
    // [Range(1, ulong.MaxValue, ErrorMessage = "ID hồ sơ trang trại phải là số dương")]
    // public ulong FarmProfileId { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu đo đạc là bắt buộc")]
    public DateOnly MeasurementStartDate { get; set; }

    [Required(ErrorMessage = "Ngày kết thúc đo đạc là bắt buộc")]
    public DateOnly MeasurementEndDate { get; set; }

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
    public string? Notes { get; set; }

    // Thuộc tính tiêu thụ năng lượng
    [Required(ErrorMessage = "Lượng điện tiêu thụ là bắt buộc")]
    [Range(0, 99999999.99, ErrorMessage = "Lượng điện tiêu thụ tính theo kWh và phải là số dương")]
    [RegularExpression(@"^\d{1,8}(\.\d{1,2})?$", ErrorMessage = "Lượng điện tiêu thụ tối đa 8 chữ số trước dấu phẩy và 2 chữ số sau dấu phẩy")]
    public decimal ElectricityKwh { get; set; } = 0.00m;

    [Required(ErrorMessage = "Lượng xăng tiêu thụ là bắt buộc")]
    [Range(0, 99999999.99, ErrorMessage = "Lượng xăng tiêu thụ phải là số dương")]
    [RegularExpression(@"^\d{1,8}(\.\d{1,2})?$", ErrorMessage = "Lượng xăng tiêu thụ tối đa 8 chữ số trước dấu phẩy và 2 chữ số sau dấu phẩy")]
    public decimal GasolineLiters { get; set; } = 0.00m;

    [Required(ErrorMessage = "Lượng dầu diesel tiêu thụ là bắt buộc")]
    [Range(0, 99999999.99, ErrorMessage = "Lượng dầu diesel tiêu thụ phải là số dương")]
    [RegularExpression(@"^\d{1,8}(\.\d{1,2})?$", ErrorMessage = "Lượng dầu diesel tiêu thụ tối đa 8 chữ số trước dấu phẩy và 2 chữ số sau dấu phẩy")]
    public decimal DieselLiters { get; set; } = 0.00m;

    // Thuộc tính phân bón
    [Required(ErrorMessage = "Lượng phân hữu cơ là bắt buộc")]
    [Range(0, 99999999.99, ErrorMessage = "Lượng phân hữu cơ tính theo kg và phải là số dương")]
    [RegularExpression(@"^\d{1,8}(\.\d{1,2})?$", ErrorMessage = "Lượng phân hữu cơ tối đa 8 chữ số trước dấu phẩy và 2 chữ số sau dấu phẩy")]
    public decimal OrganicFertilizer { get; set; } = 0.00m;

    [Required(ErrorMessage = "Lượng phân NPK là bắt buộc")]
    [Range(0, 99999999.99, ErrorMessage = "Lượng phân NPK tính theo kg và phải là số dương")]
    [RegularExpression(@"^\d{1,8}(\.\d{1,2})?$", ErrorMessage = "Lượng phân NPK tối đa 8 chữ số trước dấu phẩy và 2 chữ số sau dấu phẩy")]
    public decimal NpkFertilizer { get; set; } = 0.00m;

    [Required(ErrorMessage = "Lượng phân urê là bắt buộc")]
    [Range(0, 99999999.99, ErrorMessage = "Lượng phân urê tính theo kg và phải là số dương")]
    [RegularExpression(@"^\d{1,8}(\.\d{1,2})?$", ErrorMessage = "Lượng phân urê tối đa 8 chữ số trước dấu phẩy và 2 chữ số sau dấu phẩy")]
    public decimal UreaFertilizer { get; set; } = 0.00m;

    [Required(ErrorMessage = "Lượng phân lân là bắt buộc")]
    [Range(0, 99999999.99, ErrorMessage = "Lượng phân lân tính theo kg và phải là số dương")]
    [RegularExpression(@"^\d{1,8}(\.\d{1,2})?$", ErrorMessage = "Lượng phân lân tối đa 8 chữ số trước dấu phẩy và 2 chữ số sau dấu phẩy")]
    public decimal PhosphateFertilizer { get; set; } = 0.00m;
}