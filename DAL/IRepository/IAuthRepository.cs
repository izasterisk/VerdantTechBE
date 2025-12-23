using DAL.Data.Models;

namespace DAL.IRepository;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task LogoutUserAsync(User user, CancellationToken cancellationToken = default);
    Task<Transaction?> ValidateVendorSubscriptionAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithFarmByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdateVendorProfileAsync(VendorProfile vendorProfile, CancellationToken cancellationToken = default);
    Task<VendorProfile> GetVendorProfileByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
}