using System.Reflection;
using System.Text;
using BLL.Interfaces.Infrastructure;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Infrastructure.Email;

public class EmailSender : IEmailSender
{
    public async Task SendVerificationEmailAsync(string toEmail, string fullName, string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(toEmail);
        ArgumentException.ThrowIfNullOrEmpty(code);

        var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
        var smtpPortStr = Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587";
        var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? throw new InvalidOperationException("SMTP_USER not configured");
        var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? throw new InvalidOperationException("SMTP_PASS not configured");
        var smtpSenderName = Environment.GetEnvironmentVariable("SMTP_SENDER_NAME") ?? "VerdantTech";

        if (!int.TryParse(smtpPortStr, out int smtpPort)) smtpPort = 587;

        // Load templates from embedded resources
        string htmlTemplate = GetEmbeddedResourceString("Email.Templates.verification.html") ?? "";
        string textTemplate = GetEmbeddedResourceString("Email.Templates.verification.txt") ?? "";

        string fullNameValue = string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName;

        string htmlBody = ReplacePlaceholders(htmlTemplate, fullNameValue, code);
        string textBody = ReplacePlaceholders(textTemplate, fullNameValue, code);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtpSenderName, smtpUser));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Xác thực tài khoản VerdantTech";

        var builder = new BodyBuilder
        {
            HtmlBody = string.IsNullOrWhiteSpace(htmlBody) ? null : htmlBody,
            TextBody = string.IsNullOrWhiteSpace(textBody) ? $"Xin chào {fullNameValue}, mã xác thực của bạn là: {code}" : textBody
        };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(smtpUser, smtpPass, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    public async Task SendForgotPasswordEmailAsync(string toEmail, string fullName, string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(toEmail);
        ArgumentException.ThrowIfNullOrEmpty(code);

        var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
        var smtpPortStr = Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587";
        var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? throw new InvalidOperationException("SMTP_USER not configured");
        var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? throw new InvalidOperationException("SMTP_PASS not configured");
        var smtpSenderName = Environment.GetEnvironmentVariable("SMTP_SENDER_NAME") ?? "VerdantTech";

        if (!int.TryParse(smtpPortStr, out int smtpPort)) smtpPort = 587;

        // Load forgot password templates from embedded resources
        string htmlTemplate = GetEmbeddedResourceString("Email.Templates.forgot-password.html") ?? "";
        string textTemplate = GetEmbeddedResourceString("Email.Templates.forgot-password.txt") ?? "";

        string fullNameValue = string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName;

        string htmlBody = ReplacePlaceholders(htmlTemplate, fullNameValue, code);
        string textBody = ReplacePlaceholders(textTemplate, fullNameValue, code);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtpSenderName, smtpUser));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Đặt lại mật khẩu VerdantTech";

        var builder = new BodyBuilder
        {
            HtmlBody = string.IsNullOrWhiteSpace(htmlBody) ? null : htmlBody,
            TextBody = string.IsNullOrWhiteSpace(textBody) ? $"Xin chào {fullNameValue}, mã đặt lại mật khẩu của bạn là: {code}" : textBody
        };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(smtpUser, smtpPass, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private static string ReplacePlaceholders(string template, string fullName, string code)
    {
        if (string.IsNullOrEmpty(template)) return template;
        return template
            .Replace("{{FULL_NAME}}", fullName)
            .Replace("{{VERIFICATION_TOKEN}}", code);
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


