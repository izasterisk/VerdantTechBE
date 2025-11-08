using AutoMapper;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.IRepository;

namespace BLL.Services;

public class CashoutService : ICashoutService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;
    private readonly IPayOSApiClient _payOSApiClient;
    private readonly ICashoutRepository _cashoutRepository;
    
    public CashoutService(IWalletRepository walletRepository, IMapper mapper, IPayOSApiClient payOSApiClient, ICashoutRepository cashoutRepository)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
        _payOSApiClient = payOSApiClient;
        _cashoutRepository = cashoutRepository;
    }

    public async Task<(string IPv4, string IPv6)> GetIPAddressAsync(CancellationToken cancellationToken = default)
    {
        return await _payOSApiClient.GetIPAddressAsync(cancellationToken);
    }
}