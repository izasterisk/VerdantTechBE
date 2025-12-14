# VerdantTech Solutions - Deployment Information
**Tài liệu cấu hình và triển khai hệ thống**

---

## 1. MODULE PHẦN MÀM

### 1.1. Tổng quan kiến trúc
Hệ thống được phát triển theo kiến trúc 3-layer với .NET 8:

```
VerdantTechSolution/
├── Controller/          # API Layer - ASP.NET Core Web API
├── BLL/                # Business Logic Layer
├── DAL/                # Data Access Layer
├── Infrastructure/     # External Services Integration
└── DB/                 # Database Scripts
```

### 1.2. Danh sách các module chức năng

#### **A. Authentication & User Management**
- **Controller**: `AuthController.cs`, `UserController.cs`
- **Service**: `AuthService.cs`, `UserService.cs`
- **Repository**: `AuthRepository.cs`, `UserRepository.cs`
- **Chức năng**:
  - Đăng nhập/đăng ký (Email/Password, Google OAuth)
  - Quản lý tài khoản người dùng (Customer, Staff, Vendor, Admin)
  - Xác thực email, đổi mật khẩu, quên mật khẩu
  - JWT-based authentication với refresh token

#### **B. Product Management**
- **Controller**: `ProductController.cs`, `ProductCategoryController.cs`, `ProductRegistrationController.cs`, `ProductCertificateController.cs`, `ProductSerialController.cs`, `ProductReviewController.cs`, `ProductUpdateRequestController.cs`
- **Service**: `ProductService.cs`, `ProductCategoryService.cs`, `ProductRegistrationService.cs`, `ProductCertificateService.cs`, `ProductReviewService.cs`, `ProductUpdateRequestService.cs`
- **Repository**: `ProductRepository.cs`, `ProductCategoryRepository.cs`, `ProductRegistrationRepository.cs`, `ProductCertificateRepository.cs`, `ProductSerialRepository.cs`, `ProductReviewRepository.cs`
- **Chức năng**:
  - Quản lý sản phẩm nông nghiệp xanh
  - Đăng ký sản phẩm mới (workflow: Pending → Approved/Rejected)
  - Chứng nhận sản phẩm (product certificates)
  - Quản lý serial numbers cho thiết bị
  - Đánh giá & review sản phẩm
  - Yêu cầu cập nhật sản phẩm (ProductSnapshot cho proposed changes)

#### **C. Vendor & Farm Management**
- **Controller**: `VendorProfilesController.cs`, `VendorCertificatesController.cs`, `FarmProfileController.cs`, `CropController.cs`
- **Service**: `VendorProfileService.cs`, `VendorCertificateService.cs`, `FarmProfileService.cs`, `CropService.cs`
- **Repository**: `VendorProfileRepository.cs`, `VendorCertificateRepository.cs`, `FarmProfileRepository.cs`, `CropRepository.cs`
- **Chức năng**:
  - Quản lý hồ sơ nhà cung cấp/vendor
  - Chứng nhận vendor (VGaP, GlobalGaP, VietGaP, Organic, FDA, CE)
  - Quản lý trang trại (farm profiles)
  - Quản lý cây trồng (crops: loại cây, phương pháp gieo trồng, loại hình canh tác)

#### **D. Cart & Order Management**
- **Controller**: `CartController.cs`, `OrderController.cs`
- **Service**: `CartService.cs`, `OrderService.cs`
- **Repository**: `CartRepository.cs`, `OrderRepository.cs`, `OrderDetailRepository.cs`
- **Chức năng**:
  - Giỏ hàng (thêm/sửa/xóa sản phẩm)
  - Đặt hàng (Order workflow: Pending → Processing → Shipping → Delivered/Cancelled)
  - Quản lý chi tiết đơn hàng
  - Tích hợp vận chuyển (GoShip, GHN)

