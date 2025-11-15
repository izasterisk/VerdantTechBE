# 🔔 HƯỚNG DẪN TÍCH HỢP SIGNALR CHO HỆ THỐNG THÔNG BÁO REAL-TIME

## 📋 MỤC LỤC

1. [Tổng quan](#1-tổng-quan)
2. [Kiến trúc Clean Architecture & SignalR](#2-kiến-trúc-clean-architecture--signalr)
3. [Cài đặt Package](#3-cài-đặt-package)
4. [Tạo cấu trúc SignalR](#4-tạo-cấu-trúc-signalr)
5. [Cấu hình Infrastructure](#5-cấu-hình-infrastructure)
6. [Cấu hình Program.cs](#6-cấu-hình-programcs)
7. [Cập nhật NotificationService](#7-cập-nhật-notificationservice)
8. [Tích hợp vào các Service](#8-tích-hợp-vào-các-service)
9. [Client-side Implementation](#9-client-side-implementation)
10. [Testing](#10-testing)
11. [Troubleshooting](#11-troubleshooting)

---

## 1. TỔNG QUAN

### 🎯 Mục tiêu
Tích hợp SignalR để gửi thông báo real-time cho người dùng khi có sự kiện xảy ra trong hệ thống (order, payment, request, etc.)

### 🏗️ Kiến trúc
```
┌─────────────┐     Event      ┌──────────────────┐     SignalR      ┌─────────────┐
│   Service   │ ─────────────> │ NotificationHub  │ ──────────────>  │   Client    │
│ (Order, etc)│                │     Service      │   (WebSocket)    │ (Browser)   │
└─────────────┘                └──────────────────┘                  └─────────────┘
                                        ↓
                                   ┌─────────┐
                                   │   DB    │
                                   └─────────┘
```

### 💡 SignalR là gì?

**SignalR** cho phép **server chủ động gửi (push) dữ liệu real-time cho client** qua WebSocket.

**So sánh:**
```
REST API (cũ):
Client: "Có notification mới không?" → Server: "Không"
Client: "Có notification mới không?" → Server: "Không"
Client: "Có notification mới không?" → Server: "Có!"
❌ Phải hỏi liên tục (polling)

SignalR (mới):
Client: "Kết nối với tôi!" → Server: "OK!"
[Connection giữ mở...]
Server: "Có notification mới đây!" → Client: "Nhận rồi!"
✅ Server chủ động đẩy, real-time
```

---

## 2. KIẾN TRÚC CLEAN ARCHITECTURE & SIGNALR

### 🏛️ **VỊ TRÍ CỦA SIGNALR TRONG KIẾN TRÚC**

SignalR Hub là **External Communication Mechanism** - thuộc **Infrastructure Layer**!

```
┌──────────────────────────────────────────────┐
│   PRESENTATION LAYER (Controller/)           │
│   └── Program.cs                      ✅     │
│       (CHỈ configure & map endpoints)        │
└──────────────────────────────────────────────┘
                    ↓ uses
┌──────────────────────────────────────────────┐
│   INFRASTRUCTURE LAYER (Infrastructure/)     │
│   └── SignalR/                        ✅     │
│       ├── BaseHub.cs                  ✅     │
│       ├── NotificationHub.cs          ✅     │
│       └── NotificationHubService.cs   ✅     │
└──────────────────────────────────────────────┘
                    ↓ implements
┌──────────────────────────────────────────────┐
│   APPLICATION LAYER (BLL/)                   │
│   ├── Interfaces/Infrastructure/             │
│   │   └── INotificationHub.cs         ✅     │
│   └── Services/                              │
│       └── NotificationService.cs      ✅     │
└──────────────────────────────────────────────┘
                    ↓ uses
┌──────────────────────────────────────────────┐
│   DOMAIN LAYER (DAL/)                        │
│   └── Repository/NotificationRepository.cs   │
└──────────────────────────────────────────────┘
```

### 🔐 **TẠI SAO PHẢI CÓ [Authorize] TRÊN HUB?**

SignalR Hub **KHÔNG đi qua Controller**!

```
HTTP Request Flow:
Client → Controller [Authorize] → Service → DB

SignalR WebSocket Flow:
Client → Hub [PHẢI Authorize RIÊNG] → Service → DB
```

**Kết luận:** Hub là endpoint độc lập, bỏ `[Authorize]` = lỗ hổng bảo mật nghiêm trọng! 🔴

### 📂 Cấu trúc file sẽ tạo/cập nhật

```
BE/
├── Controller/                        (PRESENTATION LAYER)
│   └── Program.cs                    (CẬP NHẬT - chỉ config)
│
├── Infrastructure/                    (INFRASTRUCTURE LAYER)
│   ├── SignalR/                      ✅ TẠO FOLDER MỚI
│   │   ├── BaseHub.cs                ✅ TẠO MỚI
│   │   ├── NotificationHub.cs        ✅ TẠO MỚI
│   │   └── NotificationHubService.cs ✅ TẠO MỚI
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs (CẬP NHẬT)
│   └── Infrastructure.csproj         (CẬP NHẬT)
│
├── BLL/                              (APPLICATION LAYER)
│   ├── Interfaces/
│   │   ├── Infrastructure/
│   │   │   └── INotificationHub.cs   ✅ TẠO MỚI
│   │   └── INotificationService.cs   (CẬP NHẬT)
│   └── Services/
│       └── NotificationService.cs    (CẬP NHẬT)
│
└── DAL/                              (DOMAIN LAYER)
    ├── Data/Models/
    │   └── Notification.cs           (Có sẵn)
    └── Repository/
        └── NotificationRepository.cs (Có sẵn)
```

### 📊 **Dependencies Flow**

```
✅ Controller → Infrastructure → BLL → DAL
✅ KHÔNG có circular dependency!
✅ Dependencies luôn hướng vào trong (inward)!

Chi tiết:
Controller/Program.cs
  → MapHub<NotificationHub>() (từ Infrastructure)
  → AddInfrastructure() (đăng ký services)

Infrastructure/SignalR/NotificationHubService
  → implements INotificationHub (từ BLL)
  → uses NotificationHub (cùng Infrastructure)

BLL/Services/NotificationService
  → uses INotificationHub (interface trong BLL)
```

---

## 3. CẤU HÌNH REFERENCES (KHÔNG CẦN CÀI PACKAGE)

### ✅ SignalR đã tích hợp sẵn trong Shared Framework

**Từ .NET Core 3.0 trở lên** (bao gồm .NET 8), các abstractions như `HttpContext`, `Hub`, `IHubContext` **đã được tích hợp sẵn** trong **shared framework `Microsoft.AspNetCore.App`**.

**Điều này có nghĩa:**

- ❌ **KHÔNG cần cài package riêng** `Microsoft.AspNetCore.SignalR`
- ❌ **KHÔNG cần cài package** `Microsoft.AspNetCore.Http.Abstractions`
- ✅ SignalR APIs (`Hub`, `IHubContext`, `MapHub`, `AddSignalR`) **có sẵn ngay**
- ✅ Chỉ cần thêm `FrameworkReference` vào Infrastructure project

### 📋 Cấu hình duy nhất cần thiết

**Infrastructure project** (Class Library) cần thêm `FrameworkReference` để truy cập shared framework:

#### **Cập nhật `Infrastructure/Infrastructure.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- ✅ THÊM FrameworkReference để truy cập shared framework Microsoft.AspNetCore.App -->
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="CloudinaryDotNet" Version="1.27.8" />
    <PackageReference Include="DotNetEnv" Version="3.1.1" />
    <PackageReference Include="Google.Apis.Auth" Version="1.71.0" />
    <PackageReference Include="Google.Apis.Gmail.v1" Version="1.70.0.3833" />
    <PackageReference Include="payOS" Version="1.0.9" />
    <!-- ❌ KHÔNG cần cài bất kỳ package SignalR nào -->
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\BLL\BLL.csproj" />
  </ItemGroup>
  
  <!-- ... existing embedded resources ... -->
</Project>
```

### 📝 Lý do kỹ thuật

**Shared Framework là gì?**
- Từ .NET Core 3.0, Microsoft tách ASP.NET Core thành shared framework riêng
- `Microsoft.AspNetCore.App` chứa tất cả APIs cốt lõi: HTTP, MVC, SignalR, Authentication, etc.
- Framework này được cài đặt cùng .NET Runtime, không cần download riêng

**Controller project:**
- ✅ Đã là ASP.NET Core Web API project (`Microsoft.NET.Sdk.Web`)
- ✅ Tự động reference `Microsoft.AspNetCore.App` shared framework
- ✅ Có sẵn tất cả APIs: `Hub`, `IHubContext`, `MapHub`, `AddSignalR`

**Infrastructure project:**
- ⚠️ Là Class Library project (`Microsoft.NET.Sdk`)
- ⚠️ Mặc định KHÔNG reference shared framework
- ✅ Phải thêm `<FrameworkReference Include="Microsoft.AspNetCore.App" />` để truy cập SignalR APIs
- ✅ Không cần cài package vì framework đã có sẵn trên máy

**BLL project:**
- ✅ Chỉ chứa interfaces (POCO)
- ✅ Không cần reference gì thêm

### 🔍 Kiểm tra sau khi cấu hình

**Rebuild solution và verify không có lỗi:**

```bash
dotnet clean
dotnet build
```

Nếu thấy lỗi kiểu:
- `"Hub" could not be found` → Cần thêm `FrameworkReference` vào Infrastructure
- `"IHubContext" could not be found` → Cần thêm `FrameworkReference` vào Infrastructure

Nếu build thành công → **Sẵn sàng chuyển sang bước 4** ✅

---

## 4. TẠO CẤU TRÚC SIGNALR

### 📄 Bước 4.1: Tạo `Infrastructure/SignalR/BaseHub.cs`

Tương tự như BaseController, tạo BaseHub để tái sử dụng logic:

```csharp
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Infrastructure.SignalR;

/// <summary>
/// Base class cho tất cả SignalR Hubs
/// Cung cấp các helper methods giống BaseController
/// </summary>
public abstract class BaseHub : Hub
{
    /// <summary>
    /// Lấy UserId từ JWT token claims
    /// Logic giống BaseController.GetCurrentUserId()
    /// </summary>
    protected ulong GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("Người dùng chưa được xác thực");

        if (!ulong.TryParse(userIdClaim, out ulong userId))
            throw new ArgumentException("Định dạng ID người dùng không hợp lệ");

        return userId;
    }

    /// <summary>
    /// Thử lấy UserId, trả về null nếu không có
    /// </summary>
    protected ulong? TryGetCurrentUserId()
    {
        try
        {
            return GetCurrentUserId();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Lấy Role của user hiện tại
    /// </summary>
    protected string? GetCurrentUserRole()
    {
        return Context.User?.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Kiểm tra user có role cụ thể không
    /// </summary>
    protected bool IsInRole(string role)
    {
        return Context.User?.IsInRole(role) ?? false;
    }
}
```

**📝 Lợi ích của BaseHub:**
- ✅ Tái sử dụng logic như BaseController
- ✅ Tránh duplicate code
- ✅ Dễ dàng thêm helper methods chung
- ✅ Consistent với kiến trúc hiện tại

---

### 📄 Bước 4.2: Tạo `Infrastructure/SignalR/NotificationHub.cs`

```csharp
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.SignalR;

/// <summary>
/// SignalR Hub để xử lý thông báo real-time
/// Đây là Infrastructure component - External Communication Mechanism
/// </summary>
[Authorize] // ✅ BẮT BUỘC - Hub là endpoint độc lập, phải authorize riêng
public class NotificationHub : BaseHub
{
    /// <summary>
    /// Khi client kết nối tới Hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = TryGetCurrentUserId();
        
        if (userId.HasValue)
        {
            // Thêm connection vào group theo UserId
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId.Value}");
            
            // Optional: Thêm vào group theo Role
            var role = GetCurrentUserRole();
            if (!string.IsNullOrEmpty(role))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
            }
            
            Console.WriteLine($"[SignalR] User {userId.Value} ({role}) connected - ConnectionId: {Context.ConnectionId}");
        }
        else
        {
            Console.WriteLine($"[SignalR] Anonymous connection rejected - ConnectionId: {Context.ConnectionId}");
        }
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Khi client ngắt kết nối
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = TryGetCurrentUserId();
        
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId.Value}");
            
            var role = GetCurrentUserRole();
            if (!string.IsNullOrEmpty(role))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Role_{role}");
            }
            
            Console.WriteLine($"[SignalR] User {userId.Value} disconnected");
        }
        
        if (exception != null)
        {
            Console.WriteLine($"[SignalR] Disconnect error: {exception.Message}");
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    /// <summary>
    /// Method để client có thể gọi để đánh dấu đã đọc
    /// </summary>
    public async Task MarkNotificationAsRead(ulong notificationId)
    {
        try
        {
            var userId = GetCurrentUserId(); // Sử dụng method từ BaseHub
            Console.WriteLine($"[SignalR] User {userId} marked notification {notificationId} as read");
            
            // Notify client
            await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
        }
        catch (UnauthorizedAccessException)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
        }
    }
    
    /// <summary>
    /// Test connection - client có thể gọi để kiểm tra kết nối
    /// </summary>
    public async Task<string> Ping()
    {
        var userId = TryGetCurrentUserId();
        var role = GetCurrentUserRole();
        return $"Pong from User {userId} (Role: {role})";
    }
}
```

**📝 Điểm quan trọng:**
- Hub nằm ở Infrastructure (external communication)
- Kế thừa từ `BaseHub` (tái sử dụng logic)
- `[Authorize]` là BẮT BUỘC (endpoint độc lập)

---

### 📄 Bước 4.3: Tạo `BLL/Interfaces/Infrastructure/INotificationHub.cs`

**Tạo file mới trong folder đã có - giống IEmailSender, IPayOSApiClient:**

```csharp
namespace BLL.Interfaces.Infrastructure;

/// <summary>
/// Interface cho NotificationHub Service để gửi thông báo real-time
/// </summary>
public interface INotificationHub
{
    /// <summary>
    /// Gửi thông báo cho một user cụ thể
    /// </summary>
    Task SendNotificationToUser(ulong userId, object notification);
    
    /// <summary>
    /// Gửi thông báo cho nhiều user
    /// </summary>
    Task SendNotificationToMultipleUsers(List<ulong> userIds, object notification);
    
    /// <summary>
    /// Gửi thông báo cho tất cả user đang online (broadcast)
    /// </summary>
    Task SendNotificationToAllUsers(object notification);
    
    /// <summary>
    /// Gửi thông báo cho user theo role cụ thể (Staff, Admin, Vendor...)
    /// </summary>
    Task SendNotificationToRole(string role, object notification);
}
```

**📝 Lưu ý:**
- Interface nằm ở `BLL/Interfaces/Infrastructure/` (giống IEmailSender, IPayOSApiClient)
- Namespace: `BLL.Interfaces.Infrastructure`
- Implementation ở Infrastructure layer

---

### 📄 Bước 4.4: Tạo `Infrastructure/SignalR/NotificationHubService.cs`

```csharp
using BLL.Interfaces.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR;

/// <summary>
/// Service để gửi thông báo real-time qua SignalR
/// Đây là Infrastructure implementation
/// </summary>
public class NotificationHubService : INotificationHub
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Gửi thông báo cho 1 user cụ thể
    /// </summary>
    public async Task SendNotificationToUser(ulong userId, object notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("ReceiveNotification", notification);
            
            Console.WriteLine($"[NotificationHub] Sent notification to User {userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationHub] Error sending to User {userId}: {ex.Message}");
            // Không throw - notification đã lưu DB, việc gửi realtime fail là acceptable
        }
    }

    /// <summary>
    /// Gửi thông báo cho nhiều user
    /// </summary>
    public async Task SendNotificationToMultipleUsers(List<ulong> userIds, object notification)
    {
        try
        {
            var groupNames = userIds.Select(id => $"User_{id}").ToList();
            
            await _hubContext.Clients
                .Groups(groupNames)
                .SendAsync("ReceiveNotification", notification);
            
            Console.WriteLine($"[NotificationHub] Sent notification to {userIds.Count} users");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationHub] Error sending to multiple users: {ex.Message}");
        }
    }

    /// <summary>
    /// Gửi thông báo cho tất cả user đang online (broadcast)
    /// </summary>
    public async Task SendNotificationToAllUsers(object notification)
    {
        try
        {
            await _hubContext.Clients.All
                .SendAsync("ReceiveNotification", notification);
            
            Console.WriteLine($"[NotificationHub] Broadcast notification to all users");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationHub] Error broadcasting: {ex.Message}");
        }
    }

    /// <summary>
    /// Gửi thông báo cho tất cả user có role cụ thể
    /// </summary>
    public async Task SendNotificationToRole(string role, object notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"Role_{role}")
                .SendAsync("ReceiveNotification", notification);
            
            Console.WriteLine($"[NotificationHub] Sent notification to role {role}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationHub] Error sending to role {role}: {ex.Message}");
        }
    }
}
```

**📝 Giải thích:**
- `IHubContext<NotificationHub>`: Interface để gửi message từ bên ngoài Hub
- `Group($"User_{userId}")`: Gửi message tới group của user cụ thể
- `SendAsync("ReceiveNotification", notification)`: Gọi method "ReceiveNotification" ở client
- Try-catch để không crash app nếu SignalR lỗi

---

## 5. CẤU HÌNH INFRASTRUCTURE

### 📄 Bước 5.1: Cập nhật `Infrastructure/Extensions/ServiceCollectionExtensions.cs`

```csharp
using BLL.Interfaces.Infrastructure;
using Infrastructure.Address;
using Infrastructure.Email;
using Infrastructure.Soil;
using Infrastructure.Weather;
using Infrastructure.Courier;
using Infrastructure.Payment.PayOS;
using Infrastructure.SignalR; // ✅ THÊM
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmail(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, EmailSender>();
        return services;
    }

    public static IServiceCollection AddWeather(this IServiceCollection services)
    {
        services.AddScoped<HttpClient>();
        services.AddScoped<IWeatherApiClient, WeatherApiClient>();
        return services;
    }

    public static IServiceCollection AddSoilGrids(this IServiceCollection services)
    {
        services.AddScoped<HttpClient>();
        services.AddScoped<ISoilGridsApiClient, SoilGridsApiClient>();
        return services;
    }

    public static IServiceCollection AddCourier(this IServiceCollection services)
    {
        services.AddScoped<HttpClient>();
        services.AddScoped<IGoshipCourierApiClient, GoshipCourierApiClient>();
        return services;
    }
    
    public static IServiceCollection AddAddress(this IServiceCollection services)
    {
        services.AddScoped<HttpClient>();
        services.AddScoped<IGoshipAddressApiClient, GoshipAddressApiClient>();
        return services;
    }
    
    public static IServiceCollection AddPayOS(this IServiceCollection services)
    {
        services.AddScoped<IPayOSApiClient, PayOSApiClient>();
        return services;
    }

    // ✅ THÊM METHOD MỚI
    public static IServiceCollection AddSignalRNotification(this IServiceCollection services)
    {
        services.AddScoped<INotificationHub, NotificationHubService>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddEmail();
        services.AddWeather();
        services.AddSoilGrids();
        services.AddCourier();
        services.AddAddress();
        services.AddPayOS();
        services.AddSignalRNotification(); // ✅ THÊM
        return services;
    }
}
```

**📝 Lợi ích:**
- ✅ Consistent với pattern hiện có (AddEmail, AddPayOS, etc.)
- ✅ Tất cả infrastructure services đăng ký ở 1 chỗ
- ✅ Dễ dàng enable/disable từng service

---

## 6. CẤU HÌNH PROGRAM.CS

Mở file `Controller/Program.cs` và thực hiện các cập nhật sau:

### 🔧 Bước 6.1: Thêm using statement

Thêm vào đầu file (sau các using statements khác, khoảng dòng 21):

```csharp
using Infrastructure.SignalR;
```

### 🔧 Bước 6.2: Thêm SignalR configuration

Thêm sau dòng 164 (sau `builder.Services.AddInfrastructure();`):

```csharp
// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment(); // Chỉ enable trong Development
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});
```

**📝 Lưu ý:** 
- `builder.Environment.IsDevelopment()` đọc từ biến môi trường `ASPNETCORE_ENVIRONMENT`
- Development → `EnableDetailedErrors = true`
- Production → `EnableDetailedErrors = false` (bảo mật)

### 🔧 Bước 6.3: Cấu hình JWT Authentication cho WebSocket

**⚠️ VẤN ĐỀ:** SignalR sử dụng WebSocket protocol, **không thể gửi JWT token qua HTTP Header** như REST API!

**Giải pháp:** Gửi token qua **Query String** (`?access_token=...`)

Cập nhật JWT configuration trong `Program.cs` (khoảng dòng 173-226):

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };

    options.Events = new JwtBearerEvents
    {
        // ✅ THÊM: Đọc JWT token từ query string cho SignalR WebSocket
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            // Chỉ áp dụng cho SignalR Hub endpoints
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        },
        
        // Custom 401 response (giữ nguyên)
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            
            var response = new APIResponse
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessages = new List<string> { "Người dùng chưa được xác thực" }
            };
            
            return context.Response.WriteAsJsonAsync(response);
        },
        
        // Custom 403 response (giữ nguyên)
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            
            var response = new APIResponse
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.Forbidden,
                ErrorMessages = new List<string> { "Người dùng không có quyền truy cập" }
            };
            
            return context.Response.WriteAsJsonAsync(response);
        }
    };
});
```

