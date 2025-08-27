using BLL.DTO;
using BLL.DTO.SupportedBanks;

namespace BLL.Interfaces;

public interface ISupportedBanksService
{
    Task<SupportedBanksReadOnlyDTO> CreateSupportedBankAsync(SupportedBanksCreateDTO dto);
    Task<SupportedBanksReadOnlyDTO> UpdateSupportedBankAsync(ulong id, SupportedBanksUpdateDTO dto);
    Task<SupportedBanksReadOnlyDTO?> GetSupportedBankByIdAsync(ulong id);
    Task<SupportedBanksReadOnlyDTO?> GetSupportedBankByBankCodeAsync(string code);
    Task<PagedResponse<SupportedBanksReadOnlyDTO>> GetAllSupportedBanksAsync(int page, int pageSize);
}