#### **E. Payment & Wallet Management**
- **Controller**: `PayOSController.cs`, `WalletController.cs`, `CashoutController.cs`
- **Service**: `PayOSService.cs` (BLL/Services/Payment/), `WalletService.cs`, `CashoutService.cs`
- **Repository**: `PaymentRepository.cs`, `TransactionRepository.cs`, `WalletRepository.cs`, `CashoutRepository.cs`
- **Chức năng**:
  - Thanh toán online qua PayOS
  - Ví điện tử người dùng (Wallet)
  - Rút tiền (Cashout/Payout)
  - Lịch sử giao dịch (Transactions)

#### **F. Inventory Management**
- **Controller**: `BatchInventoryController.cs`, `ExportInventoryController.cs`
- **Service**: `BatchInventoryService.cs`, `ExportInventoryService.cs`
- **Repository**: `BatchInventoryRepository.cs`, `ExportInventoryRepository.cs`
- **Chức năng**:
  - Quản lý lô hàng nhập kho (Batch Inventory)
  - Quản lý xuất kho (Export Inventory)
  - Tracking serial numbers

#### **G. Address Management**
- **Controller**: `AddressController.cs`
- **Service**: `AddressService.cs`
- **Repository**: `AddressRepository.cs`
- **Chức năng**:
  - Quản lý địa chỉ người dùng (UserAddress)
  - Tích hợp API địa chỉ Việt Nam (province/district/ward)

#### **H. User Bank Accounts**
- **Controller**: `UserBankAccountsController.cs`
- **Service**: `UserBankAccountsService.cs`
- **Repository**: `UserBankAccountsRepository.cs`
- **Chức năng**:
  - Quản lý tài khoản ngân hàng của người dùng
  - Hỗ trợ rút tiền từ ví

#### **I. Environmental & Weather Data**
- **Controller**: `WeatherController.cs`, `CO2Controller.cs`
- **Service**: `WeatherService.cs`, `CO2Service.cs`, `EnvCacheService.cs`
- **Repository**: `EnvironmentalDataRepository.cs`, `FertilizerRepository.cs`, `EnergyUsageRepository.cs`
- **Chức năng**:
  - Lấy dữ liệu thời tiết (Open-Meteo API)
  - Theo dõi dữ liệu môi trường (environmental_data)
  - Tính toán CO2 footprint
  - Cache dữ liệu môi trường

#### **J. Forum Management**
- **Controller**: `ForumCategoryController.cs`, `ForumPostController.cs`, `ForumCommentController.cs`
- **Service**: `ForumCategoryService.cs`, `ForumPostService.cs`, `ForumCommentService.cs`
- **Repository**: `ForumCategoryRepository.cs`, `ForumPostRepository.cs`, `ForumCommentRepository.cs`
- **Chức năng**:
  - Diễn đàn thảo luận
  - Quản lý danh mục forum
  - Đăng bài, bình luận
  - Content blocks (JSON structure cho rich content)

#### **K. Support Ticket Management**
- **Controller**: `RequestTicketController.cs`
- **Service**: `RequestService.cs`
- **Repository**: `RequestRepository.cs`
- **Chức năng**:
  - Hệ thống ticket hỗ trợ khách hàng
  - Chat real-time với staff
  - Request messages

#### **L. AI Chatbot Management**
- **Controller**: `ChatbotConversationController.cs`
- **Service**: `ChatbotConversationService.cs`
- **Repository**: `ChatbotConversationRepository.cs`
- **Chức năng**:
  - Quản lý cuộc hội thoại với AI chatbot
  - Lịch sử tin nhắn chatbot

#### **M. Notification System**
- **Controller**: `NotificationController.cs`
- **Service**: `NotificationService.cs`
- **Repository**: `NotificationRepository.cs`
- **Chức năng**:
  - Thông báo real-time qua SignalR Hub
  - Quản lý thông báo người dùng
  - SignalR endpoint: `/hubs/notification`