**📝 Giải thích:**

```
REST API Request:
  Authorization: Bearer eyJhbGc...
  ✅ Gửi token qua HTTP Header

SignalR WebSocket Connection:
  wss://${VITE_API_BASE_URL}/hubs/notification?access_token=eyJhbGc...
  ✅ Gửi token qua Query String
  ❌ WebSocket protocol KHÔNG hỗ trợ custom headers khi handshake
```

**Tại sao cần OnMessageReceived?**
- JWT middleware mặc định chỉ đọc từ `Authorization` header
- WebSocket không thể gửi custom headers khi kết nối
- `OnMessageReceived` cho phép đọc token từ query string
- Chỉ áp dụng cho paths bắt đầu bằng `/hubs` (SignalR endpoints)

### 🔧 Bước 6.4: CORS đã đủ

CORS hiện tại của bạn đã đủ cho SignalR (có `AllowCredentials()`). Không cần thay đổi.

### 🔧 Bước 6.5: Map SignalR Hub endpoint

Thêm TRƯỚC `app.Run();` (sau dòng 330, sau `app.MapControllers();`):

```csharp
// Map SignalR Hub endpoint
app.MapHub<NotificationHub>("/hubs/notification");
```

**📍 Tổng hợp các vị trí trong Program.cs:**

