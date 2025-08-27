using DAL.Data.Models;

namespace DAL.IRepository;

public interface ISupportedBanksRepository
{
    Task<SupportedBank> CreateSupportedBankWithTransactionAsync(SupportedBank supportedBank);
    Task<SupportedBank> UpdateSupportedBankWithTransactionAsync(SupportedBank supportedBank);
    Task<SupportedBank?> GetSupportedBankByIdAsync(ulong id);
    Task<SupportedBank?> GetSupportedBankByBankCodeAsync(String code);
    Task<bool> CheckBankCodeExistsAsync(string code);
    Task<(List<SupportedBank>, int totalCount)> GetAllSupportedBanksAsync(int page, int pageSize);
}