using System.ComponentModel.DataAnnotations;

namespace BLL.Helpers.AddressHelper;

public static class AddressHelper
{
    /// <summary>
    /// Kiểm tra tính hợp lệ của các trường địa chỉ - các cặp trường phải cùng null hoặc cùng có giá trị
    /// </summary>
    /// <param name="province">Tên tỉnh/thành phố</param>
    /// <param name="provinceCode">Mã tỉnh/thành phố</param>
    /// <param name="district">Tên quận/huyện</param>
    /// <param name="districtCode">Mã quận/huyện</param>
    /// <param name="commune">Tên phường/xã</param>
    /// <param name="communeCode">Mã phường/xã</param>
    /// <exception cref="ValidationException">Ném ra khi các cặp trường không đồng nhất về null</exception>
    public static void ValidateAddressFields(string? province, string? provinceCode, 
        string? district, string? districtCode, 
        string? commune, string? communeCode)
    {
        if ((province == null) != (provinceCode == null))
        {
            throw new ValidationException("Province và ProvinceCode phải cùng null hoặc cùng có giá trị.");
        }
        if ((district == null) != (districtCode == null))
        {
            throw new ValidationException("District và DistrictCode phải cùng null hoặc cùng có giá trị.");
        }
        if ((commune == null) != (communeCode == null))
        {
            throw new ValidationException("Commune và CommuneCode phải cùng null hoặc cùng có giá trị.");
        }
    }
}