#### **N. Dashboard & Analytics**
- **Controller**: `DashboardController.cs`
- **Service**: `DashboardService.cs`
- **Repository**: `DashboardRepository.cs`
- **Chức năng**:
  - Thống kê, báo cáo tổng quan
  - Dashboard cho Admin/Vendor

#### **O. Survey & Feedback**
- **Controller**: `SurveyResponseController.cs`
- **Service**: `SurveyResponseService.cs`
- **Repository**: `SurveyResponseRepository.cs`
- **Chức năng**:
  - Khảo sát người dùng
  - Thu thập phản hồi

---

## 2. THƯ VIỆN, FRAMEWORK VÀ CÔNG CỤ BÊN THỨ 3

### 2.1. Core Framework & Libraries

#### **ASP.NET Core & Entity Framework**
- **Microsoft.NET.Sdk.Web** - .NET 8.0 Web SDK
- **Microsoft.EntityFrameworkCore** (v8.0.19) - ORM Framework
- **Pomelo.EntityFrameworkCore.MySql** (v8.0.3) - MySQL Provider cho EF Core
- **Microsoft.EntityFrameworkCore.Tools** (v8.0.19) - Migration tools
- **Microsoft.EntityFrameworkCore.Design** (v8.0.19) - Design-time tools

#### **Authentication & Security**
- **Microsoft.AspNetCore.Authentication.JwtBearer** (v8.0.8) - JWT Authentication
- **Microsoft.AspNetCore.Authentication.Google** (v8.0.19) - Google OAuth
- **System.IdentityModel.Tokens.Jwt** (v8.0.2) - JWT Token generation
- **Google.Apis.Auth** (v1.71.0) - Google Authentication library
- **BCrypt.Net-Next** (v4.0.3) - Password hashing

#### **API Documentation**
- **Swashbuckle.AspNetCore** (v6.6.2) - Swagger/OpenAPI
- **Microsoft.AspNetCore.OpenApi** (v8.0.4) - OpenAPI support

#### **Object Mapping & JSON**
- **AutoMapper** (v14.0.0) - Object-to-object mapping
- **Newtonsoft.Json** (v13.0.3) - JSON serialization

#### **Configuration & Environment**
- **DotNetEnv** (v3.1.1) - .env file support

#### **Caching**
- **Microsoft.Extensions.Caching.Memory** (v8.0.1) - In-memory cache

### 2.2. Third-party Services Integration

#### **Cloud Storage**
- **CloudinaryDotNet** (v1.27.8) - Cloudinary image/file upload service

#### **Payment Gateway**
- **payOS** (v1.0.9) - PayOS payment integration

#### **Email Service**
- **Google.Apis.Gmail.v1** (v1.70.0.3833) - Gmail API client
- Embedded email templates (HTML/Text) trong Infrastructure project

#### **Container & Deployment**
- **Microsoft.VisualStudio.Azure.Containers.Tools.Targets** (v1.21.0) - Docker support
- **Dockerfile** - Multi-stage build configuration

---

## 3. CẤU HÌNH CÁC THÀNH PHẦN BÊN TRONG PHẦN MỀM

### 3.1. Connection String & Database

**File**: `Controller/.env`

```env
# Database Configuration
DATABASE_CONNECTION_STRING=Server=verdanttech.mysql.database.azure.com;Database=verdanttech;Uid=izasterisk;Pwd=xxx;SslMode=Required;

# Database timeout (seconds)
DATABASE_TIMEOUT=30

# Auto create/recreate database (Development only)
AUTO_CREATE_DB=False
```

**Database Server**: Azure MySQL Database
- **Host**: verdanttech.mysql.database.azure.com
- **Database**: verdanttech
- **User**: izasterisk
- **SSL**: Required
- **Version**: MySQL 8.0.43

