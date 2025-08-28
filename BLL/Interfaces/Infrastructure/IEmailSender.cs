using System.Threading;
using System.Threading.Tasks;

namespace BLL.Interfaces.Infrastructure;

public interface IEmailSender
{
    Task SendVerificationEmailAsync(string toEmail, string fullName, string code, CancellationToken cancellationToken = default);
}


