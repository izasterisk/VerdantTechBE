using System.Reflection;
using System.Text;
using System.Net.Mail;
using BLL.Interfaces.Infrastructure;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;

namespace Infrastructure.Email;

public class EmailSender : IEmailSender
{
    public async Task SendVerificationEmailAsync(string toEmail, string fullName, string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(toEmail);
        ArgumentException.ThrowIfNullOrEmpty(code);

        // Load templates from embedded resources
        string htmlTemplate = GetEmbeddedResourceString("Email.Templates.verification.html") ?? "";
        string textTemplate = GetEmbeddedResourceString("Email.Templates.verification.txt") ?? "";

        string fullNameValue = string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName;

        string htmlBody = ReplacePlaceholders(htmlTemplate, fullNameValue, code);
        string textBody = ReplacePlaceholders(textTemplate, fullNameValue, code);

        await SendEmailAsync(
            toEmail, 
            "Xác thực tài khoản VerdantTech", 
            htmlBody, 
            textBody, 
            fullNameValue, 
            code, 
            "verification",
            cancellationToken);
    }

    public async Task SendForgotPasswordEmailAsync(string toEmail, string fullName, string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(toEmail);
        ArgumentException.ThrowIfNullOrEmpty(code);

        // Load forgot password templates from embedded resources
        string htmlTemplate = GetEmbeddedResourceString("Email.Templates.forgot-password.html") ?? "";
        string textTemplate = GetEmbeddedResourceString("Email.Templates.forgot-password.txt") ?? "";

        string fullNameValue = string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName;

        string htmlBody = ReplacePlaceholders(htmlTemplate, fullNameValue, code);
        string textBody = ReplacePlaceholders(textTemplate, fullNameValue, code);

        await SendEmailAsync(
            toEmail, 
            "Đặt lại mật khẩu VerdantTech", 
            htmlBody, 
            textBody, 
            fullNameValue, 
            code, 
            "forgot-password",
            cancellationToken);
    }

    public async Task SendStaffAccountCreatedEmailAsync(string toEmail, string fullName, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(toEmail);
        ArgumentException.ThrowIfNullOrEmpty(password);

        // Load staff account created templates from embedded resources
        string htmlTemplate = GetEmbeddedResourceString("Email.Templates.staff-account-created.html") ?? "";
        string textTemplate = GetEmbeddedResourceString("Email.Templates.staff-account-created.txt") ?? "";

        string fullNameValue = string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName;

        string htmlBody = ReplaceStaffAccountPlaceholders(htmlTemplate, fullNameValue, toEmail, password);
        string textBody = ReplaceStaffAccountPlaceholders(textTemplate, fullNameValue, toEmail, password);

        await SendEmailAsync(
            toEmail, 
            "Tài khoản nội bộ được cấp riêng cho nhân viên của Verdant Tech", 
            htmlBody, 
            textBody, 
            fullNameValue, 
            password, 
            "staff-account-created",
            cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string textBody, 
        string fullName, string code, string emailType, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = CreateGmailService();
            var message = CreateEmailMessage(toEmail, subject, htmlBody, textBody, fullName, code);
            
            var request = service.Users.Messages.Send(message, "me");
            await request.ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Gửi email {emailType} thất bại: {ex.Message}", ex);
        }
    }

    private GmailService CreateGmailService()
    {
        var gmailUser = Environment.GetEnvironmentVariable("GMAIL_USER") 
            ?? throw new InvalidOperationException("GMAIL_USER not configured");
        var refreshToken = Environment.GetEnvironmentVariable("GMAIL_REFRESH_TOKEN") 
            ?? throw new InvalidOperationException("GMAIL_REFRESH_TOKEN not configured");
        var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") 
            ?? throw new InvalidOperationException("GOOGLE_CLIENT_ID not configured");
        var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") 
            ?? throw new InvalidOperationException("GOOGLE_CLIENT_SECRET not configured");

        var credential = new UserCredential(
            new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },
                    Scopes = new[] { GmailService.Scope.GmailSend }
                }),
            gmailUser,
            new Google.Apis.Auth.OAuth2.Responses.TokenResponse
            {
                RefreshToken = refreshToken
            });

        return new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "VerdantTech"
        });
    }

    private Message CreateEmailMessage(string toEmail, string subject, string htmlBody, string textBody, 
        string fullName, string code)
    {
        var gmailUser = Environment.GetEnvironmentVariable("GMAIL_USER") 
            ?? throw new InvalidOperationException("GMAIL_USER not configured");

        // Encode subject properly for Unicode characters
        var encodedSubject = EncodeSubject(subject);

        var emailMessage = new StringBuilder();
        emailMessage.AppendLine($"To: {toEmail}");
        emailMessage.AppendLine($"From: VerdantTech <{gmailUser}>");
        emailMessage.AppendLine($"Subject: {encodedSubject}");
        emailMessage.AppendLine("MIME-Version: 1.0");
        emailMessage.AppendLine("Content-Type: multipart/alternative; boundary=\"boundary123\"");
        emailMessage.AppendLine();
        emailMessage.AppendLine("--boundary123");
        emailMessage.AppendLine("Content-Type: text/plain; charset=utf-8");
        emailMessage.AppendLine();
        
        // Text body with fallback
        string finalTextBody = string.IsNullOrWhiteSpace(textBody) 
            ? $"Xin chào {fullName}, mã của bạn là: {code}" 
            : textBody;
        emailMessage.AppendLine(finalTextBody);
        
        // HTML body if available
        if (!string.IsNullOrWhiteSpace(htmlBody))
        {
            emailMessage.AppendLine();
            emailMessage.AppendLine("--boundary123");
            emailMessage.AppendLine("Content-Type: text/html; charset=utf-8");
            emailMessage.AppendLine();
            emailMessage.AppendLine(htmlBody);
        }
        
        emailMessage.AppendLine();
        emailMessage.AppendLine("--boundary123--");

        var rawMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(emailMessage.ToString()))
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");

        return new Message { Raw = rawMessage };
    }

    private static string EncodeSubject(string subject)
    {
        // Check if subject contains non-ASCII characters
        if (subject.All(c => c < 128))
        {
            return subject; // No encoding needed for ASCII-only subjects
        }

        // Encode subject using RFC 2047 MIME encoded-word format
        var bytes = Encoding.UTF8.GetBytes(subject);
        var encoded = Convert.ToBase64String(bytes);
        return $"=?UTF-8?B?{encoded}?=";
    }

    private static string ReplacePlaceholders(string template, string fullName, string code)
    {
        if (string.IsNullOrEmpty(template)) return template;
        return template
            .Replace("{{FULL_NAME}}", fullName)
            .Replace("{{VERIFICATION_TOKEN}}", code);
    }

    private static string ReplaceStaffAccountPlaceholders(string template, string fullName, string email, string password)
    {
        if (string.IsNullOrEmpty(template)) return template;
        return template
            .Replace("{{fullName}}", fullName)
            .Replace("{{email}}", email)
            .Replace("{{password}}", password);
    }

    private static string? GetEmbeddedResourceString(string resourcePathTail)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(resourcePathTail.Replace('/', '.'), StringComparison.OrdinalIgnoreCase));
        if (resourceName == null) return null;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}


