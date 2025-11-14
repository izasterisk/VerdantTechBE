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

---

## 2. KIẾN TRÚC CLEAN ARCHITECTURE & SIGNALR

### 🏛️ **VỊ TRÍ CỦA SIGNALR TRONG KIẾN TRÚC**

SignalR được coi là **External Service/Communication Mechanism** - thuộc **Infrastructure Layer**!

```
┌──────────────────────────────────────────────┐
│   PRESENTATION LAYER (Controller/)           │
│   ├── Controllers/                           │
│   │   ├── NotificationController.cs          │
│   │   ├── OrderController.cs                 │
│   │   └── BaseController.cs                  │
│   └── Hubs/                            ✅    │
│       ├── BaseHub.cs                   ✅    │
│       └── NotificationHub.cs           ✅    │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│   INFRASTRUCTURE LAYER (Infrastructure/)     │
│   ├── SignalR/                         ✅    │
│   │   └── NotificationHubService.cs    ✅    │
│   ├── Email/                                 │
│   ├── Cloudinary/                            │
│   └── Extensions/                            │
│       └── ServiceCollectionExtensions.cs ✅  │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│   BUSINESS LOGIC LAYER (BLL/)                │
│   ├── Services/                              │
│   │   └── NotificationService.cs             │
│   └── Interfaces/                            │
│       ├── INotificationService.cs            │
│       └── INotificationHub.cs          ✅    │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│   DATA ACCESS LAYER (DAL/)                   │
│   └── Repository/                            │
│       └── NotificationRepository.cs          │
└──────────────────────────────────────────────┘
```

### 🔐 **TẠI SAO PHẢI CÓ [Authorize] TRÊN HUB?**

**Điểm quan trọng:** SignalR Hub **KHÔNG đi qua Controller**!

```
❌ SAI: Client → Controller [Authorize] → Hub → Service
✅ ĐÚNG: Client → Hub [PHẢI Authorize RIÊNG] → Service
```

**HTTP Request Flow:**
```
Client → NotificationController [Authorize] → NotificationService → DB
```

**SignalR WebSocket Flow:**
```
Client → NotificationHub [PHẢI Authorize] → NotificationHubService → DB
```

**Kết luận:** `[Authorize]` trên Hub là **BẮT BUỘC** vì:
1. Hub là endpoint độc lập, không đi qua Controller
2. Client kết nối trực tiếp tới `/hubs/notification`
3. Bỏ `[Authorize]` = lỗ hổng bảo mật nghiêm trọng 🔴

### 🎯 **TẠI SAO ĐẶT Ở INFRASTRUCTURE?**

SignalR là **external communication mechanism**, giống như:
- ✅ Email service (SMTP)
- ✅ Cloud storage (Cloudinary)
- ✅ Payment gateway (PayOS)
- ✅ External APIs (Weather, Soil)

→ Tất cả đều nằm trong **Infrastructure layer**!

### 📂 Cấu trúc file sẽ tạo/cập nhật

```
BE/
├── Controller/                        (PRESENTATION LAYER)
│   ├── Controllers/
│   │   ├── BaseController.cs         (Có sẵn)
│   │   └── NotificationController.cs (Có sẵn)
│   ├── Hubs/                         ✅ TẠO FOLDER MỚI
│   │   ├── BaseHub.cs                ✅ TẠO MỚI
│   │   └── NotificationHub.cs        ✅ TẠO MỚI
│   └── Program.cs                    (CẬP NHẬT)
│
├── Infrastructure/                    (INFRASTRUCTURE LAYER)
│   ├── SignalR/                      ✅ TẠO FOLDER MỚI
│   │   └── NotificationHubService.cs ✅ TẠO MỚI
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs (CẬP NHẬT)
│   └── Infrastructure.csproj         (CẬP NHẬT)
│
├── BLL/                              (BUSINESS LOGIC LAYER)
│   ├── Interfaces/
│   │   ├── INotificationService.cs   (CẬP NHẬT)
│   │   └── INotificationHub.cs       ✅ TẠO MỚI
│   └── Services/
│       └── NotificationService.cs    (CẬP NHẬT)
│
└── DAL/                              (DATA ACCESS LAYER)
    ├── Data/Models/
    │   └── Notification.cs           (Có sẵn)
    └── Repository/
        └── NotificationRepository.cs (Có sẵn)
```

