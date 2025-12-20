using BLL.DTO.Dashboard;
using BLL.DTO.Dashboard.Dashboard;
using BLL.Interfaces;
using DAL.IRepository;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BLL.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IAdminDashboardRepository _repository;

    private static readonly string[] MonthNames = 
    {
        "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
        "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"
    };

    public AdminDashboardService(IAdminDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminOverviewDTO> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var (revToday, revThisWeek, revThisMonth, revLastMonth) = await _repository.GetRevenueOverviewAsync(cancellationToken);
        var (commThisMonth, commLastMonth) = await _repository.GetCommissionOverviewAsync(cancellationToken);
        var (ordToday, ordThisWeek, ordThisMonth, pendingShipment, inTransit) = await _repository.GetOrdersOverviewAsync(cancellationToken);
        var (totalCustomers, totalVendors, newCustomers, newVendors) = await _repository.GetUsersOverviewAsync(cancellationToken);
        var (prodActive, prodInactive, prodOutOfStock) = await _repository.GetProductsOverviewAsync(cancellationToken);
        var queues = await _repository.GetPendingQueuesOverviewAsync(cancellationToken);

        return new AdminOverviewDTO
        {
            Revenue = new AdminRevenueOverviewDTO
            {
                Today = revToday,
                ThisWeek = revThisWeek,
                ThisMonth = revThisMonth,
                LastMonth = revLastMonth,
                GrowthPercent = CalculateGrowthPercent(revThisMonth, revLastMonth)
            },
            Commission = new AdminCommissionOverviewDTO
            {
                ThisMonth = commThisMonth,
                LastMonth = commLastMonth,
                GrowthPercent = CalculateGrowthPercent(commThisMonth, commLastMonth)
            },
            Orders = new AdminOrdersOverviewDTO
            {
                Today = ordToday,
                ThisWeek = ordThisWeek,
                ThisMonth = ordThisMonth,
                PendingShipment = pendingShipment,
                InTransit = inTransit
            },
            Users = new AdminUsersOverviewDTO
            {
                TotalCustomers = totalCustomers,
                TotalVendors = totalVendors,
                NewCustomersThisMonth = newCustomers,
                NewVendorsThisMonth = newVendors
            },
            Products = new AdminProductsOverviewDTO
            {
                TotalActive = prodActive,
                TotalInactive = prodInactive,
                OutOfStock = prodOutOfStock
            },
            PendingQueues = new AdminPendingQueuesOverviewDTO
            {
                VendorProfiles = queues.vendorProfiles,
                ProductRegistrations = queues.productRegistrations,
                VendorCertificates = queues.vendorCertificates,
                ProductCertificates = queues.productCertificates,
                ProductUpdateRequests = queues.productUpdateRequests,
                SupportRequests = queues.supportRequests,
                RefundRequests = queues.refundRequests,
                CashoutRequests = queues.cashoutRequests
            }
        };
    }

    public async Task<AdminRevenueDTO> GetRevenueAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);

        var (totalRevenue, totalCommission, totalOrders, totalTransactions) = await _repository.GetRevenueByTimeRangeAsync(from, to, cancellationToken);
        var (bankingCount, bankingAmount, codCount, codAmount) = await _repository.GetPaymentMethodBreakdownAsync(from, to, cancellationToken);

        return new AdminRevenueDTO
        {
            From = from,
            To = to,
            TotalRevenue = totalRevenue,
            TotalCommission = totalCommission,
            TotalVendorPayout = totalRevenue - totalCommission,
            TotalOrders = totalOrders,
            TotalTransactions = totalTransactions,
            AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0,
            PaymentMethodBreakdown = new AdminPaymentMethodBreakdownDTO
            {
                Banking = new AdminPaymentMethodItemDTO { Count = bankingCount, Amount = bankingAmount },
                Cod = new AdminPaymentMethodItemDTO { Count = codCount, Amount = codAmount }
            }
        };
    }

    public async Task<AdminDailyRevenueDTO> GetDailyRevenueAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to, maxDays: 90);

        var dailyData = await _repository.GetDailyRevenueAsync(from, to, cancellationToken);

        return new AdminDailyRevenueDTO
        {
            From = from,
            To = to,
            DailyRevenues = dailyData.Select(d => new AdminDailyRevenueItemDTO
            {
                Date = d.date,
                Revenue = d.revenue,
                Commission = d.commission,
                OrderCount = d.orderCount,
                TransactionCount = d.transactionCount
            }).ToList(),
            TotalRevenue = dailyData.Sum(d => d.revenue),
            TotalCommission = dailyData.Sum(d => d.commission),
            TotalOrders = dailyData.Sum(d => d.orderCount)
        };
    }

    public async Task<AdminMonthlyRevenueDTO> GetMonthlyRevenueAsync(int year, CancellationToken cancellationToken = default)
    {
        if (year < 2020 || year > DateTime.UtcNow.Year + 1)
            throw new ArgumentException("Năm không hợp lệ.", nameof(year));

        var monthlyData = await _repository.GetMonthlyRevenueAsync(year, cancellationToken);

        return new AdminMonthlyRevenueDTO
        {
            Year = year,
            MonthlyRevenues = monthlyData.Select(m => new AdminMonthlyRevenueItemDTO
            {
                Month = m.month,
                MonthName = MonthNames[m.month - 1],
                Revenue = m.revenue,
                Commission = m.commission,
                OrderCount = m.orderCount
            }).ToList(),
            TotalRevenue = monthlyData.Sum(m => m.revenue),
            TotalCommission = monthlyData.Sum(m => m.commission),
            TotalOrders = monthlyData.Sum(m => m.orderCount)
        };
    }

    public async Task<AdminOrderStatisticsDTO> GetOrderStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);

        var statusCounts = await _repository.GetOrderCountsByStatusAsync(from, to, cancellationToken);
        var (bankingCount, codCount) = await _repository.GetOrderCountsByPaymentMethodAsync(from, to, cancellationToken);
        var courierCounts = await _repository.GetOrderCountsByCourierAsync(from, to, cancellationToken);
        var avgDeliveryDays = await _repository.GetAverageDeliveryDaysAsync(from, to, cancellationToken);

        var total = statusCounts.Values.Sum();
        var delivered = statusCounts.GetValueOrDefault("Delivered");
        var cancelled = statusCounts.GetValueOrDefault("Cancelled");
        var refunded = statusCounts.GetValueOrDefault("Refunded") + statusCounts.GetValueOrDefault("PartialRefund");

        return new AdminOrderStatisticsDTO
        {
            From = from,
            To = to,
            TotalOrders = total,
            OrdersByStatus = new AdminOrdersByStatusDTO
            {
                Pending = statusCounts.GetValueOrDefault("Pending"),
                Processing = statusCounts.GetValueOrDefault("Processing"),
                Paid = statusCounts.GetValueOrDefault("Paid"),
                Shipped = statusCounts.GetValueOrDefault("Shipped"),
                Delivered = delivered,
                Cancelled = cancelled,
                Refunded = statusCounts.GetValueOrDefault("Refunded"),
                PartialRefund = statusCounts.GetValueOrDefault("PartialRefund")
            },
            FulfillmentRate = total > 0 ? Math.Round((decimal)delivered / total * 100, 2) : 0,
            CancellationRate = total > 0 ? Math.Round((decimal)cancelled / total * 100, 2) : 0,
            RefundRate = total > 0 ? Math.Round((decimal)refunded / total * 100, 2) : 0,
            AverageDeliveryDays = Math.Round(avgDeliveryDays, 2),
            OrdersByPaymentMethod = new AdminOrdersByPaymentMethodDTO
            {
                Banking = bankingCount,
                Cod = codCount
            },
            OrdersByCourier = courierCounts.Select(c => new AdminOrdersByCourierDTO
            {
                CourierId = c.courierId,
                CourierName = $"Courier #{c.courierId}",
                OrderCount = c.orderCount,
                Percentage = total > 0 ? Math.Round((decimal)c.orderCount / total * 100, 2) : 0
            }).ToList()
        };
    }

    public async Task<AdminUserStatisticsDTO> GetUserStatisticsAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = from ?? today.AddDays(-30);
        var toDate = to ?? today;

        ValidateDateRange(fromDate, toDate);

        var (custTotal, custActive, custInactive, custNew) = await _repository.GetCustomerStatisticsAsync(fromDate, toDate, cancellationToken);
        var (vendorTotal, vendorVerified, vendorPending, vendorNew) = await _repository.GetVendorStatisticsAsync(fromDate, toDate, cancellationToken);
        var (staffTotal, staffActive) = await _repository.GetStaffStatisticsAsync(cancellationToken);
        var registrationTrend = await _repository.GetRegistrationTrendAsync(fromDate, toDate, cancellationToken);

        // Calculate growth for previous period
        var previousPeriodDays = (toDate.ToDateTime(TimeOnly.MinValue) - fromDate.ToDateTime(TimeOnly.MinValue)).Days;
        var previousFrom = fromDate.AddDays(-previousPeriodDays);
        var (_, _, _, prevCustNew) = await _repository.GetCustomerStatisticsAsync(previousFrom, fromDate.AddDays(-1), cancellationToken);

        return new AdminUserStatisticsDTO
        {
            From = fromDate,
            To = toDate,
            Customers = new AdminCustomerStatisticsDTO
            {
                Total = custTotal,
                Active = custActive,
                Inactive = custInactive,
                NewThisPeriod = custNew,
                GrowthPercent = CalculateGrowthPercent(custNew, prevCustNew)
            },
            Vendors = new AdminVendorUserStatisticsDTO
            {
                Total = vendorTotal,
                Verified = vendorVerified,
                PendingVerification = vendorPending,
                NewThisPeriod = vendorNew
            },
            Staff = new AdminStaffStatisticsDTO
            {
                Total = staffTotal,
                Active = staffActive
            },
            RegistrationTrend = registrationTrend.Select(r => new AdminRegistrationTrendDTO
            {
                Date = r.date,
                Customers = r.customers,
                Vendors = r.vendors
            }).ToList()
        };
    }

    public async Task<AdminProductStatisticsDTO> GetProductStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var (total, active, inactive, outOfStock, lowStock) = await _repository.GetProductStatisticsAsync(cancellationToken);
        var categoryDist = await _repository.GetCategoryDistributionAsync(cancellationToken);
        var (vendorsWithProducts, avgProductsPerVendor, topVendorProductCount) = await _repository.GetVendorProductDistributionAsync(cancellationToken);

        return new AdminProductStatisticsDTO
        {
            TotalProducts = total,
            ActiveProducts = active,
            InactiveProducts = inactive,
            OutOfStockProducts = outOfStock,
            LowStockProducts = lowStock,
            CategoryDistribution = categoryDist.Select(c => new AdminCategoryDistributionDTO
            {
                CategoryId = c.categoryId,
                CategoryName = c.categoryName,
                ProductCount = c.productCount,
                Percentage = total > 0 ? Math.Round((decimal)c.productCount / total * 100, 2) : 0,
                ActiveCount = c.activeCount,
                OutOfStockCount = c.outOfStockCount
            }).ToList(),
            VendorDistribution = new AdminVendorProductDistributionDTO
            {
                VendorsWithProducts = vendorsWithProducts,
                AverageProductsPerVendor = Math.Round(avgProductsPerVendor, 2),
                TopVendorProductCount = topVendorProductCount
            }
        };
    }

    public async Task<AdminBestSellingProductsDTO> GetBestSellingProductsAsync(DateOnly? from, DateOnly? to, int limit, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = from ?? today.AddDays(-30);
        var toDate = to ?? today;
        limit = Math.Clamp(limit, 1, 50);

        ValidateDateRange(fromDate, toDate);

        var products = await _repository.GetBestSellingProductsAsync(fromDate, toDate, limit, cancellationToken);

        return new AdminBestSellingProductsDTO
        {
            From = fromDate,
            To = toDate,
            Products = products.Select((p, index) => new AdminBestSellingProductItemDTO
            {
                Rank = index + 1,
                ProductId = p.product.Id,
                ProductCode = p.product.ProductCode,
                ProductName = p.product.ProductName,
                VendorId = p.product.VendorId,
                VendorName = p.vendorName,
                ImageUrl = p.imageUrl,
                UnitPrice = p.product.UnitPrice,
                SoldQuantity = p.soldQuantity,
                TotalRevenue = p.totalRevenue,
                CommissionEarned = p.commission,
                StockQuantity = p.product.StockQuantity,
                RatingAverage = p.product.RatingAverage
            }).ToList()
        };
    }

    public async Task<AdminVendorStatisticsDTO> GetVendorStatisticsAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = from ?? today.AddDays(-30);
        var toDate = to ?? today;

        ValidateDateRange(fromDate, toDate);

        var (total, verified, pending, active, inactive, suspended) = await _repository.GetVendorCountsAsync(cancellationToken);
        var (_, totalCommission, _, _) = await _repository.GetRevenueByTimeRangeAsync(fromDate, toDate, cancellationToken);
        var totalVendorRevenue = await _repository.GetTotalVendorRevenueAsync(fromDate, toDate, cancellationToken);

        return new AdminVendorStatisticsDTO
        {
            From = fromDate,
            To = toDate,
            TotalVendors = total,
            VerifiedVendors = verified,
            PendingVerification = pending,
            VendorsByStatus = new AdminVendorsByStatusDTO
            {
                Active = active,
                Inactive = inactive,
                Suspended = suspended
            },
            TotalVendorRevenue = totalVendorRevenue,
            TotalCommissionCollected = totalCommission,
            AverageRevenuePerVendor = verified > 0 ? Math.Round(totalVendorRevenue / verified, 2) : 0
        };
    }

    public async Task<AdminTopVendorsDTO> GetTopVendorsAsync(DateOnly? from, DateOnly? to, int limit, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = from ?? today.AddDays(-30);
        var toDate = to ?? today;
        limit = Math.Clamp(limit, 1, 20);

        ValidateDateRange(fromDate, toDate);

        var vendors = await _repository.GetTopVendorsAsync(fromDate, toDate, limit, cancellationToken);

        return new AdminTopVendorsDTO
        {
            From = fromDate,
            To = toDate,
            Vendors = vendors.Select((v, index) => new AdminTopVendorItemDTO
            {
                Rank = index + 1,
                VendorId = v.vendor.UserId,
                CompanyName = v.vendor.CompanyName,
                Slug = v.vendor.Slug,
                VerifiedAt = v.vendor.VerifiedAt,
                GrossRevenue = v.grossRevenue,
                NetRevenue = v.grossRevenue - v.commission,
                CommissionPaid = v.commission,
                OrderCount = v.orderCount,
                ProductCount = v.productCount,
                AverageRating = Math.Round(v.avgRating, 2),
                WalletBalance = v.walletBalance
            }).ToList()
        };
    }

    public async Task<AdminTransactionStatisticsDTO> GetTransactionStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);

        var (totalInflow, totalOutflow) = await _repository.GetTransactionFlowAsync(from, to, cancellationToken);
        var paymentInStats = await _repository.GetPaymentInStatisticsAsync(from, to, cancellationToken);
        var walletTopupStats = await _repository.GetWalletTopupStatisticsAsync(from, to, cancellationToken);
        var walletCashoutStats = await _repository.GetWalletCashoutStatisticsAsync(from, to, cancellationToken);
        var refundStats = await _repository.GetRefundStatisticsAsync(from, to, cancellationToken);
        var dailyTrend = await _repository.GetDailyTransactionTrendAsync(from, to, cancellationToken);

        return new AdminTransactionStatisticsDTO
        {
            From = from,
            To = to,
            Summary = new AdminTransactionSummaryDTO
            {
                TotalInflow = totalInflow,
                TotalOutflow = totalOutflow,
                NetFlow = totalInflow - totalOutflow
            },
            ByType = new AdminTransactionByTypeDTO
            {
                PaymentIn = new AdminPaymentInTransactionsDTO
                {
                    Count = paymentInStats.count,
                    CompletedAmount = paymentInStats.completedAmount,
                    PendingAmount = paymentInStats.pendingAmount,
                    FailedCount = paymentInStats.failedCount
                },
                WalletTopup = new AdminWalletTopupTransactionsDTO
                {
                    Count = walletTopupStats.count,
                    CompletedAmount = walletTopupStats.completedAmount
                },
                WalletCashout = new AdminWalletCashoutTransactionsDTO
                {
                    Count = walletCashoutStats.count,
                    CompletedAmount = walletCashoutStats.completedAmount,
                    PendingCount = walletCashoutStats.pendingCount,
                    PendingAmount = walletCashoutStats.pendingAmount
                },
                Refund = new AdminRefundTransactionsDTO
                {
                    Count = refundStats.count,
                    CompletedAmount = refundStats.completedAmount,
                    PendingCount = refundStats.pendingCount,
                    PendingAmount = refundStats.pendingAmount
                }
            },
            DailyTrend = dailyTrend.Select(d => new AdminDailyTransactionTrendDTO
            {
                Date = d.date,
                Inflow = d.inflow,
                Outflow = d.outflow,
                TransactionCount = d.count
            }).ToList()
        };
    }

    public async Task<AdminQueueStatisticsDTO> GetQueueStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var vendorProfileQueue = await _repository.GetVendorProfileQueueAsync(cancellationToken);
        var productRegQueue = await _repository.GetProductRegistrationQueueAsync(cancellationToken);
        var productUpdateQueue = await _repository.GetProductUpdateRequestQueueAsync(cancellationToken);
        var vendorCertQueue = await _repository.GetVendorCertificateQueueAsync(cancellationToken);
        var productCertQueue = await _repository.GetProductCertificateQueueAsync(cancellationToken);
        var supportQueue = await _repository.GetSupportRequestQueueAsync(cancellationToken);
        var refundQueue = await _repository.GetRefundRequestQueueAsync(cancellationToken);
        var cashoutQueue = await _repository.GetCashoutRequestQueueAsync(cancellationToken);

        return new AdminQueueStatisticsDTO
        {
            VendorProfiles = new AdminQueueItemDTO
            {
                PendingCount = vendorProfileQueue.pendingCount,
                OldestPendingDate = vendorProfileQueue.oldestDate,
                AverageWaitDays = vendorProfileQueue.avgWaitDays
            },
            ProductRegistrations = new AdminProductRegistrationQueueDTO
            {
                PendingCount = productRegQueue.pendingCount,
                OldestPendingDate = productRegQueue.oldestDate,
                AverageWaitDays = productRegQueue.avgWaitDays,
                ByVendor = productRegQueue.byVendor.Select(v => new AdminQueueByVendorDTO
                {
                    VendorId = v.vendorId,
                    VendorName = v.vendorName,
                    Count = v.count
                }).ToList()
            },
            ProductUpdateRequests = new AdminQueueItemDTO
            {
                PendingCount = productUpdateQueue.pendingCount,
                OldestPendingDate = productUpdateQueue.oldestDate,
                AverageWaitDays = productUpdateQueue.avgWaitDays
            },
            VendorCertificates = new AdminQueueItemDTO
            {
                PendingCount = vendorCertQueue.pendingCount,
                OldestPendingDate = vendorCertQueue.oldestDate,
                AverageWaitDays = vendorCertQueue.avgWaitDays
            },
            ProductCertificates = new AdminQueueItemDTO
            {
                PendingCount = productCertQueue.pendingCount,
                OldestPendingDate = productCertQueue.oldestDate,
                AverageWaitDays = productCertQueue.avgWaitDays
            },
            SupportRequests = new AdminRequestQueueDTO
            {
                PendingCount = supportQueue.pendingCount,
                InReviewCount = supportQueue.inReviewCount,
                OldestPendingDate = supportQueue.oldestDate,
                AverageWaitDays = supportQueue.avgWaitDays
            },
            RefundRequests = new AdminRefundQueueDTO
            {
                PendingCount = refundQueue.pendingCount,
                InReviewCount = refundQueue.inReviewCount,
                TotalPendingAmount = refundQueue.totalAmount,
                OldestPendingDate = refundQueue.oldestDate,
                AverageWaitDays = refundQueue.avgWaitDays
            },
            CashoutRequests = new AdminCashoutQueueDTO
            {
                PendingCount = cashoutQueue.pendingCount,
                TotalPendingAmount = cashoutQueue.totalAmount,
                OldestPendingDate = cashoutQueue.oldestDate,
                AverageWaitDays = cashoutQueue.avgWaitDays
            }
        };
    }

    public async Task<byte[]> ExportTransactionHistoryAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        var transactions = await _repository.GetAllTransactionsInTimeRangeAsync(from, to, cancellationToken);
        var dtos = new List<TransactionExportDTO>();
        foreach (var tx in transactions)
        {
            dtos.Add(new TransactionExportDTO
            {
                TransactionId = tx.Id,
                TransactionType = tx.TransactionType.ToString(),
                Amount = tx.Amount,
                Status = tx.Status.ToString(),
                CreatedAt = tx.CreatedAt,
                Description = tx.Note
            });
        }
        
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Lịch sử giao dịch");
        var headers = new[] { "ID Giao Dịch", "Ngày tạo", "Loại giao dịch", "Số tiền (VNĐ)", "Trạng thái", "Mã tham chiếu", "Mô tả" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
        }
        using (var range = worksheet.Cells[1, 1, 1, headers.Length])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }
        int row = 2;
        foreach (var trans in dtos)
        {
            worksheet.Cells[row, 1].Value = trans.TransactionId;
        
            worksheet.Cells[row, 2].Value = trans.CreatedAt;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

            worksheet.Cells[row, 3].Value = trans.TransactionType;
        
            worksheet.Cells[row, 4].Value = trans.Amount;
            worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0"; // Format tiền tệ

            worksheet.Cells[row, 5].Value = trans.Status;
        
            // Tô màu trạng thái
            if (trans.Status == "Completed" || trans.Status == "Success") 
                worksheet.Cells[row, 5].Style.Font.Color.SetColor(System.Drawing.Color.Green);
            else if (trans.Status == "Failed" || trans.Status == "Cancelled") 
                worksheet.Cells[row, 5].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            worksheet.Cells[row, 6].Value = trans.ReferenceCode;
            worksheet.Cells[row, 7].Value = trans.Description;

            row++;
        }

        // --- Footer (Tổng cộng) ---
        worksheet.Cells[row, 3].Value = "TỔNG CỘNG:";
        worksheet.Cells[row, 3].Style.Font.Bold = true;
    
        // Dùng công thức Excel để tính tổng cột Số tiền (Cột D là cột 4)
        worksheet.Cells[row, 4].Formula = $"SUM(D2:D{row - 1})";
        worksheet.Cells[row, 4].Style.Font.Bold = true;
        worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0";

        // AutoFit các cột cho đẹp
        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    #region Private Helpers

    private static void ValidateDateRange(DateOnly from, DateOnly to, int maxDays = 365)
    {
        if (from > to)
            throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");

        if ((to.ToDateTime(TimeOnly.MinValue) - from.ToDateTime(TimeOnly.MinValue)).Days > maxDays)
            throw new ArgumentException($"Khoảng thời gian không được vượt quá {maxDays} ngày.");
    }

    private static decimal CalculateGrowthPercent(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((current - previous) / previous * 100, 2);
    }

    #endregion
}

