using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.Interfaces;
using BLL.DTO;

namespace Controller.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Lấy doanh thu theo khoảng thời gian
    /// </summary>
    /// <param name="from">Ngày bắt đầu (yyyy-MM-dd)</param>
    /// <param name="to">Ngày kết thúc (yyyy-MM-dd)</param>
    /// <returns>Tổng doanh thu trong khoảng thời gian</returns>
    [HttpGet("revenue")]
    [EndpointSummary("Get Revenue By Time Range")]
    [EndpointDescription("Lấy tổng doanh thu từ các giao dịch PaymentIn đã hoàn thành trong khoảng thời gian chỉ định.")]
    public async Task<ActionResult<APIResponse>> GetRevenueByTimeRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var result = await _dashboardService.GetRevenueByTimeRangeAsync(from, to, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thống kê đơn hàng theo khoảng thời gian
    /// </summary>
    /// <param name="from">Ngày bắt đầu (yyyy-MM-dd)</param>
    /// <param name="to">Ngày kết thúc (yyyy-MM-dd)</param>
    /// <returns>Thống kê số lượng đơn hàng theo trạng thái</returns>
    [HttpGet("orders")]
    [EndpointSummary("Get Order Statistics By Time Range")]
    [EndpointDescription("Lấy thống kê số lượng đơn hàng theo trạng thái (Paid, Shipped, Cancelled, Delivered, Refunded) trong khoảng thời gian chỉ định.")]
    public async Task<ActionResult<APIResponse>> GetOrderStatisticsByTimeRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var result = await _dashboardService.GetOrderStatisticsByTimeRangeAsync(from, to, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy số lượng items đang chờ xử lý trong các hàng đợi
    /// </summary>
    /// <returns>Thống kê số lượng pending items</returns>
    [HttpGet("queues")]
    [EndpointSummary("Get Queue Statistics")]
    [EndpointDescription("Lấy số lượng items đang chờ xử lý: Vendor chưa verify, ProductRegistration/VendorCertificate/ProductCertificate/Request đang Pending.")]
    public async Task<ActionResult<APIResponse>> GetQueueStatistics()
    {
        try
        {
            var result = await _dashboardService.GetQueueStatisticsAsync(GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy doanh thu 7 ngày gần nhất
    /// </summary>
    /// <returns>Doanh thu theo từng ngày trong 7 ngày gần nhất</returns>
    [HttpGet("revenue/last-7-days")]
    [EndpointSummary("Get Revenue Last 7 Days")]
    [EndpointDescription("Lấy doanh thu chi tiết theo từng ngày trong 7 ngày gần nhất (bao gồm hôm nay).")]
    public async Task<ActionResult<APIResponse>> GetRevenueLast7Days()
    {
        try
        {
            var result = await _dashboardService.GetRevenueLast7DaysAsync(GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}

