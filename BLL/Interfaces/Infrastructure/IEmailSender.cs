using System.Threading;
using System.Threading.Tasks;

namespace BLL.Interfaces.Infrastructure;

public interface IEmailSender
{
    Task SendVerificationEmailAsync(string toEmail, string fullName, string code, CancellationToken cancellationToken = default);
    Task SendForgotPasswordEmailAsync(string toEmail, string fullName, string code, CancellationToken cancellationToken = default);
    Task SendStaffAccountCreatedEmailAsync(string toEmail, string fullName, string password, CancellationToken cancellationToken = default);
    Task SendVendorProfileVerifiedEmailAsync(string toEmail, string fullName, string loginEmail, string password, CancellationToken cancellationToken = default);
    Task SendVendorProfileRejectedEmailAsync(string toEmail, string fullName, string reason, CancellationToken cancellationToken = default);
    Task SendVendorProfileSubmittedEmailAsync( string email, string fullName, CancellationToken ct = default);
}


