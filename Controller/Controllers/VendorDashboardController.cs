using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.Interfaces;
using BLL.DTO;

namespace Controller.Controllers;

[Route("api/vendor-dashboard")]
[ApiController]
[Authorize(Roles = "Vendor")]
public class VendorDashboardController : BaseController
{
    private readonly IVendorDashboardService _dashboardService;

    public VendorDashboardController(IVendorDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Lấy thống kê tổng quan cho vendor
    /// </summary>
    [HttpGet("overview")]
    [EndpointSummary("Get Vendor Overview")]
    [EndpointDescription("Lấy thống kê tổng quan nhanh cho vendor: số dư ví, doanh thu, đơn hàng, sản phẩm, rating.")]
    public async Task<ActionResult<APIResponse>> GetOverview()
    {
        try
        {
            var result = await _dashboardService.GetOverviewAsync(GetCurrentUserId(), GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy doanh thu theo khoảng thời gian
    /// </summary>
    [HttpGet("revenue")]
    [EndpointSummary("Get Revenue")]
    [EndpointDescription("Lấy doanh thu của vendor theo khoảng thời gian (gross, commission, net).")]
    public async Task<ActionResult<APIResponse>> GetRevenue([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var result = await _dashboardService.GetRevenueAsync(GetCurrentUserId(), from, to, GetCancellationToken());
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
    [EndpointDescription("Lấy doanh thu theo ngày trong khoảng thời gian (tối đa 90 ngày) cho biểu đồ.")]
    public async Task<ActionResult<APIResponse>> GetDailyRevenue([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var result = await _dashboardService.GetDailyRevenueAsync(GetCurrentUserId(), from, to, GetCancellationToken());
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
    [EndpointDescription("Lấy doanh thu theo tháng trong năm cho biểu đồ yearly.")]
    public async Task<ActionResult<APIResponse>> GetMonthlyRevenue([FromQuery] int year)
    {
        try
        {
            var result = await _dashboardService.GetMonthlyRevenueAsync(GetCurrentUserId(), year, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thống kê đơn hàng theo trạng thái
    /// </summary>
    [HttpGet("orders/statistics")]
    [EndpointSummary("Get Order Statistics")]
    [EndpointDescription("Thống kê đơn hàng của vendor theo trạng thái và khoảng thời gian.")]
    public async Task<ActionResult<APIResponse>> GetOrderStatistics([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var result = await _dashboardService.GetOrderStatisticsAsync(GetCurrentUserId(), from, to, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thống kê sản phẩm
    /// </summary>
    [HttpGet("products/statistics")]
    [EndpointSummary("Get Product Statistics")]
    [EndpointDescription("Thống kê sản phẩm của vendor: active, hết hàng, tồn kho, phân bố theo danh mục.")]
    public async Task<ActionResult<APIResponse>> GetProductStatistics()
    {
        try
        {
            var result = await _dashboardService.GetProductStatisticsAsync(GetCurrentUserId(), GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy top sản phẩm bán chạy
    /// </summary>
    [HttpGet("products/best-selling")]
    [EndpointSummary("Get Best Selling Products")]
    [EndpointDescription("Top sản phẩm bán chạy nhất của vendor trong khoảng thời gian.")]
    public async Task<ActionResult<APIResponse>> GetBestSellingProducts(
        [FromQuery] DateOnly? from = null, 
        [FromQuery] DateOnly? to = null, 
        [FromQuery] int limit = 10)
    {
        try
        {
            var result = await _dashboardService.GetBestSellingProductsAsync(GetCurrentUserId(), from, to, limit, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thống kê rating sản phẩm
    /// </summary>
    [HttpGet("products/ratings")]
    [EndpointSummary("Get Product Ratings")]
    [EndpointDescription("Thống kê rating sản phẩm: phân bố rating, top 3 cao nhất/thấp nhất.")]
    public async Task<ActionResult<APIResponse>> GetProductRatings()
    {
        try
        {
            var result = await _dashboardService.GetProductRatingsAsync(GetCurrentUserId(), GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thống kê ví và giao dịch
    /// </summary>
    [HttpGet("wallet/statistics")]
    [EndpointSummary("Get Wallet Statistics")]
    [EndpointDescription("Thống kê ví: số dư, pending cashout, tổng nạp/rút, giao dịch gần đây.")]
    public async Task<ActionResult<APIResponse>> GetWalletStatistics(
        [FromQuery] DateOnly? from = null, 
        [FromQuery] DateOnly? to = null)
    {
        try
        {
            var result = await _dashboardService.GetWalletStatisticsAsync(GetCurrentUserId(), from, to, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách yêu cầu đang chờ xử lý
    /// </summary>
    [HttpGet("pending-items")]
    [EndpointSummary("Get Pending Items")]
    [EndpointDescription("Danh sách các yêu cầu đang chờ xử lý: đăng ký sản phẩm, cập nhật, chứng chỉ, rút tiền.")]
    public async Task<ActionResult<APIResponse>> GetPendingItems()
    {
        try
        {
            var result = await _dashboardService.GetPendingItemsAsync(GetCurrentUserId(), GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}

