using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class CashoutRepository : ICashoutRepository
{
    private readonly IRepository<Cashout> _cashoutRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public CashoutRepository(VerdantTechDbContext context, IRepository<Cashout> cashoutRepository)
    {
        _dbContext = context;
        _cashoutRepository = cashoutRepository;
    }

    public async Task<Cashout> CreateCashoutAsync(Cashout cashout, CancellationToken cancellationToken = default)
    {
        cashout.CreatedAt = DateTime.UtcNow;
        cashout.UpdatedAt = DateTime.UtcNow;
        return await _cashoutRepository.CreateAsync(cashout, cancellationToken);
    }
}