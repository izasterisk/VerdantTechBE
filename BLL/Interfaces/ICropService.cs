using BLL.DTO.Crop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.IService
{
    public interface ICropService
    {
        Task<IEnumerable<CropResponseDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<CropResponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<IEnumerable<CropResponseDTO>> CreateAsync(CropCreateDTO dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(ulong id, CropUpdateDTO dto, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(ulong id, CancellationToken ct = default);
        Task<bool> HardDeleteAsync(ulong id, CancellationToken ct = default);
    }
}