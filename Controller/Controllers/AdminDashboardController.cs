using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.Interfaces;
using BLL.DTO;

namespace Controller.Controllers;

[Route("api/admin-dashboard")]
[ApiController]
[Authorize(Roles = "Admin,Staff")]
public class AdminDashboardController : BaseController
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminDashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Lấy thống kê tổng quan toàn hệ thống
    /// </summary>
    [HttpGet("overview")]
    [EndpointSummary("Get Admin Overview")]
    [EndpointDescription("Lấy thống kê tổng quan toàn hệ thống: doanh thu, hoa hồng, đơn hàng, người dùng, sản phẩm, pending queues.")]
    public async Task<ActionResult<APIResponse>> GetOverview()
    {
        try
        {
            var result = await _dashboardService.GetOverviewAsync(GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy doanh thu hệ thống theo khoảng thời gian
    /// </summary>
    [HttpGet("revenue")]
    [EndpointSummary("Get System Revenue")]
    [EndpointDescription("Lấy doanh thu toàn hệ thống theo khoảng thời gian: tổng revenue, commission, phương thức thanh toán.")]
    public async Task<ActionResult<APIResponse>> GetRevenue([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var result = await _dashboardService.GetRevenueAsync(from, to, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy doanh thu theo ngày cho biểu đồ
    /// </summary>
    [HttpGet("revenue/daily")]
    [EndpointSummary("Get Daily Revenue")]
    [EndpointDescription("Lấy doanh thu hệ thống theo ngày trong khoảng thời gian (tối đa 90 ngày) cho biểu đồ.")]
    public async Task<ActionResult<APIResponse>> GetDailyRevenue([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var result = await _dashboardService.GetDailyRevenueAsync(from, to, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy doanh thu theo tháng trong năm
    /// </summary>
    [HttpGet("revenue/monthly")]
    [EndpointSummary("Get Monthly Revenue")]
    [EndpointDescription("Lấy doanh thu hệ thống theo tháng trong năm cho biểu đồ yearly.")]
    public async Task<ActionResult<APIResponse>> GetMonthlyRevenue([FromQuery] int year)
    {
        try
        {
            var result = await _dashboardService.GetMonthlyRevenueAsync(year, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thống kê đơn hàng toàn hệ thống
    /// </summary>
    [HttpGet("orders/statistics")]
    [EndpointSummary("Get Order Statistics")]
    [EndpointDescription("Thống kê đơn hàng toàn hệ thống: theo trạng thái, phương thức thanh toán, đơn vị vận chuyển.")]
    public async Task<ActionResult<APIResponse>> GetOrderStatistics([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var result = await _dashboardService.GetOrderStatisticsAsync(from, to, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thống kê người dùng
    /// </summary>
    [HttpGet("users/statistics")]
    [EndpointSummary("Get User Statistics")]
    [EndpointDescription("Thống kê người dùng: customers, vendors, staff, xu hướng đăng ký.")]
    public async Task<ActionResult<APIResponse>> GetUserStatistics(
        [FromQuery] DateOnly? from = null, 
        [FromQuery] DateOnly? to = null)
    {
        try
        {
            var result = await _dashboardService.GetUserStatisticsAsync(from, to, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thống kê sản phẩm toàn hệ thống
    /// </summary>
    [HttpGet("products/statistics")]
    [EndpointSummary("Get Product Statistics")]
    [EndpointDescription("Thống kê sản phẩm toàn hệ thống: active, inactive, hết hàng, phân bố danh mục, vendor.")]
    public async Task<ActionResult<APIResponse>> GetProductStatistics()
    {
        try
        {
            var result = await _dashboardService.GetProductStatisticsAsync(GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy top sản phẩm bán chạy toàn hệ thống
    /// </summary>
    [HttpGet("products/best-selling")]
    [EndpointSummary("Get Best Selling Products")]
    [EndpointDescription("Top sản phẩm bán chạy nhất toàn hệ thống trong khoảng thời gian (kèm thông tin vendor).")]
    public async Task<ActionResult<APIResponse>> GetBestSellingProducts(
        [FromQuery] DateOnly? from = null, 
        [FromQuery] DateOnly? to = null, 
        [FromQuery] int limit = 10)
    {
        try
        {
            var result = await _dashboardService.GetBestSellingProductsAsync(from, to, limit, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thống kê vendors
    /// </summary>
    [HttpGet("vendors/statistics")]
    [EndpointSummary("Get Vendor Statistics")]
    [EndpointDescription("Thống kê chi tiết về vendors: tổng số, verified, pending, doanh thu, hoa hồng.")]
    public async Task<ActionResult<APIResponse>> GetVendorStatistics(
        [FromQuery] DateOnly? from = null, 
        [FromQuery] DateOnly? to = null)
    {
        try
        {
            var result = await _dashboardService.GetVendorStatisticsAsync(from, to, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy top vendors có doanh thu cao nhất
    /// </summary>
    [HttpGet("vendors/top-performers")]
    [EndpointSummary("Get Top Vendors")]
    [EndpointDescription("Top vendors có doanh thu cao nhất trong khoảng thời gian.")]
    public async Task<ActionResult<APIResponse>> GetTopVendors(
        [FromQuery] DateOnly? from = null, 
        [FromQuery] DateOnly? to = null, 
        [FromQuery] int limit = 10)
    {
        try
        {
            var result = await _dashboardService.GetTopVendorsAsync(from, to, limit, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thống kê giao dịch tài chính
    /// </summary>
    [HttpGet("transactions/statistics")]
    [EndpointSummary("Get Transaction Statistics")]
    [EndpointDescription("Thống kê giao dịch tài chính: inflow, outflow, theo loại giao dịch, xu hướng theo ngày.")]
    public async Task<ActionResult<APIResponse>> GetTransactionStatistics([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var result = await _dashboardService.GetTransactionStatisticsAsync(from, to, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy chi tiết các hàng đợi chờ xử lý
    /// </summary>
    [HttpGet("queues/statistics")]
    [EndpointSummary("Get Queue Statistics")]
    [EndpointDescription("Chi tiết các hàng đợi yêu cầu chờ xử lý: thời gian chờ trung bình, số lượng pending.")]
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
    
    [HttpGet("export-transactions")]
    public async Task<IActionResult> ExportTransactions(
        [FromQuery] DateOnly from, 
        [FromQuery] DateOnly to, 
        CancellationToken cancellationToken)
    {
        try 
        {
            var fileContent = await _dashboardService.ExportTransactionHistoryAsync(from, to, cancellationToken);
        
            var fileName = $"TransactionHistory_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx";
            var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(fileContent, mimeType, fileName);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