**Program.cs Configuration**:
```csharp
builder.Services.AddDbContext<VerdantTechDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.43-mysql"),
        b => b.MigrationsAssembly("DAL")
            .CommandTimeout(databaseTimeout)
            .EnableStringComparisonTranslations()));
```

### 3.2. JWT Authentication Configuration

**File**: `Controller/.env`

```env
# JWT Configuration
JWT_SECRET=xxx
JWT_ISSUER=VerdantTech
JWT_AUDIENCE=VerdantTechUsers
JWT_EXPIRE_HOURS=24
REFRESH_TOKEN_EXPIRE_DAYS=7
```

**Program.cs Configuration**:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
    });
```

### 3.3. API Server Configuration

**File**: `Controller/.env`

```env
# Application Environment
ASPNETCORE_ENVIRONMENT=Development

# Swagger UI
OPEN_SWAGGER=True
```

**URLs**:
- **Backend API**: https://verdanttechbe-bpbaaghrg5ggexds.southeastasia-01.azurewebsites.net
- **Frontend**: https://verdanttechsolution.verdev.id.vn
- **API Port**: Configured by Azure App Service (default: 80/443)

**CORS Configuration** (Program.cs):
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### 3.4. SignalR Real-time Configuration

**Program.cs Configuration**:
```csharp
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Hub endpoint
app.MapHub<NotificationHub>("/hubs/notification");
```

**Endpoint**: `/hubs/notification`

### 3.5. Memory Cache Configuration

```csharp
builder.Services.AddMemoryCache();
```

Được sử dụng trong `EnvCacheService` để cache dữ liệu môi trường.

### 3.6. AutoMapper Configuration

```csharp
builder.Services.AddAutoMapper(typeof(AutoMapperConfig));
```

**File**: `BLL/Helpers/AutoMapperConfig.cs`

### 3.7. File Upload Configuration

**Program.cs**:
```csharp
builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});
```

---

## 4. CẤU HÌNH DỊCH VỤ BÊN THỨ 3 (3RD PARTY API)

### 4.1. Cloudinary (Image/File Storage)

**File**: `Controller/.env`

```env
# Cloudinary Configuration
CLOUDINARY__CLOUDNAME=dy6htecfm
CLOUDINARY__APIKEY=xxx
CLOUDINARY__APISECRET=xxx
CLOUDINARY__DEFAULTFOLDER=VerdantTech_Image
```

**Service**: `Infrastructure/Cloudinary/CloudinaryService.cs`

**Program.cs**:
```csharp
builder.Services.Configure<CloudinaryOptions>(o =>
{
    o.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY__CLOUDNAME");
    o.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY__APIKEY");
    o.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY__APISECRET");
    o.DefaultFolder = Environment.GetEnvironmentVariable("CLOUDINARY__DEFAULTFOLDER");
});

builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
```

### 4.2. PayOS (Payment Gateway)

**File**: `Controller/.env`

```env
# PayOS Configuration
PAYOS_CLIENT_ID=xxx
PAYOS_API_KEY=xxx
PAYOS_CHECKSUM_KEY=xxx

# PayOS Payout (Cashout)
PAYOS_CLIENT_ID_PAYOUT=xxx
PAYOS_API_KEY_PAYOUT=xxx
PAYOS_CHECKSUM_KEY_PAYOUT=xxx
```

**Service**: `BLL/Services/Payment/PayOSService.cs`

**Callback URLs**:
- **Frontend**: https://verdanttechsolution.verdev.id.vn
- **Backend**: https://verdanttechbe-bpbaaghrg5ggexds.southeastasia-01.azurewebsites.net

### 4.3. Google OAuth & Gmail API

**File**: `Controller/.env`

```env
# Google OAuth
GOOGLE_CLIENT_ID=xxx
GOOGLE_CLIENT_SECRET=xxx

