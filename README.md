## VerdantTech Solutions - Backend (.NET 8)

Nền tảng cung cấp thiết bị nông nghiệp xanh tích hợp AI hỗ trợ canh tác rau củ bền vững.

Lưu ý: Nhiều chức năng trong tài liệu dự án vẫn đang phát triển dần (AI chatbot, nhận diện bệnh cây trồng, mobile app, vendor portal, expert portal, monitoring engine...). README này mô tả phần backend hiện có và cách chạy.

### 1) Kiến trúc và thư mục
- **Solution**: `VerdantTechSolution.sln`
- **Projects**:
  - `Controller/` (ASP.NET Core Web API host, Swagger, JWT, DI)
  - `BLL/` (Business Logic Layer: DTOs, Services, Interfaces, Helpers)
  - `DAL/` (Data Access Layer: DbContext, Models, Configurations, Repositories)
  - `Infrastructure/` (Email sender, đăng ký dịch vụ hạ tầng)
  - `DB/` (schema, seed SQL)

### 2) Yêu cầu môi trường
- .NET SDK 8.0+
- MySQL 8.x (hoặc compatible)
- Biến môi trường qua file `.env` (đặt tại `Controller/.env` hoặc ở root):

```
DATABASE_CONNECTION_STRING="Server=localhost;Port=3306;Database=verdanttech;User Id=root;Password=your_password;SslMode=None;"

# JWT
JWT_SECRET=your_long_random_secret
JWT_ISSUER=VerdantTech
JWT_AUDIENCE=VerdantTech.Client

# DEV tiện ích
AUTO_CREATE_DB=False           # True để tự drop/create DB khi chạy ở Development
OPEN_SWAGGER=True              # True để bật Swagger UI khi Development

# SMTP (gửi email xác thực/quên mật khẩu)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your_email@gmail.com
SMTP_PASS=your_app_password
SMTP_SENDER_NAME=VerdantTech
```

Ghi chú:
- Ứng dụng đọc `.env` bằng `DotNetEnv`. Nếu không có biến bắt buộc, chương trình sẽ throw lỗi.
- CORS mặc định cho `http://localhost:5173` và `https://localhost:5173`.

### 3) Cách chạy dự án (Development)
1. Tạo database MySQL trống trùng tên trong `DATABASE_CONNECTION_STRING`.
2. (Tuỳ chọn) Set `AUTO_CREATE_DB=True` để API tự `EnsureDeleted()` + `EnsureCreated()` khi chạy Development.
3. Tạo file `Controller/.env` theo mẫu ở trên.
4. Tại thư mục solution, chạy API:

```
dotnet build
dotnet run --project Controller/Controller.csproj
```

- Swagger: nếu `OPEN_SWAGGER=True` và môi trường Development, UI sẽ sẵn sàng tại `/swagger`.

### 4) Công nghệ chính đang dùng
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core + MySQL Provider
- JWT Authentication (Bearer)
- AutoMapper
- MailKit (SMTP)

### 5) Các nhóm API hiện có

Tất cả API đặt dưới base `/api/{ControllerName}`.

- **AuthController** (`/api/Auth`):
  - `POST /login` — đăng nhập, trả JWT + thông tin người dùng
  - `POST /google-login` — đăng nhập bằng Google ID token (stub/logic trong service)
  - `POST /send-verification` — gửi email xác thực (8 ký tự)
  - `POST /verify-email` — xác thực email bằng code
  - `POST /refresh-token` — nhận JWT mới qua refresh token
  - `GET /profile` — lấy thông tin từ JWT (yêu cầu Authorization)
  - `POST /forgot-password` — gửi email quên mật khẩu
  - `POST /reset-password` — đặt lại mật khẩu qua email + code
  - `POST /logout` — vô hiệu hoá refresh token (yêu cầu Authorization)

- **UserController** (`/api/User`):
  - `POST /` — tạo người dùng mới
  - `GET /{id}` — lấy người dùng theo id (yêu cầu Authorization)
  - `GET /?page=&pageSize=&role=` — phân trang + lọc theo role (Admin/Manager mới truy cập)
  - `PUT /{id}` — cập nhật người dùng (yêu cầu Authorization)
  - `PATCH /{id}/status?status=` — đổi trạng thái người dùng (yêu cầu Authorization)

- **SupportedBanksController** (`/api/SupportedBanks`):
  - `POST /` — tạo ngân hàng (Admin/Manager)
  - `GET /{id}` — xem theo id (anonymous)
  - `GET /by-code/{code}` — xem theo mã (anonymous)
  - `GET /?page=&pageSize=` — danh sách có phân trang (anonymous)
  - `PUT /{id}` — cập nhật (Admin/Manager)

- **SustainabilityCertificationsController** (`/api/SustainabilityCertifications`):
  - `POST /` — tạo certification (Admin/Manager)
  - `GET /{id}` — xem theo id (anonymous)
  - `GET /?page=&pageSize=&category=` — danh sách + lọc category (anonymous)
  - `PUT /{id}` — cập nhật (Admin/Manager)
  - `GET /categories` — danh sách categories (anonymous)

### 6) Mô tả tầng và DI
- `Controller/Program.cs` cấu hình:
  - DbContext: `VerdantTechDbContext` (MySQL 8.0.43)
  - Đăng ký Repository và Service cho User, VendorProfile, SustainabilityCertification, SupportedBank, Auth
  - JWT Bearer + custom 401/403 response
  - CORS, Swagger, AutoMapper, Infrastructure (Email)

- `Infrastructure/Email/EmailSender.cs` đọc biến `SMTP_*` và gửi email qua MailKit. Template email sử dụng embedded resource trong assembly.

### 7) Database
- Xem `DB/verdanttech_schema_v4.sql` để nắm cấu trúc bảng.
- `DB/SEEDER.sql` chứa dữ liệu mẫu (nếu có).

### 8) Phát triển tiếp theo (Roadmap rút gọn)
Theo `VerdantTech_Project_Info.txt`, các hạng mục lớn sẽ bổ sung dần:
- Khách hàng (Web/Mobile): profile nông trại, nhập pH, CO₂, forum, blog, cảnh báo thời tiết, chatbot AI, upload ảnh bệnh.
- Người bán (Web): hồ sơ nhà cung cấp, quản lý danh mục sản phẩm xanh, analytics, tài liệu kỹ thuật.
- Chuyên gia/Admin (Web Portal): phân quyền, giám sát hiệu năng, kiểm duyệt nội dung, đảm bảo chính xác AI.
- AI & Tự động hoá: tích hợp n8n/Botpress; nhận diện bệnh cây qua dịch vụ CV; khuyến nghị canh tác theo pH/CO₂/thời tiết.
- Environmental Monitoring Engine: cập nhật thời tiết 15 phút, cảm biến pH/độ ẩm (thiết kế API/device ingestion).

Các module trên chưa đầy đủ trong repo hiện tại, sẽ được thiết kế và triển khai theo lộ trình.

### 9) Góp ý & phát triển
- PR/Issue: mô tả rõ bối cảnh, bước tái hiện, kỳ vọng, log.
- Coding style: đặt tên rõ nghĩa, xử lý lỗi sớm, tránh lồng sâu; giữ định dạng hiện hữu.

### 10) License
Đang cập nhật.