```csharp
// ============================================
// PHẦN 1: USING STATEMENTS (Line ~21)
// ============================================
using Infrastructure.SignalR; // ✅ THÊM

// ... existing code ...

// ============================================
// PHẦN 2: JWT AUTHENTICATION (Line ~173-226)
// ============================================
builder.Services.AddAuthentication(...)
.AddJwtBearer(options =>
{
    // ... existing config ...
    
    options.Events = new JwtBearerEvents
    {
        // ✅ THÊM: OnMessageReceived để đọc token từ query string
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        },
        
        OnChallenge = context => { /* existing code */ },
        OnForbidden = context => { /* existing code */ }
    };
});

// ============================================
// PHẦN 3: SERVICES CONFIGURATION (Line ~164)
// ============================================

// Infrastructure services (đã bao gồm SignalR registration)
builder.Services.AddInfrastructure();

// SignalR configuration (THÊM NGAY SAU)
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// ... existing code ...

// ============================================
// PHẦN 4: MIDDLEWARE & ENDPOINTS (Line ~330)
// ============================================
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map SignalR Hub endpoint (THÊM)
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();
```
    
    options.Events = new JwtBearerEvents
    {
        // ✅ THÊM: OnMessageReceived để đọc token từ query string
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        },
        
        OnChallenge = context => { /* existing code */ },
        OnForbidden = context => { /* existing code */ }
    };
});

// ============================================
// PHẦN 3: SERVICES CONFIGURATION (Line ~164)
// ============================================

// Infrastructure services (đã bao gồm SignalR registration)
builder.Services.AddInfrastructure();

// SignalR configuration (THÊM NGAY SAU)
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// ... existing code ...

// ============================================
// PHẦN 4: MIDDLEWARE & ENDPOINTS (Line ~330)
// ============================================
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map SignalR Hub endpoint (THÊM)
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();
```

---

## 7. CẬP NHẬT NOTIFICATIONSERVICE

### 📄 Bước 7.1: Cập nhật Interface

Mở file `BLL/Interfaces/INotificationService.cs` và thêm method signature mới:

```csharp
using BLL.DTO;
using BLL.DTO.Notification;
using DAL.Data;

namespace BLL.Interfaces;

public interface INotificationService
{
    // ✅ THÊM METHOD MỚI
    /// <summary>
    /// Tạo và gửi thông báo real-time cho user
    /// </summary>
    Task<NotificationResponseDTO> CreateAndSendNotificationAsync(
        ulong userId, 
        string title, 
        string message, 
        NotificationReferenceType? referenceType = null,
        ulong? referenceId = null,
        CancellationToken cancellationToken = default);
    
    // CÁC METHOD CŨ GIỮ NGUYÊN
    Task<NotificationResponseDTO> RevertReadStatusAsync(ulong notificationId, CancellationToken cancellationToken = default);
    Task<PagedResponse<NotificationResponseDTO>> GetAllNotificationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> DeleteNotificationAsync(ulong notificationId, CancellationToken cancellationToken = default);
}
```

### 📄 Bước 7.2: Cập nhật Implementation

Mở file `BLL/Services/NotificationService.cs` và cập nhật:

```csharp
using AutoMapper;
using BLL.DTO;
using BLL.DTO.Notification;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure; // ✅ THÊM
using DAL.Data.Models;
using DAL.Data;
using DAL.IRepository;

namespace BLL.Services;

public class NotificationService : INotificationService
{
    private readonly IMapper _mapper;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationHub _notificationHub; // ✅ THÊM
    
    public NotificationService(
        IMapper mapper, 
        INotificationRepository notificationRepository,
        INotificationHub notificationHub) // ✅ THÊM
    {
        _mapper = mapper;
        _notificationRepository = notificationRepository;
        _notificationHub = notificationHub; // ✅ THÊM
    }
    
    // ✅ THÊM METHOD MỚI
    /// <summary>
    /// Tạo và gửi thông báo real-time cho user
    /// </summary>
    public async Task<NotificationResponseDTO> CreateAndSendNotificationAsync(
        ulong userId, 
        string title, 
        string message, 
        NotificationReferenceType? referenceType = null,
        ulong? referenceId = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Tạo notification trong database
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            IsRead = false
        };
        
        var createdNotification = await _notificationRepository.CreateNotificationAsync(notification, cancellationToken);
        var notificationDto = _mapper.Map<NotificationResponseDTO>(createdNotification);
        
        // 2. Gửi real-time qua SignalR (không chặn nếu lỗi)
        try
        {
            await _notificationHub.SendNotificationToUser(userId, notificationDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationService] Failed to send real-time notification: {ex.Message}");
            // Không throw exception - notification đã lưu DB thành công
            // User vẫn có thể xem notification khi refresh
        }
        
        return notificationDto;
    }
    
    // ============================================
    // CÁC METHOD CŨ GIỮ NGUYÊN
    // ============================================
    
    public async Task<NotificationResponseDTO> RevertReadStatusAsync(ulong notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId, cancellationToken);
        notification.IsRead = !notification.IsRead;
        var updatedNotification = await _notificationRepository.UpdateNotificationAsync(notification, cancellationToken);
        return _mapper.Map<NotificationResponseDTO>(updatedNotification);
    }
    
    public async Task<PagedResponse<NotificationResponseDTO>> GetAllNotificationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (notifications, totalCount) = await _notificationRepository.GetAllNotificationsByUserIdAsync(userId, page, pageSize, cancellationToken);
        var notificationDtos = _mapper.Map<List<NotificationResponseDTO>>(notifications);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return new PagedResponse<NotificationResponseDTO>
        {
            Data = notificationDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
    
    public async Task<bool> DeleteNotificationAsync(ulong notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId, cancellationToken);
        return await _notificationRepository.DeleteNotificationAsync(notification, cancellationToken);
    }
}
```

**📝 Điểm quan trọng:**
- Import: `BLL.Interfaces.Infrastructure` (interface)
- Inject: `INotificationHub` (interface, implementation từ Infrastructure)
- Method mới: `CreateAndSendNotificationAsync` (tạo + push real-time)
- Methods cũ: Giữ nguyên 100%

---

## 8. TÍCH HỢP VÀO CÁC SERVICE

### 📌 Nguyên tắc chung:
1. Inject `INotificationService` vào constructor
2. Gọi `CreateAndSendNotificationAsync()` khi có sự kiện quan trọng
3. Chọn `NotificationReferenceType` phù hợp từ enum

### 🔔 Ví dụ 1: RequestService.cs

```csharp
using BLL.Interfaces;
using DAL.Data;

namespace BLL.Services;

public class RequestService : IRequestService
{
    private readonly IRequestRepository _requestRepository;
    private readonly INotificationService _notificationService; // ✅ THÊM
    
    public RequestService(
        IRequestRepository requestRepository,
        INotificationService notificationService) // ✅ THÊM
    {
        _requestRepository = requestRepository;
        _notificationService = notificationService;
    }
    
    // Khi tạo request mới
    public async Task<RequestResponseDTO> CreateRequestAsync(
        ulong userId, 
        RequestCreateDTO dto, 
        CancellationToken cancellationToken = default)
    {
        // ... logic tạo request ...
        var request = await _requestRepository.CreateRequestAsync(newRequest, cancellationToken);
        
        // ✅ Gửi thông báo cho user
        await _notificationService.CreateAndSendNotificationAsync(
            userId: request.UserId,
            title: "Yêu cầu mới đã được tạo",
            message: $"Yêu cầu #{request.Id} ({request.RequestType}) đã được gửi thành công và đang chờ xử lý",
            referenceType: NotificationReferenceType.Request,
            referenceId: request.Id,
            cancellationToken: cancellationToken
        );
        
        return _mapper.Map<RequestResponseDTO>(request);
    }
    