# Gmail API (for sending emails)
GMAIL_USER=hoannse170538@fpt.edu.vn
GMAIL_REFRESH_TOKEN=xxx
```

**Service**: `Infrastructure/Email/GmailService.cs`

**Email Templates**: Embedded resources trong `Infrastructure/Email/Templates/`
- verification.html/txt
- forgot-password.html/txt
- staff-account-created.html/txt
- vendor-profile-verified/rejected/submitted.html/txt
- product-approved/rejected/registration-created.html/txt
- vendor-soft-delete.html/txt

### 4.4. Open-Meteo (Weather Data API)

**File**: `Controller/.env`

```env
# Open-Meteo Weather API
OPEN_METEO_URL=https://api.open-meteo.com/v1/
OPEN_METEO_ARCHIVE_URL=https://archive-api.open-meteo.com/v1/

# Timezone
DEFAULT_TIME_ZONE=Asia/Ho_Chi_Minh
TIME_OUT_SECONDS=10
```

**Service**: `Infrastructure/Weather/OpenMeteoService.cs`

### 4.5. SoilGrids API (Soil Data)

**File**: `Controller/.env`

```env
# SoilGrids API
SOIL_GRIDS_URL=https://rest.isric.org/soilgrids/v2.0/properties/
SOIL_GRIDS_WMS=https://maps.isric.org/mapserv
```

**Service**: `Infrastructure/Soil/SoilGridsService.cs`

### 4.6. GoShip (Courier/Shipping Service)

**File**: `Controller/.env`

```env
# GoShip API v2 (Sandbox)
GOSHIP_TOKEN=xxx
GOSHIP_CLIENT_ID=206
GOSHIP_CLIENT_SECRET=xxx
GOSHIP_SANDBOX_MANAGE_URL=https://dev-shop.goship.io
GOSHIP_SANDBOX_MANAGE_API_ENDPOINT=https://sandbox.goship.io/api/v2

