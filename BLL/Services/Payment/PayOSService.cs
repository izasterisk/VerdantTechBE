using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;

namespace BLL.Services.Payment;

public class PayOSService : IPayOSService
{
    private readonly IPayOSApiClient _payOSApiClient;

    public PayOSService(IPayOSApiClient PayOSApiClient)
    {
        _payOSApiClient = PayOSApiClient;
    }
}