    // Khi staff xử lý request
    public async Task<RequestResponseDTO> ProcessRequestAsync(
        ulong staffId,
        ulong requestId, 
        RequestUpdateDTO dto, 
        CancellationToken cancellationToken = default)
    {
        // ... logic xử lý request ...
        
        var statusMessages = new Dictionary<RequestStatus, string>
        {
            { RequestStatus.InReview, "đang được xem xét" },
            { RequestStatus.Approved, "đã được chấp nhận" },
            { RequestStatus.Rejected, "đã bị từ chối" },
            { RequestStatus.Completed, "đã hoàn thành" },
            { RequestStatus.Cancelled, "đã bị hủy" }
        };
        
        // ✅ Gửi thông báo cho user
        await _notificationService.CreateAndSendNotificationAsync(
            userId: request.UserId,
            title: "Cập nhật yêu cầu",
            message: $"Yêu cầu #{request.Id} {statusMessages[dto.Status]}",
            referenceType: NotificationReferenceType.Request,
            referenceId: request.Id,
            cancellationToken: cancellationToken
        );
        
        return _mapper.Map<RequestResponseDTO>(request);
    }
}
```

### 🔔 Ví dụ 2: OrderService.cs

```csharp
namespace BLL.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly INotificationService _notificationService; // ✅ THÊM
    
    public OrderService(
        IOrderRepository orderRepository,
        INotificationService notificationService) // ✅ THÊM
    {
        _orderRepository = orderRepository;
        _notificationService = notificationService;
    }
    
    // Khi tạo order
    public async Task<OrderResponseDTO> CreateOrder(...)
    {
        // ... logic tạo order ...
        
        // Gửi thông báo cho customer
        await _notificationService.CreateAndSendNotificationAsync(
            userId: order.UserId,
            title: "Đơn hàng mới",
            message: $"Đơn hàng #{order.Id} đã được tạo thành công. Tổng tiền: {order.TotalAmount:N0}₫",
            referenceType: NotificationReferenceType.Order,
            referenceId: order.Id
        );
        
        // Gửi thông báo cho vendor (nếu có)
        if (order.VendorId.HasValue)
        {
            await _notificationService.CreateAndSendNotificationAsync(
                userId: order.VendorId.Value,
                title: "Đơn hàng mới",
                message: $"Bạn có đơn hàng mới #{order.Id}",
                referenceType: NotificationReferenceType.Order,
                referenceId: order.Id
            );
        }
        
        return orderDto;
    }
    
    // Khi cập nhật status
    public async Task<OrderResponseDTO> UpdateOrderStatus(ulong orderId, OrderStatus newStatus)
    {
        // ... logic update ...
        
        var messages = new Dictionary<OrderStatus, string>
        {
            { OrderStatus.Processing, "đang được xử lý" },
            { OrderStatus.Paid, "đã được thanh toán" },
            { OrderStatus.Shipped, "đã được giao cho đơn vị vận chuyển" },
            { OrderStatus.Delivered, "đã được giao thành công" },
            { OrderStatus.Cancelled, "đã bị hủy" },
            { OrderStatus.Refunded, "đã được hoàn tiền" }
        };
        
        await _notificationService.CreateAndSendNotificationAsync(
            userId: order.UserId,
            title: "Cập nhật đơn hàng",
            message: $"Đơn hàng #{orderId} {messages[newStatus]}",
            referenceType: NotificationReferenceType.Order,
            referenceId: orderId
        );
        
        return orderDto;
    }
}
```

### 🔔 Ví dụ 3: PaymentService.cs (trong folder Payment)

```csharp
// Khi payment thành công
public async Task HandlePaymentSuccess(ulong orderId)
{
    // ... logic xử lý payment ...
    
    await _notificationService.CreateAndSendNotificationAsync(
        userId: order.UserId,
        title: "Thanh toán thành công",
        message: $"Thanh toán cho đơn hàng #{orderId} đã được xử lý thành công. Số tiền: {payment.Amount:N0}₫",
        referenceType: NotificationReferenceType.Payment,
        referenceId: payment.Id
    );
}

// Khi payment thất bại
public async Task HandlePaymentFailed(ulong orderId, string reason)
{
    await _notificationService.CreateAndSendNotificationAsync(
        userId: order.UserId,
        title: "Thanh toán thất bại",
        message: $"Thanh toán cho đơn hàng #{orderId} không thành công. Lý do: {reason}",
        referenceType: NotificationReferenceType.Payment,
        referenceId: orderId
    );
}
```

### 🔔 Ví dụ 4: CashoutService.cs

```csharp
// Khi cashout được xử lý
public async Task ProcessCashout(ulong cashoutId, CashoutStatus status)
{
    // ... logic xử lý ...
    
    var messages = new Dictionary<CashoutStatus, string>
    {
        { CashoutStatus.Processing, "đang được xử lý" },
        { CashoutStatus.Completed, "đã hoàn thành. Tiền sẽ được chuyển trong vòng 1-3 ngày làm việc" },
        { CashoutStatus.Failed, "không thành công" },
        { CashoutStatus.Cancelled, "đã bị hủy" }
    };
    
    await _notificationService.CreateAndSendNotificationAsync(
        userId: cashout.UserId,
        title: "Cập nhật rút tiền",
        message: $"Yêu cầu rút tiền #{cashoutId} {messages[status]}",
        referenceType: NotificationReferenceType.Cashout,
        referenceId: cashoutId
    );
}
```

### 🔔 Ví dụ 5: ProductRegistrationService.cs

```csharp
// Khi sản phẩm được duyệt/từ chối
public async Task ProcessProductRegistration(ulong productRegId, ProductRegistrationStatus status)
{
    // ... logic xử lý ...
    
    var messages = new Dictionary<ProductRegistrationStatus, string>
    {
        { ProductRegistrationStatus.Approved, "Sản phẩm của bạn đã được phê duyệt và có thể bán trên hệ thống" },
        { ProductRegistrationStatus.Rejected, "Sản phẩm của bạn không được phê duyệt. Vui lòng kiểm tra lý do" }
    };
    
    await _notificationService.CreateAndSendNotificationAsync(
        userId: productReg.VendorId,
        title: status == ProductRegistrationStatus.Approved ? "Sản phẩm được duyệt" : "Sản phẩm bị từ chối",
        message: messages[status],
        referenceType: NotificationReferenceType.ProductRegistration,
        referenceId: productRegId
    );
}
```

### 📋 Bảng tổng hợp các trường hợp sử dụng

| Service | Sự kiện | Người nhận | ReferenceType |
|---------|---------|-----------|---------------|
| **RequestService** | Tạo request | User | Request |
| | Xử lý request | User | Request |
| **OrderService** | Tạo order | Customer + Vendor | Order |
| | Cập nhật status | Customer | Order |
| **PaymentService** | Thanh toán thành công | Customer | Payment |
| | Thanh toán thất bại | Customer | Payment |
| **CashoutService** | Xử lý cashout | Vendor | Cashout |
| **ProductRegistrationService** | Duyệt/Từ chối | Vendor | ProductRegistration |
| **ProductReviewService** | Review mới | Vendor | - |
| **VendorCertificateService** | Xác minh certificate | Vendor | - |
| **ProductCertificateService** | Xác minh certificate | Vendor | - |

---

## 9. CLIENT-SIDE IMPLEMENTATION (TYPESCRIPT THUẦN)

### 🎯 Tổng quan

Phần này hướng dẫn tích hợp SignalR cho frontend TypeScript thuần (không React/Vue/Angular).

**Cấu trúc file:**
```
frontend/
├── src/
│   ├── services/
│   │   └── NotificationService.ts       ✅ SignalR connection
│   ├── managers/
│   │   └── NotificationManager.ts       ✅ State & UI management
│   ├── types/
│   │   └── notification.types.ts        ✅ Type definitions
│   ├── utils/
│   │   └── toast.ts                     ✅ Toast notification helper
│   └── main.ts                          ✅ App entry point
├── public/
│   └── index.html                       ✅ HTML structure
└── styles/
    └── notifications.css                ✅ Styling
```

---

### 📦 Bước 9.1: Cài đặt package

```bash
npm install @microsoft/signalr
```

Hoặc thêm vào `package.json`:
```json
{
  "dependencies": {
    "@microsoft/signalr": "^8.0.0"
  },
  "devDependencies": {
    "typescript": "^5.0.0"
  }
}
```

### 📄 Bước 9.1.1: Cấu hình Environment Variables

**File: `.env`**

```env
VITE_API_BASE_URL=https://sep490.onrender.com
```

**Lưu ý:**
- Với Vite, biến môi trường phải có prefix `VITE_` để được expose cho client
- SignalR Hub URL sẽ được tạo từ `VITE_API_BASE_URL`: `${VITE_API_BASE_URL}/hubs/notification`

---

### 📄 Bước 9.2: Tạo Type Definitions

**File: `src/types/notification.types.ts`**

```typescript
/**
 * Interface cho Notification từ backend
 */
export interface Notification {
    id: number;
    userId: number;
    title: string;
    message: string;
    referenceType: NotificationReferenceType | null;
    referenceId: number | null;
    isRead: boolean;
    createdAt: string;
    updatedAt?: string;
}

/**
 * Enum các loại reference (phải khớp với backend)
 */
export enum NotificationReferenceType {
    Order = "Order",
    Payment = "Payment",
    Request = "Request",
    ForumPost = "ForumPost",
    ChatbotConversation = "ChatbotConversation",
    Cashout = "Cashout",
    ProductRegistration = "ProductRegistration",
    EnvironmentalData = "EnvironmentalData"
}

/**
 * Callback khi nhận notification mới
 */
export type NotificationCallback = (notification: Notification) => void;

/**
 * Connection state
 */
export enum ConnectionState {
    Disconnected = "Disconnected",
    Connecting = "Connecting",
    Connected = "Connected",
    Reconnecting = "Reconnecting"
}
```

---

### 📄 Bước 9.3: Tạo NotificationService

**File: `src/services/NotificationService.ts`**

```typescript
import * as signalR from "@microsoft/signalr";
import { 
    Notification, 
    NotificationCallback, 
    ConnectionState 
} from "../types/notification.types";

/**
 * Service quản lý kết nối SignalR và nhận thông báo real-time
 */
class NotificationService {
    private connection: signalR.HubConnection | null = null;
    private token: string;
    private hubUrl: string;
    private listeners: NotificationCallback[] = [];
    private connectionStateCallbacks: ((state: ConnectionState) => void)[] = [];
    private currentState: ConnectionState = ConnectionState.Disconnected;

    constructor(token: string, hubUrl?: string) {
        this.token = token;
        const baseUrl = import.meta.env.VITE_API_BASE_URL;
        this.hubUrl = hubUrl || `${baseUrl}/hubs/notification`;
    }

    /**
     * Khởi tạo và kết nối tới SignalR Hub
     */
    async start(): Promise<void> {
        if (this.connection) {
            console.log("[SignalR] Already connected");
            return;
        }

        this.updateConnectionState(ConnectionState.Connecting);

        // Tạo connection với cấu hình
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.hubUrl, {
                // ✅ accessTokenFactory gửi JWT token qua Query String
                // SignalR client tự động append: ?access_token=eyJhbGc...
                // WebSocket KHÔNG thể gửi Authorization header khi handshake!
                accessTokenFactory: () => this.token,
                transport: signalR.HttpTransportType.WebSockets,
                skipNegotiation: false
            })
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: (context) => {
                    // Exponential backoff: 0s, 2s, 10s, 30s
                    if (context.previousRetryCount === 0) return 0;
                    if (context.previousRetryCount === 1) return 2000;
                    if (context.previousRetryCount === 2) return 10000;
                    return 30000;
                }
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Đăng ký event handlers
        this.setupEventHandlers();