# GoShip API v1 (Production)
GOSHIP_MANAGE_API_ENDPOINT_V1=https://api.goship.io/api/v1
GOSHIP_TOKEN_V1=xxx
```

**Service**: `Infrastructure/Courier/GoShipService.cs`

### 4.7. GHN (Giao Hàng Nhanh - Shipping)

**File**: `Controller/.env`

```env
# GHN API
GHN_SHOP_ID=197547
GHN_PRODUCTION_TOKEN=xxx
GHN_PRODUCTION_URL=https://online-gateway.ghn.vn/shiip/public-api/master-data
GHN_TESTING_TOKEN=xxx
GHN_TESTING_URL=https://dev-online-gateway.ghn.vn/shiip/public-api/v2
```

**Service**: `Infrastructure/Courier/GHNService.cs`

---

## 5. DANH SÁCH ROLES VÀ TÀI KHOẢN DEMO

### 5.1. User Roles

**Enum**: `DAL/Data/Enums.cs`

```csharp
public enum UserRole
{
    Customer,    // Khách hàng
    Staff,       // Nhân viên hỗ trợ
    Vendor,      // Nhà cung cấp/Người bán
    Admin        // Quản trị viên
}
```

### 5.2. Tài khoản Demo (Test Accounts)

**Lưu ý**: Danh sách tài khoản demo cần được cung cấp từ database seeder hoặc tài liệu riêng. Dưới đây là template:

#### **Admin Account**
- **Email**: admin@verdanttech.com
- **Password**: Admin@123
- **Role**: Admin
- **Chức năng**: Quản trị toàn hệ thống, duyệt vendor/product registration, quản lý user

#### **Staff Account**
- **Email**: staff@verdanttech.com
- **Password**: Staff@123
- **Role**: Staff
- **Chức năng**: Hỗ trợ khách hàng, xử lý support tickets, xem báo cáo

#### **Vendor Account**
- **Email**: vendor@verdanttech.com
- **Password**: Vendor@123
- **Role**: Vendor
- **Chức năng**: Đăng ký/quản lý sản phẩm, xem đơn hàng, quản lý inventory, farm profiles

#### **Customer Account**
- **Email**: customer@verdanttech.com
- **Password**: Customer@123
- **Role**: Customer
- **Chức năng**: Mua sản phẩm, đánh giá, sử dụng chatbot, forum, xem thông tin môi trường

### 5.3. User Status

```csharp
public enum UserStatus
{
    Active,      // Đang hoạt động
    Inactive,    // Chưa kích hoạt email
    Suspended,   // Bị tạm khóa
    Deleted      // Đã xóa (soft delete)
}
```

---

## 6. HƯỚNG DẪN CÀI ĐẶT & TRIỂN KHAI

### 6.1. Cài đặt môi trường Development

**Prerequisites**:
- .NET 8.0 SDK
- MySQL 8.x Server
- IDE: Visual Studio 2022 / JetBrains Rider / VS Code

**Bước 1**: Clone repository
```bash
git clone <repository-url>
cd BE
```

**Bước 2**: Cấu hình file `.env`
```bash
cd Controller
# Copy .env.example to .env và điền thông tin
cp .env.example .env
```

**Bước 3**: Restore packages
```bash
dotnet restore
```

**Bước 4**: Apply database migrations
```bash
cd DAL
dotnet ef database update --project ../Controller
```

**Bước 5**: Run seeder (nếu có)
```bash
# Import DB/SEEDER.sql vào MySQL database
mysql -u root -p verdanttech < DB/SEEDER.sql
```

**Bước 6**: Run application
```bash
cd Controller
dotnet run
```

**Swagger URL**: https://localhost:5001/swagger

### 6.2. Build & Deploy

**Build Docker Image**:
```bash
docker build -t verdanttech-backend .
docker run -p 8080:80 --env-file Controller/.env verdanttech-backend
```

**Deploy to Azure App Service**:
- Configuration đã được thiết lập cho Azure App Service
- Environment variables được config trong Azure Portal App Settings
- Auto-scaling enabled

---

## 7. DEPENDENCY INJECTION SUMMARY

**File**: `Controller/Program.cs`

### Repositories
```csharp
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// + 30+ specific repository registrations
```

### Services
```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
// + 25+ service registrations
```

### Infrastructure
```csharp
builder.Services.AddInfrastructure(); // Extension method
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
// Email, Weather, Soil, Courier services
```

---

## 8. API RESPONSE FORMAT

Tất cả API endpoints đều trả về `APIResponse<T>`:

```json
{
  "status": 200,
  "message": "Success",
  "data": { ... }
}
```

**Error Response**:
```json
{
  "status": 400,
  "message": "Validation failed",
  "data": {
    "errors": ["Field 'email' is required"]
  }
}
```

---

## 9. DATABASE SCHEMA

**File**: `DB/verdanttech_schema_v10.sql`

**Tables**: 40+ bảng bao gồm:
- users, vendor_profiles, farm_profiles
- products, product_categories, product_registrations, product_certificates
- orders, order_details, carts, cart_items
- payments, transactions, wallets, cashouts
- batch_inventory, export_inventory, product_serials
- forum_categories, forum_posts, forum_comments
- chatbot_conversations, chatbot_messages
- requests, request_messages, notifications
- crops, environmental_data, fertilizers, energy_usage
- survey_responses, media_links, user_addresses

---

## 10. LIÊN HỆ & HỖ TRỢ

**Backend Team**:
- Email: support@verdanttech.com
- Documentation: README.md trong repository

**Production URLs**:
- Backend API: https://verdanttechbe-bpbaaghrg5ggexds.southeastasia-01.azurewebsites.net
- Frontend: https://verdanttechsolution.verdev.id.vn
- Swagger: https://verdanttechbe-bpbaaghrg5ggexds.southeastasia-01.azurewebsites.net/swagger

---

**Last Updated**: December 15, 2025
**Version**: 1.0
**Framework**: .NET 8.0
**Database**: MySQL 8.0.43