### 📊 **Dependencies Flow**

```
✅ Controller → Infrastructure → BLL → DAL
✅ KHÔNG có circular dependency!
✅ Follow Clean Architecture principles!

Chi tiết:
NotificationService (BLL)
  → INotificationHub (BLL Interface)
    → NotificationHubService (Infrastructure Implementation)
      → IHubContext<NotificationHub> (SignalR)
        → NotificationHub (Controller)
```

---

## 3. CÀI ĐẶT PACKAGE

### 📦 Bước 3.1: Cài đặt cho Controller project

```bash
cd Controller
dotnet add package Microsoft.AspNetCore.SignalR
```

Hoặc thêm vào `Controller/Controller.csproj`:
```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.0" />
```

**Lý do:** Controller chứa `NotificationHub` (kế thừa từ `Hub` class)

---

### 📦 Bước 3.2: Cài đặt và cấu hình Infrastructure project

**Cài đặt package:**
```bash
cd Infrastructure
dotnet add package Microsoft.AspNetCore.SignalR
```

**Thêm project reference:**
```bash
cd Infrastructure
dotnet add reference ../Controller/Controller.csproj
```

**Cập nhật `Infrastructure/Infrastructure.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CloudinaryDotNet" Version="1.27.8" />
    <PackageReference Include="DotNetEnv" Version="3.1.1" />
    <PackageReference Include="Google.Apis.Auth" Version="1.71.0" />
    <PackageReference Include="Google.Apis.Gmail.v1" Version="1.70.0.3833" />
    <PackageReference Include="payOS" Version="1.0.9" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.0" /> <!-- ✅ THÊM -->
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\BLL\BLL.csproj" />
    <ProjectReference Include="..\Controller\Controller.csproj" /> <!-- ✅ THÊM -->
  </ItemGroup>
  
  <!-- ... existing embedded resources ... -->
</Project>
```

**Lý do:** 
- Infrastructure chứa `NotificationHubService` sử dụng `IHubContext<NotificationHub>`
- `IHubContext` từ SignalR package
- `NotificationHub` từ Controller project

---

### ✅ Verify Installation

Sau khi cài đặt, kiểm tra:

**Controller/Controller.csproj:**
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.0" />
</ItemGroup>
```

**Infrastructure/Infrastructure.csproj:**
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.0" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\BLL\BLL.csproj" />
  <ProjectReference Include="..\Controller\Controller.csproj" />
</ItemGroup>
```

---

## 4. TẠO CẤU TRÚC SIGNALR

### 📄 Bước 4.1: Tạo `Controller/Hubs/BaseHub.cs`

Tương tự như BaseController, chúng ta tạo BaseHub để tái sử dụng logic:

```csharp
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Controller.Hubs;

/// <summary>
/// Base class cho tất cả SignalR Hubs
/// Cung cấp các helper methods giống BaseController
/// </summary>
public abstract class BaseHub : Hub
{
    /// <summary>
    /// Lấy UserId từ JWT token claims
    /// (Sử dụng logic giống BaseController.GetCurrentUserId())
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

### 📄 Bước 4.2: Tạo `Controller/Hubs/NotificationHub.cs`

```csharp
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Hubs;

/// <summary>
/// SignalR Hub để xử lý thông báo real-time
/// Đây là Presentation Layer component (giống Controller)
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
            
            // Có thể gọi NotificationService ở đây nếu cần update DB
            // Hiện tại chỉ notify client
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

---

### 📄 Bước 4.3: Tạo `BLL/Interfaces/INotificationHub.cs`

```csharp
namespace BLL.Interfaces;

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

---

### 📄 Bước 4.4: Tạo `Infrastructure/SignalR/NotificationHubService.cs`

```csharp
using BLL.Interfaces;
using Controller.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR;

/// <summary>
/// Service để gửi thông báo real-time qua SignalR
/// Đây là Infrastructure component - External Service
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
using BLL.Interfaces; // ✅ THÊM
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
- ✅ Consistent với pattern hiện có
- ✅ Dễ dàng enable/disable SignalR
- ✅ Tất cả infrastructure services đăng ký ở 1 chỗ

