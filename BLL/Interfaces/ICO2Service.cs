namespace BLL.Interfaces;

public interface ICO2Service
{
    Task SaveSoilDataByFarmIdAsync(ulong farmId, ulong customerId, DateOnly measurementStartDate, DateOnly measurementEndDate, CancellationToken cancellationToken = default);
}