## VerdantTech Solutions - Backend (.NET 8)

Nền tảng cung cấp thiết bị nông nghiệp xanh tích hợp AI hỗ trợ canh tác rau củ bền vững.

Lưu ý: Nhiều chức năng trong tài liệu dự án vẫn đang phát triển dần (AI chatbot, nhận diện bệnh cây trồng, mobile app, vendor portal, expert portal, monitoring engine...). README này mô tả phần backend hiện có.

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

### 3) Công nghệ chính đang dùng
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core + MySQL Provider
- JWT Authentication (Bearer)
- AutoMapper
- MailKit (SMTP)

### 4) Các nhóm API và Validation chi tiết

Tất cả API đặt dưới base `/api/{ControllerName}`.

#### **AuthController** (`/api/Auth`):

**`POST /login` — Đăng nhập với email/password**
- **Validation từ DTO**:
  - Email: Bắt buộc, định dạng email hợp lệ
  - Password: Bắt buộc, tối thiểu 6 ký tự
- **Validation từ Service**:
  - Kiểm tra user tồn tại theo email
  - Kiểm tra trạng thái user (phải Active)
  - Xác minh mật khẩu với hash trong database
  - Cập nhật LastLoginAt

**`POST /google-login` — Đăng nhập bằng Google ID token**
- **Validation từ DTO**:
  - IdToken: Bắt buộc, chuỗi không rỗng
- **Validation từ Service**:
  - Xác thực Google ID token qua Google API
  - Tạo user mới nếu chưa tồn tại hoặc kiểm tra trạng thái user existing
  - Cập nhật LastLoginAt

**`POST /send-verification` — Gửi email xác thực**
- **Validation từ DTO**:
  - Email: Bắt buộc, định dạng email hợp lệ
- **Validation từ Service**:
  - Kiểm tra user tồn tại theo email
  - Kiểm tra trạng thái user (phải Active)
  - Kiểm tra user chưa được verify (IsVerified = false)
  - Tạo mã xác thực 8 số ngẫu nhiên

**`POST /verify-email` — Xác thực email bằng mã code**
- **Validation từ DTO**:
  - Email: Bắt buộc, định dạng email hợp lệ
  - Code: Bắt buộc, 8 ký tự số
- **Validation từ Service**:
  - Kiểm tra user tồn tại theo email
  - Kiểm tra mã xác thực khớp với database
  - Kiểm tra mã chưa hết hạn (thời gian gửi)
  - Kiểm tra trạng thái user (phải Active)

**`POST /refresh-token` — Làm mới JWT token**
- **Validation từ DTO**:
  - RefreshToken: Bắt buộc, chuỗi không rỗng
- **Validation từ Service**:
  - Kiểm tra refresh token tồn tại trong database
  - Kiểm tra refresh token chưa hết hạn
  - Kiểm tra trạng thái user (phải Active)