        // Kết nối
        try {
            await this.connection.start();
            this.updateConnectionState(ConnectionState.Connected);
            console.log("[SignalR] ✅ Connected successfully");
            
            // Test ping
            const pingResult = await this.connection.invoke<string>("Ping");
            console.log("[SignalR] 🏓 Ping result:", pingResult);
        } catch (err) {
            this.updateConnectionState(ConnectionState.Disconnected);
            console.error("[SignalR] ❌ Connection failed:", err);
            throw err;
        }
    }

    /**
     * Ngắt kết nối
     */
    async stop(): Promise<void> {
        if (!this.connection) return;

        try {
            await this.connection.stop();
            this.updateConnectionState(ConnectionState.Disconnected);
            console.log("[SignalR] Disconnected");
        } catch (err) {
            console.error("[SignalR] Disconnect error:", err);
        } finally {
            this.connection = null;
            this.listeners = [];
        }
    }

    /**
     * Đăng ký các event handlers
     */
    private setupEventHandlers(): void {
        if (!this.connection) return;

        // ✅ Lắng nghe thông báo mới từ server
        this.connection.on("ReceiveNotification", (notification: Notification) => {
            console.log("[SignalR] 🔔 Received notification:", notification);
            
            // Gọi tất cả listeners đã đăng ký
            this.listeners.forEach(listener => {
                try {
                    listener(notification);
                } catch (err) {
                    console.error("[SignalR] Error in listener:", err);
                }
            });
        });

        // Lắng nghe khi notification đã được đánh dấu đã đọc
        this.connection.on("NotificationMarkedAsRead", (notificationId: number) => {
            console.log("[SignalR] Notification marked as read:", notificationId);
        });

        // Lắng nghe error message từ server
        this.connection.on("Error", (errorMessage: string) => {
            console.error("[SignalR] Server error:", errorMessage);
        });

        // Khi reconnecting
        this.connection.onreconnecting((error) => {
            this.updateConnectionState(ConnectionState.Reconnecting);
            console.warn("[SignalR] 🔄 Reconnecting...", error?.message);
        });

        // Khi reconnected
        this.connection.onreconnected((connectionId) => {
            this.updateConnectionState(ConnectionState.Connected);
            console.log("[SignalR] ✅ Reconnected:", connectionId);
        });

        // Khi connection bị đóng
        this.connection.onclose((error) => {
            this.updateConnectionState(ConnectionState.Disconnected);
            console.error("[SignalR] ❌ Connection closed:", error?.message);
        });
    }

    /**
     * Đăng ký listener để nhận thông báo mới
     * Returns unsubscribe function
     */
    onNotification(callback: NotificationCallback): () => void {
        this.listeners.push(callback);
        
        // Return unsubscribe function
        return () => {
            this.listeners = this.listeners.filter(l => l !== callback);
        };
    }

    /**
     * Đăng ký listener cho connection state changes
     */
    onConnectionStateChange(callback: (state: ConnectionState) => void): () => void {
        this.connectionStateCallbacks.push(callback);
        
        // Gọi ngay lập tức với state hiện tại
        callback(this.currentState);
        
        // Return unsubscribe function
        return () => {
            this.connectionStateCallbacks = this.connectionStateCallbacks.filter(c => c !== callback);
        };
    }

    /**
     * Update connection state và notify callbacks
     */
    private updateConnectionState(newState: ConnectionState): void {
        this.currentState = newState;
        this.connectionStateCallbacks.forEach(callback => {
            try {
                callback(newState);
            } catch (err) {
                console.error("[SignalR] Error in connection state callback:", err);
            }
        });
    }

    /**
     * Đánh dấu notification đã đọc (gọi method trên server)
     */
    async markAsRead(notificationId: number): Promise<void> {
        if (!this.connection || !this.isConnected) {
            throw new Error("Not connected to SignalR");
        }

        try {
            await this.connection.invoke("MarkNotificationAsRead", notificationId);
            console.log("[SignalR] ✅ Marked notification as read:", notificationId);
        } catch (err) {
            console.error("[SignalR] ❌ Error marking as read:", err);
            throw err;
        }
    }

    /**
     * Test connection (ping server)
     */
    async ping(): Promise<string> {
        if (!this.connection || !this.isConnected) {
            throw new Error("Not connected to SignalR");
        }

        try {
            const result = await this.connection.invoke<string>("Ping");
            console.log("[SignalR] 🏓 Ping result:", result);
            return result;
        } catch (err) {
            console.error("[SignalR] ❌ Ping error:", err);
            throw err;
        }
    }

    /**
     * Update JWT token (dùng khi refresh token)
     */
    updateToken(newToken: string): void {
        this.token = newToken;
        console.log("[SignalR] Token updated");
    }

    /**
     * Kiểm tra trạng thái kết nối
     */
    get isConnected(): boolean {
        return this.connection?.state === signalR.HubConnectionState.Connected;
    }

    /**
     * Lấy connection state hiện tại
     */
    get connectionState(): ConnectionState {
        return this.currentState;
    }

    /**
     * Lấy số lượng listeners hiện có
     */
    get listenerCount(): number {
        return this.listeners.length;
    }
}

export default NotificationService;
```

**📝 Giải thích:**
- ✅ Kết nối tới SignalR Hub với JWT token
- ✅ Auto-reconnect với exponential backoff
- ✅ Lắng nghe event "ReceiveNotification" từ server
- ✅ Quản lý nhiều listeners (observer pattern)
- ✅ Connection state management
- ✅ Error handling đầy đủ
- ✅ Support update token (khi refresh)

---

### 📄 Bước 9.4: Tạo Toast Utility (Hiển thị thông báo popup)

**File: `src/utils/toast.ts`**

```typescript
/**
 * Simple toast notification system (không cần thư viện)
 */
export class ToastManager {
    private container: HTMLDivElement | null = null;

    constructor() {
        this.createContainer();
    }

    /**
     * Tạo container chứa toasts
     */
    private createContainer(): void {
        this.container = document.createElement('div');
        this.container.id = 'toast-container';
        this.container.className = 'toast-container';
        document.body.appendChild(this.container);
    }

    /**
     * Hiển thị toast
     */
    show(title: string, message: string, type: 'info' | 'success' | 'warning' | 'error' = 'info', duration: number = 5000): void {
        if (!this.container) return;

        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-header">
                <span class="toast-icon">${this.getIcon(type)}</span>
                <strong class="toast-title">${this.escapeHtml(title)}</strong>
                <button class="toast-close" aria-label="Close">&times;</button>
            </div>
            <div class="toast-body">${this.escapeHtml(message)}</div>
        `;

        // Close button
        const closeBtn = toast.querySelector('.toast-close');
        closeBtn?.addEventListener('click', () => this.remove(toast));

        // Add to container
        this.container.appendChild(toast);

        // Trigger animation
        setTimeout(() => toast.classList.add('show'), 10);

        // Auto remove
        if (duration > 0) {
            setTimeout(() => this.remove(toast), duration);
        }
    }

    /**
     * Remove toast
     */
    private remove(toast: HTMLElement): void {
        toast.classList.remove('show');
        toast.classList.add('hide');
        setTimeout(() => toast.remove(), 300);
    }

    /**
     * Get icon theo type
     */
    private getIcon(type: string): string {
        const icons = {
            info: '🔔',
            success: '✅',
            warning: '⚠️',
            error: '❌'
        };
        return icons[type as keyof typeof icons] || '🔔';
    }

    /**
     * Escape HTML để tránh XSS
     */
    private escapeHtml(text: string): string {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// Singleton instance
export const toast = new ToastManager();
```

---

### 📄 Bước 9.5: Tạo NotificationManager (Quản lý State & UI)

**File: `src/managers/NotificationManager.ts`**

```typescript
import NotificationService from "../services/NotificationService";
import { Notification, ConnectionState } from "../types/notification.types";
import { toast } from "../utils/toast";

/**
 * Manager để quản lý notifications và UI
 */
export class NotificationManager {
    private service: NotificationService;
    private notifications: Notification[] = [];
    private unsubscribe: (() => void) | null = null;
    
    // DOM Elements
    private bellElement: HTMLElement | null = null;
    private badgeElement: HTMLElement | null = null;
    private dropdownElement: HTMLElement | null = null;
    private listElement: HTMLElement | null = null;
    private connectionIndicator: HTMLElement | null = null;

    constructor(
        service: NotificationService,
        bellId: string = "notification-bell"
    ) {
        this.service = service;
        this.bellElement = document.getElementById(bellId);
        
        if (!this.bellElement) {
            console.error(`[NotificationManager] Element #${bellId} not found`);
            return;
        }

