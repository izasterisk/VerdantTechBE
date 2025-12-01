using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.Crops;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/farm/{farmId}/[controller]")]
public class CropController : BaseController
{
    private readonly ICropService _cropService;
    
    public CropController(ICropService cropService)
    {
        _cropService = cropService;
    }

    /// <summary>
    /// Thêm danh sách cây trồng vào trang trại
    /// </summary>
    /// <param name="farmId">ID của trang trại</param>
    /// <param name="dtos">Danh sách thông tin cây trồng cần thêm</param>
    /// <returns>Thông tin trang trại đã cập nhật với danh sách cây trồng mới</returns>
    [HttpPost]
    [Authorize]
    [EndpointSummary("Add Crops to Farm")]
    [EndpointDescription("Thêm một hoặc nhiều cây trồng vào trang trại. Hệ thống sẽ kiểm tra trùng lặp (tên + ngày trồng), " +
                         "validate phương pháp trồng phù hợp với loại cây và kiểu canh tác, " +
                         "và không cho phép trạng thái Completed/Deleted/Failed khi tạo mới.")]
    public async Task<ActionResult<APIResponse>> AddCropsToFarm(ulong farmId, [FromBody] List<CropsCreateDTO> dtos)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var farmProfile = await _cropService.AddCropsToFarmAsync(farmId, dtos, GetCancellationToken());
            return SuccessResponse(farmProfile, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Cập nhật danh sách cây trồng của trang trại
    /// </summary>
    /// <param name="farmId">ID của trang trại</param>
    /// <param name="dtos">Danh sách thông tin cây trồng cần cập nhật</param>
    /// <returns>Thông tin trang trại đã cập nhật với danh sách cây trồng mới</returns>
    [HttpPatch]
    [Authorize]
    [EndpointSummary("Update Crops")]
    [EndpointDescription("Cập nhật thông tin các cây trồng của trang trại. Mỗi DTO phải có Id. " +
                         "Hệ thống sẽ validate phương pháp trồng, loại cây, kiểu canh tác, và ngày trồng không được lớn hơn ngày hiện tại.")]
    public async Task<ActionResult<APIResponse>> UpdateCrops(ulong farmId, [FromBody] List<CropsUpdateDTO> dtos)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var farmProfile = await _cropService.UpdateCropsAsync(farmId, dtos, GetCancellationToken());
            return SuccessResponse(farmProfile);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
