using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly IRepository<User> _userRepository;
    private readonly VerdantTechDbContext _context;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<VendorProfile> _vendorProfileRepository;
    
    public AuthRepository(IRepository<User> userRepository, VerdantTechDbContext context,
        IRepository<Transaction> transactionRepository, IRepository<VendorProfile> vendorProfileRepository)
    {
        _userRepository = userRepository;
        _context = context;
        _transactionRepository = transactionRepository;
        _vendorProfileRepository = vendorProfileRepository;
    }
    
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase),true, 
            cancellationToken: cancellationToken);
    }
    
    public async Task<User?> GetUserWithFarmByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetWithRelationsAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase),true, 
            query => query.Include(u => u.FarmProfiles)
                .ThenInclude(f => f.Address),
            cancellationToken: cancellationToken);
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetAsync(u => u.RefreshToken == refreshToken 
                                                   && u.RefreshTokenExpiresAt > DateTime.UtcNow
                                                   && u.DeletedAt == null && u.Status != UserStatus.Deleted, cancellationToken: cancellationToken);
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task LogoutUserAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
            
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<Transaction?> ValidateVendorSubscriptionAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.UserId == vendorId && t.TransactionType == TransactionType.VendorSubscription
                        && t.Status == TransactionStatus.Completed)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task UpdateVendorProfileAsync(VendorProfile vendorProfile, CancellationToken cancellationToken = default)
    {
        vendorProfile.UpdatedAt = DateTime.UtcNow;
        await _vendorProfileRepository.UpdateAsync(vendorProfile, cancellationToken);
    }
    
    public async Task<VendorProfile> GetVendorProfileByUserIdAsync(ulong userId, CancellationToken cancellationToken = default) =>
        await _vendorProfileRepository.GetAsync(vp => vp.UserId == userId, true, cancellationToken)
        ?? throw new InvalidOperationException("Tài khoản thương nhân không tồn tại.");
}