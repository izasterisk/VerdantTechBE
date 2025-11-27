namespace BLL.DTO.ExportInventory;

public class IdentityNumberDTO
{
    public List<LotNumberResponseDTO>? LotNumberInfo { get; set; }
    public List<SerialNumberResponseDTO>? SerialNumberInfo { get; set; }
}

public class LotNumberResponseDTO
{
    public string LotNumber { get; set; } = null!;
    public int Quantity { get; set; }
}

public class SerialNumberResponseDTO
{
    public string LotNumber { get; set; } = null!;
    public string SerialNumber { get; set; } = null!;
}