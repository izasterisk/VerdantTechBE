using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class SupportedBanksRepository : ISupportedBanksRepository
{
    private readonly IRepository<SupportedBank> _supportedBanksRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public SupportedBanksRepository(VerdantTechDbContext context)
    {
        _supportedBanksRepository = new Repository<SupportedBank>(context);
        _dbContext = context;
    }

    public async Task<SupportedBank> CreateSupportedBankWithTransactionAsync(SupportedBank supportedBank)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            supportedBank.IsActive = true;
            supportedBank.CreatedAt = DateTime.Now;
            supportedBank.UpdatedAt = DateTime.Now;
            var createdSupportedBank = await _supportedBanksRepository.CreateAsync(supportedBank);
            await transaction.CommitAsync();
            return createdSupportedBank;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<SupportedBank> UpdateSupportedBankWithTransactionAsync(SupportedBank supportedBank)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            supportedBank.UpdatedAt = DateTime.Now;
            var updatedSupportedBank = await _supportedBanksRepository.UpdateAsync(supportedBank);
            await transaction.CommitAsync();
            return updatedSupportedBank;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<bool> CheckBankCodeExistsAsync(string code)
    {
        return await _supportedBanksRepository.AnyAsync(sb => sb.BankCode.ToUpper() == code.ToUpper());
    }
    
    public async Task<SupportedBank?> GetSupportedBankByIdAsync(ulong id)
    {
        return await _supportedBanksRepository.GetAsync(sb => sb.Id == id, useNoTracking: true);
    }
    
    public async Task<(List<SupportedBank>, int totalCount)> GetAllSupportedBanksAsync(int page, int pageSize)
    {
        return await _supportedBanksRepository.GetPaginatedAsync(page, pageSize, useNoTracking: true);
    }
    
    public async Task<SupportedBank?> GetSupportedBankByBankCodeAsync(String code)
    {
        return await _supportedBanksRepository.GetAsync(sb => sb.BankCode == code, useNoTracking: true);
    }
}