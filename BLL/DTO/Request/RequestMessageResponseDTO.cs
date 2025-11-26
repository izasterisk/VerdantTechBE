using BLL.DTO.User;

namespace BLL.DTO.Request;

public class RequestMessageResponseDTO
{
    public ulong Id { get; set; }

    // public ulong RequestId { get; set; }

    // public ulong? StaffId { get; set; }
    
    public string Description { get; set; } = null!;
    
    public UserResponseDTO? Staff { get; set; }

    public string? ReplyNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    
    public List<RequestImageDTO>? Images { get; set; }
}