---

## 6. CẤU HÌNH PROGRAM.CS

Mở file `Controller/Program.cs` và thực hiện các cập nhật sau:

### 🔧 Bước 6.1: Thêm using statement

Thêm vào đầu file (sau dòng 21):

```csharp
using Controller.Hubs;
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

### 🔧 Bước 6.3: CORS đã đủ

CORS hiện tại của bạn đã đủ cho SignalR (có `AllowCredentials()`). Không cần thay đổi.

### 🔧 Bước 6.4: Map SignalR Hub endpoint

Thêm TRƯỚC `app.Run();` (sau dòng 330, sau `app.MapControllers();`):

```csharp
// Map SignalR Hub endpoint
app.MapHub<NotificationHub>("/hubs/notification");
```

**📍 Tổng hợp các vị trí trong Program.cs:**

```csharp
// Line ~21 - THÊM
using Controller.Hubs;

// ... existing code ...

// Line ~164 - Infrastructure services (đã có sẵn, bao gồm SignalR)
builder.Services.AddInfrastructure();

// THÊM NGAY SAU
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// ... existing code ...

// Line ~330 - THÊM (trước app.Run())
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();
```

---

## 7. CẬP NHẬT NOTIFICATIONSERVICE

### 📄 Bước 7.1: Cập nhật Interface

Mở file `BLL/Interfaces/INotificationService.cs` và thêm method signature mới:

```csharp
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
```

### 📄 Bước 7.2: Cập nhật Implementation

Mở file `BLL/Services/NotificationService.cs` và cập nhật:

```csharp
using AutoMapper;
using BLL.DTO;
using BLL.DTO.Notification;
using BLL.Interfaces;
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
    
    // CÁC METHOD CŨ GIỮ NGUYÊN
    
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
- Inject `INotificationHub` vào constructor (interface từ BLL)
- Implementation từ Infrastructure được inject tự động qua DI
- Method `CreateAndSendNotificationAsync` sẽ:
  1. Lưu notification vào database
  2. Gửi real-time qua SignalR
