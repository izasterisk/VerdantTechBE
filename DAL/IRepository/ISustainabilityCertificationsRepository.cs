using DAL.Data;
using DAL.Data.Models;

namespace DAL.IRepository;

public interface ISustainabilityCertificationsRepository
{
    Task<SustainabilityCertification> CreateSustainabilityCertificationWithTransactionAsync(SustainabilityCertification sustainabilityCertification);
    Task<SustainabilityCertification> UpdateSustainabilityCertificationWithTransactionAsync(SustainabilityCertification sustainabilityCertification);
    Task<SustainabilityCertification?> GetSustainabilityCertificationByIdAsync(ulong id);
    Task<(List<SustainabilityCertification> sustainabilityCertifications, int totalCount)> GetAllSustainabilityCertificationsAsync(int page, int pageSize, String? category = null);
    Task<List<SustainabilityCertificationCategory>> GetAllCategoriesAsync();
    Task<bool> CheckCodeExistsAsync(string code);
}