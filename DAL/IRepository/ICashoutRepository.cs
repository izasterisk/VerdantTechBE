using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICashoutRepository
{
    Task<Cashout> CreateWalletCashoutAsync(Cashout cashout, Transaction tr, CancellationToken cancellationToken = default);
    Task<Transaction> CreateRefundCashoutWithTransactionAsync(Transaction tr, Cashout cashout, 
        UserBankAccount? bankAccount, Order order, Request request, List<ProductSerial> serials, 
        List<ExportInventory> exports, List<OrderDetail> orderDetails, CancellationToken cancellationToken = default);
    Task<Transaction> UpdateCashoutAsync(Cashout cashout, Transaction tr, CancellationToken cancellationToken = default);
    Task<Transaction> GetCashoutRequestWithRelationsByTransactionIdAsync(ulong transactionId, CancellationToken cancellationToken = default);
    Task<List<ProductSerial>> GetSoldProductSerialsBySerialNumbersAsync(Dictionary<string, string> serials, CancellationToken cancellationToken = default);
    Task<(Order, List<ExportInventory>)> ValidateExportedOrderByOrderDetailIdsAsync(
        Dictionary<(ulong OrderDetailId, string LotNumber), int> validateLotNumber, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRefundedAmountByOrderDetailIdsAsync(Dictionary<(ulong OrderDetailId, string LotNumber), int> validateLotNumber,
        CancellationToken cancellationToken = default);
    Task<List<ExportInventory>> GetAllExportInventoriesByOrderDetailIdsAsync(HashSet<ulong> orderDetailIds,
        CancellationToken cancellationToken = default);
    Task<(Order, List<OrderDetail>)> GetOrderAndChosenOrderDetailsById(List<ulong> orderDetailIds,
        CancellationToken cancellationToken = default);
}