- Wrap SignalR trong try-catch để không ảnh hưởng nếu lỗi

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
    private readonly INotificationService _notificationService; // THÊM
    
    public RequestService(
        IRequestRepository requestRepository,
        INotificationService notificationService) // THÊM
    {
        _requestRepository = requestRepository;
        _notificationService = notificationService; // THÊM
    }
    
    // Khi tạo request mới
    public async Task<RequestResponseDTO> CreateRequestAsync(
        ulong userId, 
        RequestCreateDTO dto, 
        CancellationToken cancellationToken = default)
    {
        // ... logic tạo request ...
        var request = await _requestRepository.CreateRequestAsync(newRequest, cancellationToken);
        
        // Gửi thông báo cho user
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
        var request = await _requestRepository.UpdateRequestAsync(request, cancellationToken);
        
        // Tạo message phù hợp theo status
        var statusMessages = new Dictionary<RequestStatus, string>
        {
            { RequestStatus.InReview, "đang được xem xét" },
            { RequestStatus.Approved, "đã được chấp nhận" },
            { RequestStatus.Rejected, "đã bị từ chối" },
            { RequestStatus.Completed, "đã hoàn thành" },
            { RequestStatus.Cancelled, "đã bị hủy" }
        };
        
        // Gửi thông báo cho user
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
// Inject INotificationService vào constructor
private readonly INotificationService _notificationService;

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
    
    // Gửi thông báo cho vendor
    await _notificationService.CreateAndSendNotificationAsync(
        userId: order.VendorId, // Giả sử có VendorId
        title: "Đơn hàng mới",
        message: $"Bạn có đơn hàng mới #{order.Id} từ khách hàng {customerName}",
        referenceType: NotificationReferenceType.Order,
        referenceId: order.Id
    );
    
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
| **ProductReviewService** | Review mới | Vendor | ProductReviews |

---

## 9. CLIENT-SIDE IMPLEMENTATION

### 📦 Bước 9.1: Cài đặt package

```bash
npm install @microsoft/signalr
```

### 📄 Bước 9.2: Tạo NotificationService (TypeScript/JavaScript)

```typescript
// services/notificationService.ts
import * as signalR from "@microsoft/signalr";

export interface Notification {
    id: number;
    userId: number;
    title: string;
    message: string;
    referenceType: string | null;
    referenceId: number | null;
    isRead: boolean;
    createdAt: string;
}

class NotificationService {
    private connection: signalR.HubConnection | null = null;
    private token: string;
    private listeners: ((notification: Notification) => void)[] = [];

    constructor(token: string) {
        this.token = token;
    }

    async start(): Promise<void> {
        if (this.connection) {
            console.log("[SignalR] Already connected");
            return;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7000/hubs/notification", {
                accessTokenFactory: () => this.token,
                transport: signalR.HttpTransportType.WebSockets,
                skipNegotiation: false
            })
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: (context) => {
                    if (context.previousRetryCount === 0) return 0;
                    if (context.previousRetryCount === 1) return 2000;
                    if (context.previousRetryCount === 2) return 10000;
                    return 30000;
                }
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.setupEventHandlers();

        try {
            await this.connection.start();
            console.log("[SignalR] Connected successfully");
            
            const pingResult = await this.connection.invoke("Ping");
            console.log("[SignalR] Ping result:", pingResult);
        } catch (err) {
            console.error("[SignalR] Connection failed:", err);
            throw err;
        }
    }

    async stop(): Promise<void> {
        if (!this.connection) return;
        try {
            await this.connection.stop();
            console.log("[SignalR] Disconnected");
        } catch (err) {
            console.error("[SignalR] Disconnect error:", err);
        } finally {
            this.connection = null;
        }
    }

    private setupEventHandlers(): void {
        if (!this.connection) return;

        this.connection.on("ReceiveNotification", (notification: Notification) => {
            console.log("[SignalR] Received notification:", notification);
            this.listeners.forEach(listener => listener(notification));
        });

        this.connection.onreconnecting((error) => {
            console.warn("[SignalR] Reconnecting...", error);
        });

        this.connection.onreconnected((connectionId) => {
            console.log("[SignalR] Reconnected:", connectionId);
        });

        this.connection.onclose((error) => {
            console.error("[SignalR] Connection closed:", error);
        });
    }

    onNotification(callback: (notification: Notification) => void): () => void {
        this.listeners.push(callback);
        return () => {
            this.listeners = this.listeners.filter(l => l !== callback);
        };
    }

    async markAsRead(notificationId: number): Promise<void> {
        if (!this.connection) {
            throw new Error("Not connected to SignalR");
        }
        try {
            await this.connection.invoke("MarkNotificationAsRead", notificationId);
        } catch (err) {
            console.error("[SignalR] Error marking as read:", err);
            throw err;
        }
    }

    get isConnected(): boolean {
        return this.connection?.state === signalR.HubConnectionState.Connected;
    }
}

export default NotificationService;
```

### 📄 Bước 9.3: Sử dụng trong React (Hook)

```typescript
// hooks/useNotification.ts
import { useEffect, useState, useCallback } from 'react';
import NotificationService, { Notification } from '../services/notificationService';
import { toast } from 'react-toastify';

export const useNotification = (token: string | null) => {
    const [notifications, setNotifications] = useState<Notification[]>([]);
    const [unreadCount, setUnreadCount] = useState(0);
    const [service, setService] = useState<NotificationService | null>(null);
    const [isConnected, setIsConnected] = useState(false);

    useEffect(() => {
        if (!token) {
            setService(null);
            return;
        }

        const notificationService = new NotificationService(token);
        setService(notificationService);

        notificationService.start()
            .then(() => setIsConnected(true))
            .catch(err => console.error("Failed to connect:", err));

        return () => {
            notificationService.stop();
            setIsConnected(false);
        };
    }, [token]);

    useEffect(() => {
        if (!service) return;

        const unsubscribe = service.onNotification((notification) => {
            setNotifications(prev => [notification, ...prev]);
            setUnreadCount(prev => prev + 1);
            
            toast.info(
                <div>
                    <strong>{notification.title}</strong>
                    <p>{notification.message}</p>
                </div>,
                { position: "top-right", autoClose: 5000 }
            );
        });

        return unsubscribe;
    }, [service]);

    const markAsRead = useCallback(async (notificationId: number) => {
        if (!service) return;
        try {
            await service.markAsRead(notificationId);
            setNotifications(prev => 
                prev.map(n => 
                    n.id === notificationId ? { ...n, isRead: true } : n
                )
            );
            setUnreadCount(prev => Math.max(0, prev - 1));
        } catch (err) {
            console.error("Failed to mark as read:", err);
        }
    }, [service]);

    return { notifications, unreadCount, isConnected, markAsRead, service };
};
```

---

## 10. TESTING

### 🧪 Bước 10.1: Test Backend

#### Test 1: Kiểm tra Hub endpoint
```
GET https://localhost:7000/hubs/notification
```
Nếu trả về 401 Unauthorized → Đúng! (Vì cần JWT token)

#### Test 2: Test từ Service
Tạo test endpoint tạm:
```csharp
[HttpPost("test-send")]
[Authorize]
public async Task<ActionResult<APIResponse>> TestSendNotification([FromQuery] ulong targetUserId)
{
    await _notificationService.CreateAndSendNotificationAsync(
        userId: targetUserId,
        title: "Test Notification",
        message: "This is a test notification"
    );
    return SuccessResponse("Notification sent");
}
```

### 🧪 Bước 10.2: Test Frontend

Mở Developer Console:
```
[SignalR] Connected successfully
[SignalR] Ping result: Pong from User 1 (Role: Customer)
```

---

## 11. TROUBLESHOOTING

### ❌ Lỗi 1: "Connection refused" hoặc 404

**Giải pháp:**
```csharp
app.MapHub<NotificationHub>("/hubs/notification");
```

### ❌ Lỗi 2: "401 Unauthorized"

**Giải pháp:** Kiểm tra JWT configuration hỗ trợ query string:
```csharp
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) && 
            path.StartsWithSegments("/hubs/notification"))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }
};
```

---

## ✅ CHECKLIST HOÀN THÀNH

### Backend - Presentation Layer
- [ ] Cài package SignalR cho Controller
- [ ] Tạo folder `Controller/Hubs/`
- [ ] Tạo `Controller/Hubs/BaseHub.cs`
- [ ] Tạo `Controller/Hubs/NotificationHub.cs`
- [ ] Kiểm tra `[Authorize]` trên NotificationHub

### Backend - Infrastructure Layer
- [ ] Cài package SignalR cho Infrastructure
- [ ] Add reference Infrastructure → Controller
- [ ] Tạo folder `Infrastructure/SignalR/`
- [ ] Tạo `Infrastructure/SignalR/NotificationHubService.cs`
- [ ] Cập nhật `Infrastructure/Extensions/ServiceCollectionExtensions.cs`

### Backend - Business Logic Layer
- [ ] Tạo `BLL/Interfaces/INotificationHub.cs`
- [ ] Cập nhật `BLL/Interfaces/INotificationService.cs`
- [ ] Cập nhật `BLL/Services/NotificationService.cs`

### Backend - Configuration
- [ ] Cập nhật `Controller/Program.cs` - using statement
- [ ] Cập nhật `Controller/Program.cs` - AddSignalR()
- [ ] Cập nhật `Controller/Program.cs` - MapHub()

### Service Integration
- [ ] RequestService - CreateRequest
- [ ] RequestService - ProcessRequest
- [ ] OrderService - CreateOrder
- [ ] OrderService - UpdateStatus
- [ ] PaymentService - Success/Failed

### Frontend
- [ ] Cài đặt `@microsoft/signalr`
- [ ] Tạo `NotificationService.ts`
- [ ] Tạo `useNotification` hook
- [ ] Tích hợp vào App

---

## 🎯 TÓM TẮT KIẾN TRÚC

```
✅ Controller → Infrastructure → BLL → DAL
✅ KHÔNG có circular dependency!
✅ SignalR = External Service → Infrastructure Layer
✅ Consistent với Email, Cloudinary, PayOS
✅ Follow Clean Architecture 100%!
```

**Chúc bạn tích hợp thành công! 🚀**

*Document version: 3.0*  
*Last updated: 2025-11-14*  
*Changes: Di chuyển NotificationHubService vào Infrastructure layer*