        this.initializeElements();
        this.attachEventListeners();
        this.startListening();
    }

    /**
     * Khởi tạo các DOM elements
     */
    private initializeElements(): void {
        if (!this.bellElement) return;

        this.badgeElement = this.bellElement.querySelector('.notification-badge');
        this.dropdownElement = this.bellElement.querySelector('.notification-dropdown');
        this.listElement = this.bellElement.querySelector('.notification-list');
        this.connectionIndicator = this.bellElement.querySelector('.connection-indicator');
    }

    /**
     * Gắn event listeners
     */
    private attachEventListeners(): void {
        if (!this.bellElement) return;

        // Toggle dropdown khi click vào bell
        const toggleBtn = this.bellElement.querySelector('.notification-toggle');
        toggleBtn?.addEventListener('click', () => this.toggleDropdown());

        // Close dropdown khi click outside
        document.addEventListener('click', (e) => {
            if (this.bellElement && !this.bellElement.contains(e.target as Node)) {
                this.closeDropdown();
            }
        });

        // Connection state indicator
        this.service.onConnectionStateChange((state) => {
            this.updateConnectionIndicator(state);
        });
    }

    /**
     * Bắt đầu lắng nghe notifications
     */
    private startListening(): void {
        // Lắng nghe notifications từ SignalR
        this.unsubscribe = this.service.onNotification((notification) => {
            console.log("[NotificationManager] Received:", notification);
            
            // Thêm vào danh sách
            this.notifications.unshift(notification);
            
            // Giới hạn 100 notifications trong memory
            if (this.notifications.length > 100) {
                this.notifications = this.notifications.slice(0, 100);
            }
            
            // Update UI
            this.updateBadge();
            this.updateList();
            
            // Hiển thị toast
            toast.show(notification.title, notification.message, 'info', 5000);
        });
    }

    /**
     * Dừng listening (cleanup)
     */
    destroy(): void {
        if (this.unsubscribe) {
            this.unsubscribe();
            this.unsubscribe = null;
        }
    }

    /**
     * Toggle dropdown
     */
    private toggleDropdown(): void {
        if (!this.dropdownElement) return;
        
        const isOpen = this.dropdownElement.classList.contains('show');
        if (isOpen) {
            this.closeDropdown();
        } else {
            this.openDropdown();
        }
    }

    /**
     * Mở dropdown
     */
    private openDropdown(): void {
        if (!this.dropdownElement) return;
        
        this.dropdownElement.classList.add('show');
        this.updateList();
    }

    /**
     * Đóng dropdown
     */
    private closeDropdown(): void {
        if (!this.dropdownElement) return;
        this.dropdownElement.classList.remove('show');
    }

    /**
     * Update badge count
     */
    private updateBadge(): void {
        if (!this.badgeElement) return;
        
        const unreadCount = this.notifications.filter(n => !n.isRead).length;
        
        if (unreadCount > 0) {
            this.badgeElement.textContent = unreadCount > 99 ? '99+' : unreadCount.toString();
            this.badgeElement.classList.add('show');
        } else {
            this.badgeElement.classList.remove('show');
        }
    }

    /**
     * Update notification list trong dropdown
     */
    private updateList(): void {
        if (!this.listElement) return;
        
        if (this.notifications.length === 0) {
            this.listElement.innerHTML = '<div class="notification-empty">Không có thông báo mới</div>';
            return;
        }
        
        this.listElement.innerHTML = this.notifications
            .map(notification => this.renderNotificationItem(notification))
            .join('');
        
        // Gắn event listeners cho từng item
        this.listElement.querySelectorAll('.notification-item').forEach((item, index) => {
            item.addEventListener('click', () => this.handleNotificationClick(this.notifications[index]));
        });
    }

    /**
     * Render một notification item
     */
    private renderNotificationItem(notification: Notification): string {
        const date = new Date(notification.createdAt).toLocaleString('vi-VN');
        const readClass = notification.isRead ? 'read' : 'unread';
        
        return `
            <div class="notification-item ${readClass}" data-id="${notification.id}">
                <div class="notification-header">
                    <strong class="notification-title">${this.escapeHtml(notification.title)}</strong>
                    <span class="notification-time">${date}</span>
                </div>
                <div class="notification-message">${this.escapeHtml(notification.message)}</div>
                ${notification.referenceType ? `<div class="notification-type">${notification.referenceType}</div>` : ''}
            </div>
        `;
    }

    /**
     * Handle click vào notification
     */
    private async handleNotificationClick(notification: Notification): Promise<void> {
        // Mark as read
        if (!notification.isRead) {
            try {
                await this.markAsRead(notification.id);
            } catch (err) {
                console.error("[NotificationManager] Failed to mark as read:", err);
            }
        }
        
        // Navigate theo referenceType nếu có
        if (notification.referenceType && notification.referenceId) {
            this.handleNavigation(notification);
        }
    }

    /**
     * Đánh dấu đã đọc
     */
    async markAsRead(notificationId: number): Promise<void> {
        try {
            await this.service.markAsRead(notificationId);
            
            // Update local state
            const notification = this.notifications.find(n => n.id === notificationId);
            if (notification) {
                notification.isRead = true;
                this.updateBadge();
                this.updateList();
            }
        } catch (err) {
            throw err;
        }
    }

    /**
     * Xử lý navigation theo reference type
     */
    private handleNavigation(notification: Notification): void {
        const routes: Record<string, string> = {
            'Order': `/orders/${notification.referenceId}`,
            'Payment': `/payments/${notification.referenceId}`,
            'Request': `/requests/${notification.referenceId}`,
            'ProductRegistration': `/products/registrations/${notification.referenceId}`,
            'Cashout': `/cashouts/${notification.referenceId}`
        };
        
        const route = routes[notification.referenceType || ''];
        if (route) {
            console.log(`[NotificationManager] Navigate to: ${route}`);
            // window.location.href = route; // Uncomment để navigate thật
            
            // Hoặc nếu dùng router:
            // router.push(route);
        }
    }

    /**
     * Update connection indicator
     */
    private updateConnectionIndicator(state: ConnectionState): void {
        if (!this.connectionIndicator) return;
        
        const stateClasses: Record<ConnectionState, string> = {
            [ConnectionState.Connected]: 'connected',
            [ConnectionState.Connecting]: 'connecting',
            [ConnectionState.Reconnecting]: 'reconnecting',
            [ConnectionState.Disconnected]: 'disconnected'
        };
        
        // Remove all state classes
        Object.values(stateClasses).forEach(cls => {
            this.connectionIndicator?.classList.remove(cls);
        });
        
        // Add current state class
        this.connectionIndicator.classList.add(stateClasses[state]);
        
        // Update tooltip
        const stateTexts: Record<ConnectionState, string> = {
            [ConnectionState.Connected]: 'Đã kết nối',
            [ConnectionState.Connecting]: 'Đang kết nối...',
            [ConnectionState.Reconnecting]: 'Đang kết nối lại...',
            [ConnectionState.Disconnected]: 'Mất kết nối'
        };
        
        this.connectionIndicator.setAttribute('title', stateTexts[state]);
    }

    /**
     * Escape HTML
     */
    private escapeHtml(text: string): string {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    /**
     * Get unread count
     */
    get unreadCount(): number {
        return this.notifications.filter(n => !n.isRead).length;
    }

    /**
     * Get all notifications
     */
    getAllNotifications(): Notification[] {
        return [...this.notifications];
    }

    /**
     * Get unread notifications
     */
    getUnreadNotifications(): Notification[] {
        return this.notifications.filter(n => !n.isRead);
    }
}
```

---

### 📄 Bước 9.6: HTML Structure

**File: `public/index.html`**

```html
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>VerdantTech - Real-time Notifications</title>
    <link rel="stylesheet" href="/styles/notifications.css">
</head>
<body>
    <!-- Navigation Bar -->
    <nav class="navbar">
        <div class="navbar-brand">
            <h1>🌱 VerdantTech</h1>
        </div>
        
        <div class="navbar-actions">
            <!-- Notification Bell -->
            <div id="notification-bell" class="notification-bell">
                <button class="notification-toggle" aria-label="Notifications">
                    <svg class="bell-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
                        <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
                    </svg>
                    <span class="notification-badge"></span>
                    <span class="connection-indicator" title="Connection status"></span>
                </button>
                
                <!-- Dropdown -->
                <div class="notification-dropdown">
                    <div class="notification-header">
                        <h3>Thông báo</h3>
                        <button class="mark-all-read" style="display:none;">Đánh dấu tất cả đã đọc</button>
                    </div>
                    <div class="notification-list"></div>
                </div>
            </div>
            
            <!-- User Menu -->
            <div class="user-menu">
                <button class="user-avatar">
                    <img src="/images/avatar.png" alt="User" />
                </button>
            </div>
        </div>
    </nav>
    
    <!-- Main Content -->
    <main class="main-content">
        <div class="container">
            <h2>Dashboard</h2>
            <p>Real-time notifications đang hoạt động! Kiểm tra console log.</p>
            
            <!-- Test Buttons -->
            <div class="test-buttons">
                <button id="btn-test-connection">🏓 Test Connection</button>
                <button id="btn-test-notification">🔔 Test Notification</button>
            </div>
        </div>
    </main>
    
    <!-- Toast Container (tự động tạo bởi ToastManager) -->
    
    <!-- Scripts -->
    <script type="module" src="/src/main.ts"></script>
</body>
</html>
```

---

### 📄 Bước 9.7: CSS Styling

**File: `styles/notifications.css`**

```css
/* ================================
   GENERAL STYLES
   ================================ */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
    background-color: #f5f5f5;
    color: #333;
}

/* ================================
   NAVBAR
   ================================ */
.navbar {
    background: #ffffff;
    border-bottom: 1px solid #e0e0e0;
    padding: 1rem 2rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
    box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

.navbar-brand h1 {
    font-size: 1.5rem;
    color: #2e7d32;
    margin: 0;
}

.navbar-actions {
    display: flex;
    align-items: center;
    gap: 1rem;
}

/* ================================
   NOTIFICATION BELL
   ================================ */
.notification-bell {
    position: relative;
}

.notification-toggle {
    position: relative;
    background: none;
    border: none;
    cursor: pointer;
    padding: 0.5rem;
    border-radius: 50%;
    transition: background-color 0.2s;
    display: flex;
    align-items: center;
    justify-content: center;
}

.notification-toggle:hover {
    background-color: #f5f5f5;
}

.bell-icon {
    width: 24px;
    height: 24px;
    stroke-width: 2;
}

/* Badge */
.notification-badge {
    position: absolute;
    top: 0;
    right: 0;
    background: #f44336;
    color: white;
    font-size: 0.75rem;
    font-weight: bold;
    padding: 0.125rem 0.375rem;
    border-radius: 10px;
    min-width: 18px;
    text-align: center;
    display: none;
}

.notification-badge.show {
    display: block;
}

/* Connection Indicator */
.connection-indicator {
    position: absolute;
    bottom: 0;
    right: 0;
    width: 10px;
    height: 10px;
    border-radius: 50%;
    border: 2px solid white;
}

.connection-indicator.connected {
    background-color: #4caf50;
}

.connection-indicator.connecting {
    background-color: #ff9800;
    animation: pulse 1.5s ease-in-out infinite;
}

.connection-indicator.reconnecting {
    background-color: #ff9800;
    animation: pulse 1.5s ease-in-out infinite;
}

.connection-indicator.disconnected {
    background-color: #f44336;
}

@keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.5; }
}

/* ================================
   NOTIFICATION DROPDOWN
   ================================ */
.notification-dropdown {
    position: absolute;
    top: calc(100% + 0.5rem);
    right: 0;
    width: 400px;
    max-height: 500px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.15);
    display: none;
    flex-direction: column;
    z-index: 1000;
}

.notification-dropdown.show {
    display: flex;
}

.notification-header {
    padding: 1rem;
    border-bottom: 1px solid #e0e0e0;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.notification-header h3 {
    font-size: 1.125rem;
    margin: 0;
}

.mark-all-read {
    background: none;
    border: none;
    color: #2e7d32;
    cursor: pointer;
    font-size: 0.875rem;
}

.mark-all-read:hover {
    text-decoration: underline;
}

/* Notification List */
.notification-list {
    overflow-y: auto;
    max-height: 400px;
}

.notification-empty {
    padding: 2rem;
    text-align: center;
    color: #999;
}

/* Notification Item */
.notification-item {
    padding: 1rem;
    border-bottom: 1px solid #f0f0f0;
    cursor: pointer;
    transition: background-color 0.2s;
}

.notification-item:hover {
    background-color: #f9f9f9;
}

