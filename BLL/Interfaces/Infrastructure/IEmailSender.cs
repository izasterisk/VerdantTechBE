using System.Threading;
using System.Threading.Tasks;

namespace BLL.Interfaces.Infrastructure;

public interface IEmailSender
{
    Task SendVerificationEmailAsync(string toEmail, string fullName, string code, CancellationToken cancellationToken = default);
    Task SendForgotPasswordEmailAsync(string toEmail, string fullName, string code, CancellationToken cancellationToken = default);
    Task SendStaffAccountCreatedEmailAsync(string toEmail, string fullName, string password, CancellationToken cancellationToken = default);
}