**`GET /profile` — Lấy thông tin profile [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Xác thực JWT token hợp lệ
  - Lấy userId từ token claims
  - Kiểm tra user tồn tại theo userId

**`POST /forgot-password` — Gửi email quên mật khẩu**
- **Validation từ DTO**:
  - Email: Bắt buộc, định dạng email hợp lệ
- **Validation từ Service**:
  - Kiểm tra user tồn tại theo email
  - Kiểm tra trạng thái user (phải Active)
  - Tạo mã xác thực 8 số ngẫu nhiên

**`POST /reset-password` — Đặt lại mật khẩu**
- **Validation từ DTO**:
  - Email: Bắt buộc, định dạng email hợp lệ
  - NewPassword: Bắt buộc, chuỗi không rỗng
  - Code: Bắt buộc, 8 ký tự số
- **Validation từ Service**:
  - Kiểm tra user tồn tại theo email
  - Kiểm tra mã reset password khớp
  - Kiểm tra mã chưa hết hạn
  - Kiểm tra trạng thái user (phải Active)

**`POST /change-password` — Đổi mật khẩu [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - Email: Bắt buộc, định dạng email hợp lệ
  - OldPassword: Bắt buộc, chuỗi không rỗng
  - NewPassword: Bắt buộc, chuỗi không rỗng
- **Validation từ Service**:
  - Kiểm tra user tồn tại theo email
  - Xác minh mật khẩu cũ với hash trong database
  - Kiểm tra trạng thái user (phải Active)

**`POST /logout` — Đăng xuất [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Xác thực JWT token hợp lệ
  - Lấy userId từ token claims
  - Kiểm tra user tồn tại theo userId
  - Xóa refresh token khỏi database

#### **UserController** (`/api/User`):

**`POST /` — Tạo người dùng mới**
- **Validation từ DTO**:
  - Email: Bắt buộc, định dạng email hợp lệ, tối đa 255 ký tự
  - Password: Bắt buộc, tối đa 255 ký tự
  - FullName: Bắt buộc, từ 2-255 ký tự
  - PhoneNumber: Tùy chọn, tối đa 20 ký tự, định dạng số điện thoại Việt Nam
- **Validation từ Service**:
  - Kiểm tra email chưa tồn tại trong hệ thống
  - Hash mật khẩu trước khi lưu
  - Thiết lập IsVerified = false cho customer

**`POST /staff` — Tạo tài khoản nhân viên [Chỉ Admin]**
- **Validation từ DTO**:
  - Email: Bắt buộc, định dạng email hợp lệ, tối đa 255 ký tự
  - FullName: Bắt buộc, từ 2-255 ký tự
  - PhoneNumber: Tùy chọn, định dạng số điện thoại Việt Nam
- **Validation từ Service**:
  - Kiểm tra quyền admin của user hiện tại
  - Kiểm tra email chưa tồn tại trong hệ thống
  - Tạo mật khẩu ngẫu nhiên
  - Gửi email thông tin tài khoản
  - Thiết lập IsVerified = true

**`GET /{id}` — Lấy user theo ID [Admin/Staff Only]**
- **Validation từ Service**:
  - Kiểm tra quyền admin/staff
  - Kiểm tra user tồn tại theo ID

**`GET /?page=&pageSize=&role=` — Danh sách users với phân trang [Admin/Staff Only]**
- **Validation từ Controller**:
  - page >= 1
  - pageSize từ 1-100
- **Validation từ Service**:
  - Kiểm tra quyền admin/staff
  - Validate role parameter nếu có

**`PATCH /{id}` — Cập nhật thông tin user [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - Email: Tùy chọn, định dạng email hợp lệ nếu có
  - FullName: Tùy chọn, từ 2-255 ký tự nếu có
  - PhoneNumber: Tùy chọn, định dạng số điện thoại Việt Nam nếu có
  - Status: Tùy chọn, enum hợp lệ (Active/Inactive/Suspended/Deleted)
- **Validation từ Service**:
  - Kiểm tra user tồn tại theo ID
  - Kiểm tra tài khoản chưa bị xóa (Status != Deleted)
  - Parse và validate status enum
  - Thiết lập DeletedAt nếu status = Deleted

**`POST /{userId}/address` — Tạo địa chỉ cho user [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - LocationAddress: Bắt buộc, tối đa 500 ký tự
  - Province: Bắt buộc, tối đa 100 ký tự
  - District: Bắt buộc, tối đa 100 ký tự
  - Commune: Bắt buộc, tối đa 100 ký tự
- **Validation từ Service**:
  - Kiểm tra user tồn tại theo userId
  - Tạo địa chỉ và liên kết với user

**`PATCH /address/{addressId}` — Cập nhật địa chỉ [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - Các trường địa chỉ tùy chọn với cùng ràng buộc như tạo mới
- **Validation từ Service**:
  - Kiểm tra address tồn tại theo addressId
  - Kiểm tra address thuộc về user nào đó
  - Cập nhật cả address và user_address relationship

#### **CartController** (`/api/Cart`):

**`POST /add` — Thêm sản phẩm vào giỏ hàng [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - ProductId: Bắt buộc, phải > 0
  - Quantity: Bắt buộc, phải >= 0
- **Validation từ Service**:
  - Kiểm tra Quantity > 0 (không cho phép 0 khi thêm mới)
  - Lấy userId từ JWT token
  - Tạo cart nếu chưa tồn tại
  - Kiểm tra sản phẩm chưa có trong cart

**`PUT /update` — Cập nhật số lượng trong giỏ hàng [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - ProductId: Bắt buộc, phải > 0
  - Quantity: Bắt buộc, phải >= 0
- **Validation từ Service**:
  - Lấy userId từ JWT token
  - Kiểm tra cart tồn tại cho user
  - Kiểm tra sản phẩm tồn tại trong cart
  - Nếu quantity = 0 thì xóa sản phẩm khỏi cart

**`GET /` — Lấy giỏ hàng [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Lấy userId từ JWT token
  - Trả về cart với tất cả items hoặc thông báo giỏ hàng trống

#### **ProductController** (`/api/Product`):

**`GET /{id}` — Lấy sản phẩm theo ID [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Kiểm tra product tồn tại theo ID

**`GET /` — Lấy tất cả sản phẩm [Yêu cầu Authorization]**
- **Validation từ Service**: Không có validation đặc biệt

**`GET /category/{id}` — Lấy sản phẩm theo category ID [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Lấy products theo categoryId (có thể trả về danh sách rỗng)

**`PUT /{id}` — Cập nhật sản phẩm [Yêu cầu Authorization]**
- **Validation từ DTO**: Các trường cập nhật với validation tương ứng
- **Validation từ Service**:
  - Kiểm tra product tồn tại theo ID
  - Áp dụng partial update

#### **ProductCategoryController** (`/api/ProductCategory`):

**`POST /` — Tạo danh mục sản phẩm [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - Name: Bắt buộc, tối đa 255 ký tự
  - ParentId: Tùy chọn, phải > 0 nếu có
  - Description: Tùy chọn, tối đa 255 ký tự
  - IconUrl: Tùy chọn, tối đa 500 ký tự
- **Validation từ Service**:
  - Kiểm tra tên danh mục chưa tồn tại (unique)
  - Nếu có ParentId: kiểm tra parent category tồn tại và đang active
  - Tạo slug từ tên danh mục

**`GET /{id}` — Lấy danh mục theo ID [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Kiểm tra category tồn tại theo ID

**`GET /` — Lấy tất cả danh mục [Yêu cầu Authorization]**
- **Validation từ Service**: Không có validation đặc biệt

**`PATCH /{id}` — Cập nhật danh mục [Yêu cầu Authorization]**
- **Validation từ DTO**: Các trường tùy chọn với cùng ràng buộc như tạo mới
- **Validation từ Service**:
  - Kiểm tra category tồn tại và đang active
  - Nếu cập nhật Name: kiểm tra tên chưa tồn tại, tạo slug mới
  - Nếu cập nhật ParentId:
    - Kiểm tra category chưa là parent của category khác
    - Kiểm tra ParentId khác với chính ID của category
    - Kiểm tra parent category tồn tại

#### **OrderController** (`/api/Order`):

**`POST /` — Tạo đơn hàng [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - TaxAmount: Phải >= 0
  - DiscountAmount: Phải >= 0
  - AddressId: Bắt buộc
  - Notes: Tùy chọn, tối đa 500 ký tự
  - OrderDetails: Bắt buộc, ít nhất 1 item
- **Validation từ Service**:
  - Lấy userId từ JWT token
  - Kiểm tra user tồn tại
  - Kiểm tra address thuộc về user
  - Tính toán subtotal cho từng order detail
  - Tính tổng tiền đơn hàng

**`PATCH /{orderId}` — Cập nhật đơn hàng [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - AddressId: Tùy chọn, nếu có phải hợp lệ
  - OrderDetails: Tùy chọn, danh sách các thay đổi
- **Validation từ Service**:
  - Kiểm tra order tồn tại theo orderId
  - Nếu có AddressId: kiểm tra address thuộc về customer của order
  - Nếu có CancelledReason: Status bắt buộc phải truyền vào null hoặc Cancelled
  - Xử lý OrderDetails:
    - Nếu Id = 0: tạo mới (cần ProductId, Quantity, UnitPrice)
    - Nếu Id > 0: cập nhật existing (kiểm tra thuộc về order)
    - Nếu Quantity = 0: xóa order detail
    - Nếu không còn order detail nào: xóa cả order

**`GET /{orderId}` — Lấy đơn hàng theo ID [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Kiểm tra order tồn tại theo orderId

**`GET /me` — Lấy đơn hàng của user hiện tại [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Lấy userId từ JWT token

**`GET /user/{userId}` — Lấy đơn hàng theo userId [Admin/Staff Only]**
- **Validation từ Service**:
  - Kiểm tra quyền admin/staff

#### **FarmProfileController** (`/api/FarmProfile`):

**`POST /` — Tạo hồ sơ trang trại [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - FarmName: Bắt buộc, tối đa 255 ký tự
  - FarmSizeHectares: Bắt buộc, phải > 0
  - LocationAddress: Bắt buộc, tối đa 500 ký tự
  - Province: Bắt buộc, tối đa 100 ký tự
  - District: Bắt buộc, tối đa 100 ký tự
  - Commune: Bắt buộc, tối đa 100 ký tự
  - Latitude: Bắt buộc, từ -90 đến 90
  - Longitude: Bắt buộc, từ -180 đến 180
  - PrimaryCrops: Tùy chọn, tối đa 500 ký tự
- **Validation từ Service**:
  - Lấy userId từ JWT token
  - Tạo address và farm profile với transaction

**`GET /{id}` — Lấy hồ sơ trang trại theo ID [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Kiểm tra farm profile tồn tại theo ID

**`GET /User/{userId}` — Lấy hồ sơ trang trại theo userId [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Sắp xếp theo UpdatedAt giảm dần

**`PATCH /{id}` — Cập nhật hồ sơ trang trại [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - Status: Tùy chọn, enum hợp lệ
  - Các trường khác tùy chọn với cùng ràng buộc như tạo mới
- **Validation từ Service**:
  - Kiểm tra farm profile tồn tại
  - Kiểm tra address của farm tồn tại
  - Parse và validate status enum nếu có
  - Cập nhật cả farm profile và address

#### **CO2Controller** (`/api/CO2`):

**`POST /farm/{farmId}` — Tạo CO2 footprint [Yêu cầu Authorization]**
- **Validation từ DTO**:
  - MeasurementStartDate: Bắt buộc
  - MeasurementEndDate: Bắt buộc
  - Notes: Tùy chọn, tối đa 500 ký tự
  - ElectricityKwh: Bắt buộc, 0-99999999.99
  - GasolineLiters: Bắt buộc, 0-99999999.99
  - DieselLiters: Bắt buộc, 0-99999999.99
  - OrganicFertilizer: Bắt buộc, 0-99999999.99 kg
  - NpkFertilizer: Bắt buộc, 0-99999999.99 kg
  - UreaFertilizer: Bắt buộc, 0-99999999.99 kg
  - PhosphateFertilizer: Bắt buộc, 0-99999999.99 kg
- **Validation từ Service**:
  - Kiểm tra dữ liệu cho khoảng thời gian này chưa tồn tại
  - Kiểm tra farm có tọa độ địa lý (cần cho API bên ngoài)
  - Gọi SoilGrids API để lấy dữ liệu đất
  - Gọi Weather API để lấy dữ liệu thời tiết
  - Tính toán weighted averages
  - Tính toán CO2 footprint

**`GET /farm/{farmId}` — Lấy dữ liệu môi trường theo farmId [Yêu cầu Authorization]**
- **Validation từ Service**: Không có validation đặc biệt

**`GET /{id}` — Lấy dữ liệu môi trường theo ID [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Kiểm tra environmental data tồn tại theo ID

**`DELETE /{id}` — Xóa dữ liệu môi trường [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Hard delete với transaction (xóa cả fertilizer và energy usage liên quan)

#### **WeatherController** (`/api/Weather`):

**`GET /hourly/{farmId}` — Lấy thời tiết theo giờ [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Kiểm tra farm có tọa độ địa lý
  - Gọi Weather API với error handling

**`GET /daily/{farmId}` — Lấy thời tiết theo ngày [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Kiểm tra farm có tọa độ địa lý
  - Gọi Weather API với error handling

**`GET /current/{farmId}` — Lấy thời tiết hiện tại [Yêu cầu Authorization]**
- **Validation từ Service**:
  - Kiểm tra farm có tọa độ địa lý
  - Gọi Weather API với error handling

### 5) Xử lý lỗi và Exception

#### **Các loại Exception phổ biến:**
- **ArgumentNullException**: Khi DTO hoặc tham số bắt buộc bị null
- **InvalidOperationException**: Khi vi phạm business rules (email đã tồn tại, user không active, v.v.)
- **KeyNotFoundException**: Khi không tìm thấy entity theo ID
- **ArgumentOutOfRangeException**: Khi giá trị nằm ngoài khoảng cho phép
- **TimeoutException**: Khi gọi external API quá thời gian chờ
- **DbUpdateException**: Khi có lỗi database operation
- **UnauthorizedAccessException**: Khi không có quyền truy cập

#### **Validation Error Messages (Tiếng Việt):**
- "Email là bắt buộc", "Email không đúng định dạng"
- "Mật khẩu là bắt buộc", "Mật khẩu phải ít nhất 6 ký tự"
- "Họ tên là bắt buộc", "Họ tên phải từ 2 đến 255 ký tự"
- "Số điện thoại không đúng định dạng Việt Nam"
- "Email {email} đã tồn tại"
- "Không tìm thấy người dùng với ID {id}"
- "Tài khoản này đã bị xóa"
- "Người dùng chưa được xác thực"
- "Mã xác thực không đúng hoặc đã hết hạn"
- "Sản phẩm đã tồn tại trong giỏ hàng"
- "Tên danh mục sản phẩm đã tồn tại"
- "Category này đã là một category cha, không thể làm category con"
- "Địa chỉ không thuộc về người dùng này"
- "Dữ liệu CO2 footprint cho khoảng thời gian này đã tồn tại"
- "Nông trại chưa có tọa độ địa lý"
- "Server thời tiết hiện đang quá tải, vui lòng thử lại sau"

#### **HTTP Status Codes được sử dụng:**
- **200 OK**: Thành công
- **201 Created**: Tạo mới thành công
- **204 No Content**: Xóa thành công hoặc order bị xóa do hết item
- **400 Bad Request**: Validation errors, business rule violations
- **401 Unauthorized**: Chưa đăng nhập hoặc token không hợp lệ
- **403 Forbidden**: Không có quyền truy cập
- **404 Not Found**: Không tìm thấy entity
- **500 Internal Server Error**: Lỗi server hoặc external API

### 6) Phát triển tiếp theo (Roadmap rút gọn)
Theo `VerdantTech_Project_Info.txt`, các hạng mục lớn sẽ bổ sung dần:
- Khách hàng (Web/Mobile): profile nông trại, nhập pH, CO₂, forum, blog, cảnh báo thời tiết, chatbot AI, upload ảnh bệnh.
- Người bán (Web): hồ sơ nhà cung cấp, quản lý danh mục sản phẩm xanh, analytics, tài liệu kỹ thuật.
- Chuyên gia/Admin (Web Portal): phân quyền, giám sát hiệu năng, kiểm duyệt nội dung, đảm bảo chính xác AI.
- AI & Tự động hoá: tích hợp n8n/Botpress; nhận diện bệnh cây qua dịch vụ CV; khuyến nghị canh tác theo pH/CO₂/thời tiết.
- Environmental Monitoring Engine: cập nhật thời tiết 15 phút, cảm biến pH/độ ẩm (thiết kế API/device ingestion).

Các module trên chưa đầy đủ trong repo hiện tại, sẽ được thiết kế và triển khai theo lộ trình.