.notification-item.unread {
    background-color: #e3f2fd;
}

.notification-item.unread:hover {
    background-color: #bbdefb;
}

.notification-item:last-child {
    border-bottom: none;
}

.notification-item .notification-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    padding: 0;
    border: none;
    margin-bottom: 0.5rem;
}

.notification-title {
    font-size: 0.9375rem;
    color: #333;
    flex: 1;
}

.notification-time {
    font-size: 0.75rem;
    color: #999;
    white-space: nowrap;
    margin-left: 0.5rem;
}

.notification-message {
    font-size: 0.875rem;
    color: #666;
    line-height: 1.4;
}

.notification-type {
    display: inline-block;
    margin-top: 0.5rem;
    padding: 0.125rem 0.5rem;
    background: #e8f5e9;
    color: #2e7d32;
    font-size: 0.75rem;
    border-radius: 12px;
}

/* ================================
   TOAST NOTIFICATIONS
   ================================ */
.toast-container {
    position: fixed;
    top: 1rem;
    right: 1rem;
    z-index: 9999;
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
}

.toast {
    min-width: 300px;
    max-width: 400px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    overflow: hidden;
    opacity: 0;
    transform: translateX(400px);
    transition: all 0.3s ease-in-out;
}

.toast.show {
    opacity: 1;
    transform: translateX(0);
}

.toast.hide {
    opacity: 0;
    transform: translateX(400px);
}

.toast-header {
    display: flex;
    align-items: center;
    padding: 0.75rem 1rem;
    border-bottom: 1px solid #f0f0f0;
}

.toast-icon {
    font-size: 1.25rem;
    margin-right: 0.5rem;
}

.toast-title {
    flex: 1;
    font-size: 0.9375rem;
}

.toast-close {
    background: none;
    border: none;
    font-size: 1.5rem;
    line-height: 1;
    cursor: pointer;
    color: #999;
}

.toast-close:hover {
    color: #333;
}

.toast-body {
    padding: 0.75rem 1rem;
    font-size: 0.875rem;
    color: #666;
}

/* Toast Types */
.toast-info {
    border-left: 4px solid #2196f3;
}

.toast-success {
    border-left: 4px solid #4caf50;
}

.toast-warning {
    border-left: 4px solid #ff9800;
}

.toast-error {
    border-left: 4px solid #f44336;
}

/* ================================
   MAIN CONTENT
   ================================ */
.main-content {
    padding: 2rem;
}

.container {
    max-width: 1200px;
    margin: 0 auto;
}

.test-buttons {
    margin-top: 2rem;
    display: flex;
    gap: 1rem;
}

.test-buttons button {
    padding: 0.75rem 1.5rem;
    background: #2e7d32;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 1rem;
}

.test-buttons button:hover {
    background: #1b5e20;
}

/* ================================
   RESPONSIVE
   ================================ */
@media (max-width: 768px) {
    .notification-dropdown {
        width: 100vw;
        max-width: 100vw;
        right: -2rem;
        border-radius: 0;
    }
    
    .toast {
        min-width: calc(100vw - 2rem);
        max-width: calc(100vw - 2rem);
    }
    
    .toast-container {
        right: 1rem;
        left: 1rem;
    }
}
```

---

### 📄 Bước 9.8: Main Entry Point

**File: `src/main.ts`**

```typescript
import NotificationService from "./services/NotificationService";
import { NotificationManager } from "./managers/NotificationManager";

/**
 * Main application entry point
 */
class App {
    private notificationService: NotificationService | null = null;
    private notificationManager: NotificationManager | null = null;

    async init() {
        console.log("[App] Initializing...");
        
        // Lấy JWT token từ localStorage
        const token = this.getToken();
        
        if (!token) {
            console.warn("[App] No JWT token found. User not authenticated.");
            this.showLoginPrompt();
            return;
        }
        
        try {
            // Khởi tạo NotificationService
            this.notificationService = new NotificationService(token);
            
            // Kết nối SignalR
            await this.notificationService.start();
            
            // Khởi tạo NotificationManager (quản lý UI)
            this.notificationManager = new NotificationManager(this.notificationService);
            
            console.log("[App] ✅ Initialized successfully");
            
            // Setup test buttons
            this.setupTestButtons();
            
        } catch (err) {
            console.error("[App] ❌ Initialization failed:", err);
            alert("Không thể kết nối đến server. Vui lòng kiểm tra lại kết nối.");
        }
    }

    /**
     * Lấy JWT token từ localStorage
     */
    private getToken(): string | null {
        // Thử các key phổ biến
        const possibleKeys = ['jwt_token', 'token', 'auth_token', 'access_token'];
        
        for (const key of possibleKeys) {
            const token = localStorage.getItem(key);
            if (token) {
                console.log(`[App] Found token in localStorage.${key}`);
                return token;
            }
        }
        
        return null;
    }

    /**
     * Hiển thị login prompt
     */
    private showLoginPrompt(): void {
        const container = document.querySelector('.container');
        if (container) {
            container.innerHTML = `
                <div style="text-align: center; padding: 3rem;">
                    <h2>⚠️ Chưa đăng nhập</h2>
                    <p>Vui lòng đăng nhập để nhận thông báo real-time.</p>
                    <button onclick="window.location.href='/login'" style="margin-top: 1rem; padding: 0.75rem 1.5rem; background: #2e7d32; color: white; border: none; border-radius: 4px; cursor: pointer;">
                        Đăng nhập
                    </button>
                </div>
            `;
        }
    }

    /**
     * Setup test buttons
     */
    private setupTestButtons(): void {
        // Test connection button
        const testConnectionBtn = document.getElementById('btn-test-connection');
        testConnectionBtn?.addEventListener('click', async () => {
            if (!this.notificationService) return;
            
            try {
                const result = await this.notificationService.ping();
                alert(`✅ Connection OK!\n\n${result}`);
            } catch (err) {
                alert(`❌ Connection Failed!\n\n${err}`);
            }
        });
        
        // Test notification button (giả lập)
        const testNotificationBtn = document.getElementById('btn-test-notification');
        testNotificationBtn?.addEventListener('click', () => {
            // Giả lập một notification local (không qua server)
            const mockNotification = {
                id: Date.now(),
                userId: 1,
                title: "🧪 Test Notification",
                message: "Đây là notification test từ frontend. Notification thật sẽ đến từ server qua SignalR.",
                referenceType: null,
                referenceId: null,
                isRead: false,
                createdAt: new Date().toISOString()
            };
            
            console.log("[App] Mock notification:", mockNotification);
            alert("⚠️ Đây là test local. Để test thật, hãy gọi API từ backend để tạo notification.");
        });
    }

    /**
     * Cleanup khi tắt app
     */
    async destroy() {
        if (this.notificationManager) {
            this.notificationManager.destroy();
        }
        
        if (this.notificationService) {
            await this.notificationService.stop();
        }
    }
}

// Khởi tạo app khi DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        const app = new App();
        app.init();
        
        // Cleanup khi unload
        window.addEventListener('beforeunload', () => {
            app.destroy();
        });
    });
} else {
    const app = new App();
    app.init();
    
    window.addEventListener('beforeunload', () => {
        app.destroy();
    });
}
```

---

### 📄 Bước 9.9: TypeScript Configuration

**File: `tsconfig.json`**

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "module": "ESNext",
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "moduleResolution": "node",
    "esModuleInterop": true,
    "allowSyntheticDefaultImports": true,
    "strict": true,
    "skipLibCheck": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": false,
    "outDir": "./dist",
    "rootDir": "./src",
    "baseUrl": "./src",
    "paths": {
      "@/*": ["./*"]
    }
  },
  "include": ["src/**/*"],
  "exclude": ["node_modules", "dist"]
}
```

---

### 📄 Bước 9.10: Ví dụ Sử dụng Nâng cao

#### **Ví dụ 1: Tự động fetch notifications cũ khi load trang**

```typescript
// src/main.ts (thêm vào)
class App {
    // ...
    
    async init() {
        // ... existing code ...
        
        // Fetch notifications cũ từ API
        await this.loadExistingNotifications();
    }
    
    private async loadExistingNotifications(): Promise<void> {
        try {
            const userId = this.getUserIdFromToken();
            if (!userId) return;
            
            const baseUrl = import.meta.env.VITE_API_BASE_URL;
            const response = await fetch(`${baseUrl}/api/Notification/user/${userId}`, {
                headers: {
                    'Authorization': `Bearer ${this.getToken()}`
                }
            });
            
            const data = await response.json();
            
            if (data.isSuccess && data.data) {
                console.log(`[App] Loaded ${data.data.length} existing notifications`);
                // Có thể inject vào NotificationManager
            }
        } catch (err) {
            console.error("[App] Failed to load existing notifications:", err);
        }
    }
    
    private getUserIdFromToken(): number | null {
        const token = this.getToken();
        if (!token) return null;
        
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return parseInt(payload.nameid || payload.sub);
        } catch {
            return null;
        }
    }
}
```

#### **Ví dụ 2: Lắng nghe notification theo type cụ thể**

```typescript
// src/managers/NotificationManager.ts (thêm vào)
export class NotificationManager {
    // ...
    
    /**
     * Lắng nghe notification theo type cụ thể
     */
    onNotificationByType(
        type: string, 
        callback: (notification: Notification) => void
    ): () => void {
        return this.service.onNotification((notification) => {
            if (notification.referenceType === type) {
                callback(notification);
            }
        });
    }
}

// Sử dụng:
notificationManager.onNotificationByType('Order', (notification) => {
    console.log('New order notification:', notification);
    // Refresh order list
    refreshOrderList();
});
```

#### **Ví dụ 3: Play sound khi có notification mới**

```typescript
// src/utils/sound.ts
export class SoundManager {
    private audio: HTMLAudioElement;
    private enabled: boolean = true;
    
    constructor(soundUrl: string = '/sounds/notification.mp3') {
        this.audio = new Audio(soundUrl);
    }
    
    play(): void {
        if (!this.enabled) return;
        
        this.audio.currentTime = 0;
        this.audio.play().catch(err => {
            console.error('[Sound] Failed to play:', err);
        });
    }
    
    toggle(): void {
        this.enabled = !this.enabled;
    }
    
    setEnabled(enabled: boolean): void {
        this.enabled = enabled;
    }
}

// Sử dụng trong NotificationManager:
import { SoundManager } from '../utils/sound';

export class NotificationManager {
    private sound: SoundManager;
    
    constructor(service: NotificationService, bellId: string = "notification-bell") {
        // ... existing code ...
        
        this.sound = new SoundManager();
    }
    
    private startListening(): void {
        this.unsubscribe = this.service.onNotification((notification) => {
            // ... existing code ...
            
            // Play sound
            this.sound.play();
        });
    }
}
```

