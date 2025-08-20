using Microsoft.AspNetCore.Http;

namespace Infrastructure.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    /// Get current user ID from JWT token context
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>User ID if available, null otherwise</returns>
    public static ulong? GetUserId(this HttpContext context)
    {
        var userIdValue = context.Items["UserId"];
        return userIdValue as ulong?;
    }

    /// <summary>
    /// Get current user email from JWT token context
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>User email if available, null otherwise</returns>
    public static string? GetUserEmail(this HttpContext context)
    {
        return context.Items["UserEmail"] as string;
    }

    /// <summary>
    /// Get current user role from JWT token context
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>User role if available, null otherwise</returns>
    public static string? GetUserRole(this HttpContext context)
    {
        return context.Items["UserRole"] as string;
    }

    /// <summary>
    /// Check if current user is verified from JWT token context
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>True if user is verified, false otherwise</returns>
    public static bool IsUserVerified(this HttpContext context)
    {
        var isVerifiedValue = context.Items["IsVerified"] as string;
        return bool.TryParse(isVerifiedValue, out var isVerified) && isVerified;
    }

    /// <summary>
    /// Check if user is authenticated (has valid JWT token)
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>True if user is authenticated, false otherwise</returns>
    public static bool IsAuthenticated(this HttpContext context)
    {
        return context.GetUserId().HasValue;
    }
}
