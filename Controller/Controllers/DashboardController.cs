using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
[ApiController]
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
    /// <param name="from">Ngày bắt đầu (YYYY-MM-DD)</param>
    /// <param name="to">Ngày kết thúc (YYYY-MM-DD)</param>
    /// <returns>Doanh thu theo khoảng thời gian</returns>
    [HttpGet("revenue")]
    [Authorize(Roles = "Admin,Vendor")]
    [EndpointSummary("Get Revenue By Time Range")]
    [EndpointDescription("Có thể sử dụng cho cả Admin và Vendor. Admin sẽ thấy tổng doanh thu toàn hệ thống, Vendor chỉ thấy doanh thu của mình.")]
    public async Task<ActionResult<APIResponse>> GetRevenueByTimeRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var vendorId = GetCurrentUserId();
            var revenue = await _dashboardService.GetRevenueByTimeRangeAsync(vendorId, from, to, GetCancellationToken());
            return SuccessResponse(revenue);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy top 5 sản phẩm bán chạy nhất theo khoảng thời gian
    /// </summary>
    /// <param name="from">Ngày bắt đầu (YYYY-MM-DD)</param>
    /// <param name="to">Ngày kết thúc (YYYY-MM-DD)</param>
    /// <returns>Top 5 sản phẩm bán chạy nhất</returns>
    [HttpGet("best-selling-products")]
    [Authorize(Roles = "Admin,Vendor")]
    [EndpointSummary("Get Top 5 Best Selling Products By Time Range")]
    [EndpointDescription("Có thể sử dụng cho cả Admin và Vendor. Admin sẽ thấy top 5 toàn hệ thống, Vendor chỉ thấy sản phẩm của mình.")]
    public async Task<ActionResult<APIResponse>> GetTop5BestSellingProductsByTimeRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var vendorId = GetCurrentUserId();
            var products = await _dashboardService.GetTop5BestSellingProductsByTimeRangeAsync(vendorId, from, to, GetCancellationToken());
            return SuccessResponse(products);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thống kê đơn hàng theo khoảng thời gian
    /// </summary>
    /// <param name="from">Ngày bắt đầu (YYYY-MM-DD)</param>
    /// <param name="to">Ngày kết thúc (YYYY-MM-DD)</param>
    /// <returns>Thống kê đơn hàng theo trạng thái</returns>
    [HttpGet("orders/statistics")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Get Order Statistics By Time Range")]
    [EndpointDescription("Lấy thống kê số lượng đơn hàng theo từng trạng thái trong khoảng thời gian. Chỉ Admin có quyền truy cập.")]
    public async Task<ActionResult<APIResponse>> GetOrderStatisticsByTimeRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var statistics = await _dashboardService.GetOrderStatisticsByTimeRangeAsync(from, to, GetCancellationToken());
            return SuccessResponse(statistics);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy số lượng các yêu cầu đang chờ xử lý
    /// </summary>
    /// <returns>Thống kê số lượng yêu cầu chờ duyệt</returns>
    [HttpGet("queues/statistics")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Get Queue Statistics")]
    [EndpointDescription("Lấy số lượng các yêu cầu đang chờ xử lý (VendorProfile, ProductRegistration, VendorCertificate, ProductCertificate, Request). Chỉ Admin có quyền truy cập.")]
    public async Task<ActionResult<APIResponse>> GetQueueStatistics()
    {
        try
        {
            var statistics = await _dashboardService.GetQueueStatisticsAsync(GetCancellationToken());
            return SuccessResponse(statistics);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy doanh thu 7 ngày gần nhất
    /// </summary>
    /// <returns>Doanh thu từng ngày trong 7 ngày gần nhất</returns>
    [HttpGet("revenue/last-7-days")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Get Revenue Last 7 Days")]
    [EndpointDescription("Lấy doanh thu chi tiết theo từng ngày trong 7 ngày gần nhất. Chỉ Admin có quyền truy cập.")]
    public async Task<ActionResult<APIResponse>> GetRevenueLast7Days()
    {
        try
        {
            var revenue = await _dashboardService.GetRevenueLast7DaysAsync(GetCancellationToken());
            return SuccessResponse(revenue);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