---

## 10. TESTING

### 🧪 Bước 10.1: Test Backend

#### Test 1: Kiểm tra Hub endpoint

```bash
GET ${VITE_API_BASE_URL}/hubs/notification
```

Nếu trả về 401 Unauthorized → ✅ Đúng! (Vì cần JWT token)

#### Test 2: Test từ Service

Thêm test endpoint tạm trong NotificationController:

```csharp
[HttpPost("test-send")]
[Authorize]
public async Task<ActionResult<APIResponse>> TestSendNotification([FromQuery] ulong targetUserId)
{
    try
    {
        await _notificationService.CreateAndSendNotificationAsync(
            userId: targetUserId,
            title: "Test Notification",
            message: "This is a test notification",
            referenceType: null,
            referenceId: null
        );
        
        return SuccessResponse("Notification sent");
    }
    catch (Exception ex)
    {
        return HandleException(ex);
    }
}
```

Gọi API:
```bash
POST ${VITE_API_BASE_URL}/api/Notification/test-send?targetUserId=1
Authorization: Bearer YOUR_JWT_TOKEN
```

Kiểm tra console log xem có "[NotificationHub] Sent notification to User X" không.

#### Test 3: Kiểm tra Database

```sql
SELECT * FROM notifications ORDER BY created_at DESC LIMIT 10;
```

### 🧪 Bước 10.2: Test Frontend

#### Test 1: Kết nối SignalR

Mở Developer Console trong browser:
```
[SignalR] Connected successfully
[SignalR] Ping result: Pong from User 1 (Role: Customer)
```

#### Test 2: Nhận notification

1. Mở 2 tab browser (User A và User B)
2. Từ Postman, gửi notification cho User A
3. Kiểm tra Tab 1 có nhận được toast notification không

#### Test 3: Test reconnection

1. Connect thành công
2. Tắt backend server
3. Bật lại
4. Kiểm tra auto-reconnect

---

## 11. TROUBLESHOOTING

### ❌ Lỗi 1: "Connection refused" hoặc 404

**Giải pháp:**
```csharp
// Đảm bảo có dòng này trước app.Run()
app.MapHub<NotificationHub>("/hubs/notification");
```

### ❌ Lỗi 2: "401 Unauthorized" khi kết nối SignalR

**Nguyên nhân:** JWT middleware không đọc được token từ query string

**Giải pháp:** Thêm `OnMessageReceived` event vào JWT configuration trong `Program.cs`:

```csharp
.AddJwtBearer(options =>
{
    // ... existing config ...
    
    options.Events = new JwtBearerEvents
    {
        // ✅ Đọc token từ query string cho WebSocket
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            // Chỉ áp dụng cho SignalR Hub endpoints
            if (!string.IsNullOrEmpty(accessToken) && 
                path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        
        // ... existing OnChallenge, OnForbidden ...
    };
});
```

**📝 Giải thích chi tiết:**

```
REST API Request:
  ✅ Client gửi: Authorization: Bearer eyJhbGc...
  ✅ Middleware đọc: Từ Authorization header
  ✅ Kết quả: Hoạt động bình thường

SignalR WebSocket Connection:
  ❌ Client KHÔNG thể gửi Authorization header!
  ✅ Client gửi: wss://${VITE_API_BASE_URL}/hubs/notification?access_token=eyJhbGc...
  ❌ Middleware chỉ đọc: Từ Authorization header (KHÔNG tìm thấy!)
  ❌ Kết quả: 401 Unauthorized
  
  ✅ Giải pháp: OnMessageReceived event
  ✅ Đọc token từ: context.Request.Query["access_token"]
  ✅ Gán vào: context.Token = accessToken
  ✅ Kết quả: JWT middleware validate token như bình thường
```

**Tại sao WebSocket không gửi custom header?**
- WebSocket handshake (HTTP Upgrade request) chỉ gửi các headers cốt lõi:
  - `Upgrade: websocket`
  - `Connection: Upgrade`
  - `Sec-WebSocket-Key: ...`
  - `Sec-WebSocket-Version: 13`
- Browser WebSocket API KHÔNG cho phép thêm custom headers (bảo mật)
- Đây là giới hạn của WebSocket protocol chuẩn (RFC 6455)
- Do đó SignalR và hầu hết WebSocket libraries sử dụng query string
```

### ❌ Lỗi 3: "Cannot find NotificationHub"

**Nguyên nhân:** Thiếu using statement

**Giải pháp:**
```csharp
// Controller/Program.cs
using Infrastructure.SignalR;
```

### 🔍 Debug Checklist

- [ ] Backend có log "[SignalR] User X connected" không?
- [ ] Frontend có log "[SignalR] Connected successfully" không?
- [ ] Service có gọi `CreateAndSendNotificationAsync()` không?
- [ ] NotificationHub có log "Sent notification to User X" không?
- [ ] Frontend có đăng ký listener "ReceiveNotification" không?
- [ ] JWT token còn valid không?
- [ ] Notification có được lưu vào database không?

---

## 📚 PHỤ LỤC

### A. Cấu trúc theo Clean Architecture

```
Controller/
└── Program.cs
    ├── using Infrastructure.SignalR
    ├── builder.Services.AddSignalR()
    ├── builder.Services.AddInfrastructure()
    └── app.MapHub<NotificationHub>()

Infrastructure/SignalR/
├── BaseHub.cs                    ← Hub base class
├── NotificationHub.cs            ← Hub endpoint
└── NotificationHubService.cs     ← Send messages

BLL/Interfaces/Infrastructure/
└── INotificationHub.cs           ← Contract (giống IEmailSender)

BLL/Services/
└── NotificationService.cs        ← Business logic
```

### B. So sánh với các Infrastructure Services khác

| Service | Interface Location | Implementation Location |
|---------|-------------------|------------------------|
| **Email** | BLL/Interfaces/Infrastructure/IEmailSender.cs | Infrastructure/Email/EmailSender.cs |
| **PayOS** | BLL/Interfaces/Infrastructure/IPayOSApiClient.cs | Infrastructure/Payment/PayOS/PayOSApiClient.cs |
| **Weather** | BLL/Interfaces/Infrastructure/IWeatherApiClient.cs | Infrastructure/Weather/WeatherApiClient.cs |
| **SignalR** | BLL/Interfaces/Infrastructure/INotificationHub.cs | Infrastructure/SignalR/NotificationHubService.cs |

**✅ HOÀN TOÀN CONSISTENT!**

### C. Enum NotificationReferenceType

```csharp
public enum NotificationReferenceType
{
    Order,                  // Đơn hàng
    Payment,               // Thanh toán
    Request,               // Yêu cầu hỗ trợ/hoàn tiền
    ForumPost,             // Bài đăng diễn đàn
    ChatbotConversation,   // Cuộc trò chuyện chatbot
    Cashout,               // Rút tiền
    ProductRegistration,   // Đăng ký sản phẩm
    EnvironmentalData      // Dữ liệu môi trường
}
```

### D. Redis Backplane (cho production với nhiều server)

```bash
dotnet add package Microsoft.AspNetCore.SignalR.StackExchangeRedis
```

```csharp
// Trong Program.cs
builder.Services.AddSignalR()
    .AddStackExchangeRedis(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING"), options =>
    {
        options.Configuration.ChannelPrefix = "VerdantTech";
    });
```

---

## ✅ CHECKLIST HOÀN THÀNH

### Backend - Infrastructure Layer
- [ ] Cài package SignalR cho Infrastructure
- [ ] Tạo folder `Infrastructure/SignalR/`
- [ ] Tạo `Infrastructure/SignalR/BaseHub.cs`
- [ ] Tạo `Infrastructure/SignalR/NotificationHub.cs`
- [ ] Tạo `Infrastructure/SignalR/NotificationHubService.cs`
- [ ] Cập nhật `Infrastructure/Extensions/ServiceCollectionExtensions.cs`

### Backend - Business Logic Layer
- [ ] Tạo `BLL/Interfaces/Infrastructure/INotificationHub.cs`
- [ ] Cập nhật `BLL/Interfaces/INotificationService.cs`
- [ ] Cập nhật `BLL/Services/NotificationService.cs`

### Backend - Presentation Layer
- [ ] Cài package SignalR cho Controller
- [ ] Cập nhật `Controller/Program.cs` - using statement
- [ ] Cập nhật `Controller/Program.cs` - AddSignalR()
- [ ] Cập nhật `Controller/Program.cs` - MapHub()

### Service Integration
- [ ] RequestService - CreateRequest & ProcessRequest
- [ ] OrderService - CreateOrder & UpdateStatus
- [ ] PaymentService - Success/Failed
- [ ] CashoutService - Process
- [ ] ProductRegistrationService - Approve/Reject

### Frontend
- [ ] Cài đặt `@microsoft/signalr`
- [ ] Tạo `NotificationService.ts`
- [ ] Tạo `useNotification` hook
- [ ] Tạo `NotificationBell` component
- [ ] Tích hợp vào App

### Testing
- [ ] Test backend endpoint
- [ ] Test frontend connection
- [ ] Test end-to-end scenarios
- [ ] Test reconnection
- [ ] Verify database records

---

## 🎯 TÓM TẮT KIẾN TRÚC

### **1. Phân lớp rõ ràng**

```
Controller/Program.cs              → Presentation (chỉ configure)
Infrastructure/SignalR/*           → Infrastructure (implementation)
BLL/Interfaces/Infrastructure/*    → Application (contracts)
BLL/Services/NotificationService   → Application (business logic)
DAL/Repository/*                   → Domain (data access)
```

### **2. Dependencies**

```
✅ Controller → Infrastructure → BLL → DAL
✅ Không có circular dependency
✅ Dependencies luôn hướng vào trong (inward)
✅ 100% tuân thủ Clean Architecture
```

### **3. SignalR = External Service**

Giống như Email, Cloudinary, PayOS - đều là external communication mechanisms:
- Interface ở `BLL/Interfaces/Infrastructure/`
- Implementation ở `Infrastructure/`
- Registration qua `AddInfrastructure()`

### **4. Controller KHÔNG chứa code Hub**

Controller chỉ:
- ⚙️ Configure SignalR
- ⚙️ Map Hub endpoint
- ❌ KHÔNG có implementation code

---

**Chúc bạn tích hợp thành công! 🚀**

*Document version: 4.0 - FINAL*  
*Last updated: 2025-11-14*  
*Changes: Hub và Service hoàn toàn ở Infrastructure, Interface ở BLL/Interfaces/Infrastructure*
