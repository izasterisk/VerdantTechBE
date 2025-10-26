using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Payment.PayOS;

public class CreatePaymentDataDTO
{
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
    public string Description { get; set; } = null!;
}

/// <summary>
/// DTO cho việc confirm webhook URL với PayOS
/// </summary>
public class ConfirmWebhookDTO
{
    [Required(ErrorMessage = "Webhook URL là bắt buộc")]
    [Url(ErrorMessage = "Webhook URL phải là một URL hợp lệ")]
    public string WebhookUrl { get; set; } = null!;
}