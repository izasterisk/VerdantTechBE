# VerdantTech Solutions - Backend (.NET 8)

Nền tảng cung cấp thiết bị nông nghiệp xanh tích hợp AI hỗ trợ canh tác rau củ bền vững.

> **Lưu ý**: Nhiều chức năng trong tài liệu dự án vẫn đang phát triển dần (AI chatbot, nhận diện bệnh cây trồng, mobile app, vendor portal, expert portal, monitoring engine...). README này mô tả phần backend hiện có.

---

## 1. Kiến trúc và thư mục

- **Solution**: `VerdantTechSolution.sln`
- **Projects**:
  - `Controller/` - ASP.NET Core Web API host, Swagger, JWT, DI
  - `BLL/` - Business Logic Layer: DTOs, Services, Interfaces, Helpers
  - `DAL/` - Data Access Layer: DbContext, Models, Configurations, Repositories
  - `Infrastructure/` - Email sender, client tích hợp courier/weather/soil/Cloudinary
  - `DB/` - Schema, seed SQL

---

## 2. Yêu cầu môi trường

- .NET SDK 8.0+
- MySQL 8.x (hoặc tương thích)

---

## 3. Công nghệ chính

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core + MySQL Provider
- JWT Authentication (Bearer)
- AutoMapper
- MailKit (SMTP)
- Cloudinary (Upload files/images)
- PayOS (Payment gateway)
- SignalR (Real-time notifications)

---

## 4. Response Format

Tất cả API trả về `APIResponse`:

```json
{
  "status": 200,
  "message": "Success",
  "data": { ... }
}
```

---

## 5. Tài liệu API chi tiết

### Base URL: `/api/{ControllerName}`

Authorization mặc định là **Bearer token** trừ khi gắn `[AllowAnonymous]`.

---

## AuthController (`/api/Auth`)

### `POST /api/Auth/login` [AllowAnonymous]
Đăng nhập bằng email và mật khẩu.

**Request Body** (`LoginDTO`):
| Field | Type | Validation |
|-------|------|------------|
| email | string | Required, Email format |
| password | string | Required, min 6 ký tự |

**Response** (`LoginResponseDTO`):
```json
{
  "token": "string",
  "tokenExpiresAt": "datetime",
  "refreshToken": "string",
  "refreshTokenExpiresAt": "datetime",
  "user": {
    "id": "ulong",
    "email": "string",
    "fullName": "string",
    "role": "string",
    "avatarUrl": "string?",
    "isVerified": "bool"
  }
}
```

**Ràng buộc nghiệp vụ**:
- Từ chối nếu user không tồn tại hoặc `Status` là `Deleted`/`Suspended`
- Cập nhật refresh token, `LastLoginAt`, `UpdatedAt`

---

### `POST /api/Auth/google-login` [AllowAnonymous]
Đăng nhập bằng Google OAuth.

**Request Body** (`GoogleLoginDTO`):
| Field | Type | Validation |
|-------|------|------------|
| idToken | string | Required (token từ Google OAuth) |

**Ràng buộc nghiệp vụ**:
- Gọi Google API để xác minh token
- Nếu user chưa tồn tại, tự động tạo mới với `Role = Customer`
- Cập nhật refresh token & timestamp

---

### `POST /api/Auth/send-verification` [AllowAnonymous]
Gửi email xác minh với mã 8 chữ số.

**Request Body** (`SendEmailDTO`):
| Field | Type | Validation |
|-------|------|------------|
| email | string | Required, Email format, max 255 |

---

### `POST /api/Auth/verify-email` [AllowAnonymous]
Xác minh email bằng mã 8 chữ số.

**Request Body** (`VerifyEmailDTO`):
| Field | Type | Validation |
|-------|------|------------|
| email | string | Required, Email format, max 255 |
| code | string | Required, max 8 ký tự |

---

### `POST /api/Auth/refresh-token` [AllowAnonymous]
Làm mới JWT token bằng refresh token.

**Request Body** (`RefreshTokenDTO`):
| Field | Type | Validation |
|-------|------|------------|
| refreshToken | string | Required |

**Response** (`RefreshTokenResponseDTO`):
```json
{
  "token": "string",
  "tokenExpiresAt": "datetime",
  "refreshToken": "string",
  "refreshTokenExpiresAt": "datetime"
}
```

---

### `GET /api/Auth/profile` [Authorize]
Lấy thông tin profile người dùng hiện tại từ JWT token.

---

### `POST /api/Auth/forgot-password` [AllowAnonymous]
Gửi email đặt lại mật khẩu với mã 8 ký tự.

**Request Body** (`SendEmailDTO`):
| Field | Type | Validation |
|-------|------|------------|
| email | string | Required, Email format |

---

### `POST /api/Auth/reset-password` [AllowAnonymous]
Đặt lại mật khẩu bằng email, mã và mật khẩu mới.

**Request Body** (`ResetForgotPasswordDTO`):
| Field | Type | Validation |
|-------|------|------------|
| email | string | Required, Email format, max 255 |
| code | string | Required, max 8 ký tự |
| newPassword | string | Required, min 6, max 100 ký tự |

---

### `POST /api/Auth/change-password` [Authorize]
Đổi mật khẩu người dùng.

**Request Body** (`ChangePasswordDTO`):
| Field | Type | Validation |
|-------|------|------------|
| email | string | Required, Email format, max 255 |
| oldPassword | string | Required |
| newPassword | string | Required, min 6, max 100 ký tự |

---

### `POST /api/Auth/logout` [Authorize]
Đăng xuất - vô hiệu hóa refresh token.

> **Lưu ý**: Gọi endpoint này trước, sau đó mới xóa token trong localStorage.

---

## UserController (`/api/User`)

### `POST /api/User` [AllowAnonymous]
Tạo người dùng mới.

**Request Body** (`UserCreateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| email | string | Required, Email format, max 255 |
| password | string | Required, max 255 |
| fullName | string | Required, min 2, max 255 |
| phoneNumber | string? | Optional, max 20, regex VN phone |

> **Lưu ý**: Nếu không truyền role thì mặc định là `Customer`. Nếu `role=Admin/Staff` thì tự động `IsVerified=true` và gửi email tài khoản được cấp.

---

### `POST /api/User/staff` [Authorize: Admin]
Tạo tài khoản nhân viên mới với mật khẩu tự động.

**Request Body** (`StaffCreateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| email | string | Required, Email format |
| fullName | string | Required |
| phoneNumber | string? | Optional |

---

### `GET /api/User/{id}` [Authorize: Admin, Staff]
Lấy thông tin người dùng theo ID.

---

### `GET /api/User` [Authorize: Admin, Staff]
Lấy danh sách người dùng với phân trang và filter theo role.

**Query Parameters**:
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Số trang |
| pageSize | int | 10 | Số bản ghi/trang (max 100) |
| role | string? | null | Filter: Customer, Staff, Admin, Vendor |

---

### `PATCH /api/User/{id}` [Authorize]
Cập nhật thông tin người dùng.

**Request Body** (`UserUpdateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| fullName | string? | min 2, max 255 |
| phoneNumber | string? | max 20, regex VN phone |
| avatarUrl | string? | URL format, max 500 |
| status | enum? | Active, Inactive, Suspended, Deleted |

---

### `POST /api/User/{userId}/address` [Authorize]
Tạo địa chỉ mới cho người dùng.

**Request Body** (`UserAddressCreateDTO`)

---

### `PATCH /api/User/address/{addressId}` [Authorize]
Cập nhật địa chỉ theo ID địa chỉ.

**Request Body** (`UserAddressUpdateDTO`)

---

## AddressController (`/api/Address`)

Tất cả endpoint yêu cầu Bearer token.

### `GET /api/Address/provinces` [Authorize]
Lấy danh sách tất cả tỉnh/thành phố từ GoShip API.

---

### `GET /api/Address/districts` [Authorize]
Lấy danh sách quận/huyện theo tỉnh/thành phố.

**Query Parameters**:
| Param | Type | Description |
|-------|------|-------------|
| provinceId | string | Required, ID của tỉnh/thành |

---

### `GET /api/Address/communes` [Authorize]
Lấy danh sách phường/xã theo quận/huyện.

**Query Parameters**:
| Param | Type | Description |
|-------|------|-------------|
| districtId | string | Required, ID của quận/huyện |

---

## CartController (`/api/Cart`)

### `POST /api/Cart/add` [Authorize]
Thêm sản phẩm vào giỏ hàng.

**Request Body** (`CartDTO`):
| Field | Type | Validation |
|-------|------|------------|
| productId | ulong | Required, > 0 |
| quantity | int | Required, >= 0 |

> Endpoint sử dụng token để xác định người dùng và áp dụng thay đổi lên cart của người dùng đó.

---

### `PUT /api/Cart/update` [Authorize]
Cập nhật số lượng sản phẩm trong giỏ hàng.

**Request Body** (`CartDTO`): Giống trên.

> **Lưu ý**: Nếu `quantity = 0`, sản phẩm sẽ bị **HARD DELETE** khỏi giỏ hàng.

---

### `GET /api/Cart` [Authorize]
Lấy thông tin giỏ hàng của người dùng hiện tại.

---

## OrderController (`/api/Order`)

### `POST /api/Order/preview` [Authorize]
Tạo order preview để xem trước tổng tiền, phí ship và các dịch vụ vận chuyển khả dụng.

**Request Body** (`OrderPreviewCreateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| taxAmount | decimal | >= 0, default 0 |
| discountAmount | decimal | >= 0, default 0 |
| addressId | ulong | Required |
| orderPaymentMethod | enum | Required: Banking, COD |
| notes | string? | max 500 |
| orderDetails | List | Required, min 1 item |

**OrderDetailsPreviewCreateDTO**:
| Field | Type | Validation |
|-------|------|------------|
| productId | ulong | Required |
| quantity | int | Required, > 0 |
| discountAmount | decimal | >= 0 |

**Ràng buộc nghiệp vụ**:
- User phải tồn tại và `IsVerified == true`
- Gọi GoShip API lấy rates, cache preview 10 phút với `OrderPreviewId` (Guid)

---

### `POST /api/Order/{orderPreviewId:guid}` [Authorize]
Tạo đơn hàng thực từ preview.

**Path Parameter**: `orderPreviewId` (Guid)

**Request Body** (`OrderCreateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| priceTableId | int | Required (ID shipping service từ preview) |

**Ràng buộc nghiệp vụ**:
- Preview phải tồn tại trong cache (chưa hết 10 phút)
- Xóa preview khỏi cache sau khi tạo thành công

---

### `POST /api/Order/{orderId:long}/ship` [Authorize: Admin, Staff]
Xuất kho và gán số serial/số lô cho sản phẩm trước khi ship.

**Path Parameter**: `orderId` (ulong)

**Request Body** (`List<OrderDetailsExportDTO>`):
| Field | Type | Validation |
|-------|------|------------|
| orderDetailId | ulong | Required |
| serialNumbers | List<string>? | Optional (bắt buộc với máy móc - category 1,2) |
| lotNumber | string? | Optional (bắt buộc với vật tư - category 3,4) |

---

### `PUT /api/Order/{orderId:long}` [Authorize]
Cập nhật trạng thái đơn hàng hoặc hủy đơn hàng.

**Path Parameter**: `orderId` (ulong)

**Request Body** (`OrderUpdateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| status | enum | Required: Pending, Paid, Processing, Shipped, Delivered, Cancelled, Refunded |
| cancelledReason | string? | max 500 (required nếu status = Cancelled) |

> Người dùng chỉ có quyền cancel đơn hàng ở trạng thái Pending.

---

### `GET /api/Order/{orderId:long}` [Authorize]
Lấy chi tiết 1 đơn hàng.

---

### `GET /api/Order` [Authorize: Admin, Staff]
Danh sách tất cả đơn hàng (phân trang, lọc theo status).

**Query Parameters**:
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Số trang |
| pageSize | int | 10 | Số bản ghi/trang |
| status | string? | null | Filter: Pending, Paid, Confirmed, Processing, Shipped, Delivered, Cancelled, Refunded |

---

### `GET /api/Order/user/{userId:long}` [Authorize]
Danh sách đơn hàng của một khách hàng cụ thể.

**Query Parameters**: `page`, `pageSize`

---

## ProductController (`/api/Product`)

### `GET /api/Product` 
Danh sách sản phẩm (phân trang).

**Query Parameters**: `page` (default 1), `pageSize` (default 20)

---

### `GET /api/Product/{id}`
Chi tiết sản phẩm theo ID (đầy đủ images).

---

### `GET /api/Product/category/{categoryId}`
Danh sách sản phẩm theo category (phân trang).

---

### `GET /api/Product/vendor/{vendorId:long}`
Danh sách sản phẩm theo vendor (phân trang).

---

### `PUT /api/Product/{id}`
Cập nhật sản phẩm + quản lý ảnh (add/remove).

**Request Body**:
```json
{
  "data": { /* ProductUpdateDTO */ },
  "addImages": [ { "imageUrl", "imagePublicId", "purpose", "sortOrder" } ],
  "removeImagePublicIds": [ "publicId1", "publicId2" ]
}
```

**ProductUpdateDTO**:
| Field | Type | Validation |
|-------|------|------------|
| id | ulong | Required |
| categoryId | ulong | Required |
| vendorId | ulong | Required |
| productCode | string | Required, max 100 |
| productName | string | Required, max 255 |
| description | string? | max 500 |
| unitPrice | decimal | Required, > 0 |
| commissionRate | decimal | 0-100 |
| discountPercentage | decimal | 0-100 |
| energyEfficiencyRating | string? | max 10 |
| specifications | Dictionary | Key-value |
| manualUrls | string? | max 1000 |
| warrantyMonths | int | > 0, default 12 |
| stockQuantity | int | >= 0, default 0 |
| weightKg | decimal | Required, 0.001-50000 |
| dimensionsCm | object | { width, height, length } |
| isActive | bool | default true |
| viewCount | long | default 0 |
| soldCount | long | default 0 |
| ratingAverage | decimal | 0-5 |

---

### `PATCH /api/Product/{id}/emission`
Cập nhật `CommissionRate` của sản phẩm.

**Request Body** (`ProductUpdateEmissionDTO`):
```json
{ "commissionRate": 0.05 }
```

---

### `DELETE /api/Product/{id}`
Xoá sản phẩm.

---

## ProductCategoryController (`/api/ProductCategory`)

### `POST /api/ProductCategory` [Authorize]
Tạo danh mục sản phẩm mới.

**Request Body** (`ProductCategoryCreateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| name | string | Required, max 100 |
| description | string? | Optional |
| parentId | ulong? | Optional |

---

### `GET /api/ProductCategory` [Authorize]
Lấy danh sách tất cả danh mục sản phẩm.

---

### `GET /api/ProductCategory/{id}` [Authorize]
Lấy thông tin danh mục theo ID.

---

### `PATCH /api/ProductCategory/{id}` [Authorize]
Cập nhật danh mục sản phẩm.

> **Ràng buộc**: Nếu category đã là cha của category khác, không thể gán `ParentId`.

---

## ProductRegistrationsController (`/api/ProductRegistrations`)

### `GET /api/ProductRegistrations`
Danh sách đăng ký sản phẩm (phân trang).

**Query Parameters**: `page` (default 1), `pageSize` (default 20)

---

### `GET /api/ProductRegistrations/{id}`
Chi tiết đăng ký sản phẩm theo ID.

---

### `GET /api/ProductRegistrations/vendor/{vendorId}`
Danh sách đăng ký theo vendor (phân trang).

---

### `POST /api/ProductRegistrations` [multipart/form-data]
Tạo đăng ký sản phẩm mới.

**Form Fields**:
| Field | Type | Description |
|-------|------|-------------|
| Data | ProductRegistrationCreateDTO | Thông tin đăng ký |
| ManualFile | IFormFile? | File PDF hướng dẫn |
| Images | List&lt;IFormFile&gt;? | Hình ảnh sản phẩm |
| Certificate | List&lt;IFormFile&gt;? | File PDF chứng chỉ |

---

### `PUT /api/ProductRegistrations/{id}` [multipart/form-data]
Cập nhật đăng ký sản phẩm.

**Form Fields**:
| Field | Type | Description |
|-------|------|-------------|
| Data | ProductRegistrationUpdateDTO | Thông tin cập nhật |
| ManualFile | IFormFile? | File PDF hướng dẫn mới |
| Images | List&lt;IFormFile&gt;? | Hình ảnh mới |
| Certificate | List&lt;IFormFile&gt;? | Chứng chỉ mới |
| RemoveImagePublicIds | List&lt;string&gt;? | Public ID ảnh cần xóa |
| RemoveCertificatePublicIds | List&lt;string&gt;? | Public ID chứng chỉ cần xóa |

---

### `PATCH /api/ProductRegistrations/{id}/status`
Duyệt hoặc từ chối đăng ký sản phẩm.

**Request Body** (`ProductRegistrationChangeStatusDTO`):
| Field | Type | Validation |
|-------|------|------------|
| status | enum | Approved, Rejected |
| rejectionReason | string? | Required nếu Rejected |
| approvedBy | ulong? | Optional |

> Khi được duyệt (Approved), hệ thống tự động tạo `Product` mới từ thông tin đăng ký.

---

### `DELETE /api/ProductRegistrations/{id}`
Xoá đăng ký sản phẩm.

---

## ProductCertificateController (`/api/ProductCertificate`)

### `POST /api/ProductCertificate/upload` [multipart/form-data]
Tạo hàng loạt chứng nhận kèm file PDF.

**Form Fields**:
| Field | Type | Description |
|-------|------|-------------|
| ProductId | long | ID sản phẩm |
| CertificationCode[] | List&lt;string&gt; | Danh sách mã chứng nhận |
| CertificationName[] | List&lt;string&gt; | Danh sách tên chứng nhận |
| Files | List&lt;IFormFile&gt; | Danh sách file PDF |

> Số lượng `CertificationCode`, `CertificationName`, và `Files` phải bằng nhau.

---

### `POST /api/ProductCertificate` [multipart/form-data]
Tạo một chứng nhận kèm file.

---

### `GET /api/ProductCertificate`
Lấy danh sách tất cả chứng nhận (phân trang).

---

### `GET /api/ProductCertificate/by-product/{productId:long}`
Lấy danh sách chứng nhận của một sản phẩm (phân trang).

---

### `GET /api/ProductCertificate/{id:long}`
Chi tiết chứng nhận theo ID.

---

### `PUT /api/ProductCertificate/{id:long}` [multipart/form-data]
Cập nhật chứng nhận, có thể thêm/xóa file.

---

### `PATCH /api/ProductCertificate/status/{id:long}`
Duyệt hoặc từ chối chứng nhận.

**Request Body** (`ProductCertificateChangeStatusDTO`):
| Field | Type | Validation |
|-------|------|------------|
| status | enum | Verified, Rejected |
| rejectionReason | string? | Required nếu Rejected |

---

### `DELETE /api/ProductCertificate/{id}`
Xoá chứng nhận.

---

## ProductReviewController (`/api/ProductReview`)

### `POST /api/ProductReview` [Authorize: Customer] [multipart/form-data]
Tạo đánh giá sản phẩm.

**Form Fields**:
| Field | Type | Description |
|-------|------|-------------|
| Data | ProductReviewCreateDTO | Thông tin đánh giá |
| Images | List&lt;IFormFile&gt;? | Ảnh đính kèm |

> Chỉ có thể đánh giá một lần cho mỗi sản phẩm trong mỗi đơn hàng.

---

### `GET /api/ProductReview/{id:long}`
Lấy thông tin đánh giá theo ID.

---

### `GET /api/ProductReview/product/{productId:long}`
Danh sách đánh giá của một sản phẩm (phân trang).

---

### `GET /api/ProductReview/order/{orderId:long}` [Authorize]
Danh sách đánh giá của một đơn hàng (phân trang).

---

### `GET /api/ProductReview/customer/{customerId:long}` [Authorize]
Danh sách đánh giá của một khách hàng (phân trang).

---

### `PUT /api/ProductReview/{id:long}` [Authorize: Customer]
Cập nhật đánh giá (chỉ chủ sở hữu).

---

### `DELETE /api/ProductReview/{id:long}` [Authorize: Customer]
Xóa đánh giá (chỉ chủ sở hữu).

---

## ProductSerialController (`/api/product-serials`)

### `GET /api/product-serials/product/{productId}`
Lấy tất cả serials theo product ID.

---

### `GET /api/product-serials/batch/{batchId}`
Lấy tất cả serials theo batch ID.

---

### `PATCH /api/product-serials/{serialId}/status`
Cập nhật trạng thái của một product serial.

**Request Body** (`ProductSerialStatusUpdateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| id | ulong | Required (phải trùng với route) |
| status | enum | New status |

---

## BatchInventoryController (`/api/BatchInventory`)

### `GET /api/BatchInventory`
Lấy tất cả batch inventories (phân trang).

**Query Parameters**: `page` (default 1), `pageSize` (default 20)

---

### `GET /api/BatchInventory/product/{productId}`
Batch inventories theo product ID (phân trang).

---

### `GET /api/BatchInventory/vendor/{vendorId}`
Batch inventories theo vendor ID (phân trang).

---

### `GET /api/BatchInventory/{id}`
Chi tiết batch inventory theo ID.

---

### `POST /api/BatchInventory`
Tạo batch inventory mới.

**Request Body** (`BatchInventoryCreateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| productId | ulong | Required |
| sku | string | Required, max 100 |
| vendorId | ulong? | Optional |
| batchNumber | string | Required, max 100 |
| lotNumber | string | Required, max 100 |
| quantity | int | >= 0 |
| unitCostPrice | decimal | >= 0 |
| expiryDate | DateOnly? | Optional |
| manufacturingDate | DateOnly? | Optional |

---

### `PUT /api/BatchInventory/{id}`
Cập nhật batch inventory.

---

### `DELETE /api/BatchInventory/{id}`
Xóa batch inventory.

---

### `POST /api/BatchInventory/{id}/quality-check`
Kiểm tra chất lượng batch inventory.

**Request Body** (`BatchInventoryQualityCheckDTO`)

---

### `GET /api/BatchInventory/{batchId}/serials`
Lấy tất cả serials của một batch.

---

## ExportInventoryController (`/api/ExportInventory`)

### `POST /api/ExportInventory` [Authorize: Admin, Staff]
Tạo đơn xuất kho mới.

**Request Body** (`List<ExportInventoryCreateDTO>`):
| Field | Type | Validation |
|-------|------|------------|
| movementType | enum | ReturnToVendor, Damage, Loss, Adjustment (không được nhập Sale) |
| ... | ... | ... |

> **Lưu ý**: `MovementType = Sale` chỉ được sử dụng khi xuất hàng bán qua OrderService.

---

### `GET /api/ExportInventory/{id}` [Authorize: Admin, Staff]
Chi tiết đơn xuất kho theo ID.

---

### `GET /api/ExportInventory` [Authorize: Admin, Staff]
Danh sách đơn xuất kho với phân trang và filter.

**Query Parameters**:
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Số trang |
| pageSize | int | 10 | Số bản ghi/trang |
| movementType | string? | null | Filter: Sale, ReturnToVendor, Damage, Loss, Adjustment |

---

### `GET /api/ExportInventory/identity-numbers/{productId}` [Authorize]
Lấy danh sách số lô hoặc số sê-ri có sẵn theo ProductId.

---

### `GET /api/ExportInventory/exported-identity-numbers/{orderDetailId}` [Authorize]
Lấy danh sách số lô/sê-ri đã xuất kho theo OrderDetailId.

---

## FarmProfileController (`/api/FarmProfile`)

### `POST /api/FarmProfile` [Authorize]
Tạo hồ sơ trang trại cho người dùng hiện tại.

**Request Body** (`FarmProfileCreateDto`)

> Id chủ trang trại sẽ được lấy từ token đăng nhập.

---

### `GET /api/FarmProfile/{id}` [Authorize]
Lấy thông tin hồ sơ trang trại theo ID.

---

### `GET /api/FarmProfile/User/{userId}` [Authorize]
Lấy danh sách tất cả hồ sơ trang trại theo User ID.

---

### `PATCH /api/FarmProfile/{id}` [Authorize]
Cập nhật thông tin hồ sơ trang trại.

**Request Body** (`FarmProfileUpdateDTO`)

---

## CropController (`/api/farm/{farmId}/Crop`)

### `POST /api/farm/{farmId}/Crop` [Authorize]
Thêm danh sách cây trồng vào trang trại.

**Request Body** (`List<CropsCreateDTO>`)

**Ràng buộc nghiệp vụ**:
- Kiểm tra trùng lặp (tên + ngày trồng)
- Validate phương pháp trồng phù hợp với loại cây và kiểu canh tác
- Không cho phép trạng thái Completed/Deleted/Failed khi tạo mới

---

### `PATCH /api/farm/{farmId}/Crop` [Authorize]
Cập nhật danh sách cây trồng của trang trại.

**Request Body** (`List<CropsUpdateDTO>`)

> Mỗi DTO phải có `Id`.

---

## CO2Controller (`/api/CO2`)

### `POST /api/CO2/farm/{farmId}` [Authorize]
Tạo CO2 footprint cho trang trại.

**Request Body** (`CO2FootprintCreateDTO`)

---

### `GET /api/CO2/farm/{farmId}` [Authorize]
Lấy tất cả dữ liệu môi trường theo Farm ID.

---

### `GET /api/CO2/{id}` [Authorize]
Lấy dữ liệu môi trường theo ID.

---

### `DELETE /api/CO2/{id}` [Authorize]
Xóa dữ liệu môi trường (HARD DELETE).

---

## WeatherController (`/api/Weather`)

### `GET /api/Weather/hourly/{farmId}` [Authorize]
Lấy thông tin thời tiết theo giờ của nông trại (ngày hôm nay).

---

### `GET /api/Weather/daily/{farmId}` [Authorize]
Lấy thông tin thời tiết theo ngày của nông trại (7 ngày từ hôm nay).

---

### `GET /api/Weather/current/{farmId}` [Authorize]
Lấy thông tin thời tiết hiện tại của nông trại.

---

## ChatbotConversationController (`/api/ChatbotConversation`)

Tất cả endpoint yêu cầu Authorization.

### `POST /api/ChatbotConversation` [Authorize]
Tạo cuộc hội thoại chatbot mới với tin nhắn đầu tiên.

**Request Body** (`ChatbotMessageCreateDTO`)

> User ID được lấy từ token.

---

### `POST /api/ChatbotConversation/{conversationId}/message` [Authorize]
Gửi tin nhắn mới trong cuộc hội thoại hiện có.

---

### `PATCH /api/ChatbotConversation/{conversationId}` [Authorize]
Cập nhật thông tin cuộc hội thoại (title, context, is_active).

**Request Body** (`ChatbotConversationUpdateDTO`)

---

### `GET /api/ChatbotConversation` [Authorize]
Lấy danh sách tất cả cuộc hội thoại của người dùng hiện tại (phân trang).

---

### `GET /api/ChatbotConversation/{conversationId}/messages` [Authorize]
Lấy danh sách tin nhắn trong cuộc hội thoại (phân trang).

---

## NotificationController (`/api/Notification`)

### `PATCH /api/Notification/{id}/revert-read-status` [Authorize]
Đảo ngược trạng thái đã đọc/chưa đọc của thông báo.

---

### `GET /api/Notification/user/{userId}` [Authorize]
Lấy danh sách thông báo của người dùng (phân trang).

**Query Parameters**: `page` (default 1), `pageSize` (default 10)

---

### `DELETE /api/Notification/{id}` [Authorize]
Xóa thông báo theo ID.

---

## RequestTicketController (`/api/RequestTicket`)

### `POST /api/RequestTicket` [Authorize]
Tạo yêu cầu mới (hỗ trợ hoặc hoàn tiền).

**Request Body** (`RequestCreateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| requestType | enum | Required: RefundRequest, SupportRequest |
| title | string | Required, min 3, max 255 |
| description | string | Required, min 10, max 2000 |
| images | List&lt;RequestImageDTO&gt;? | Optional |

---

### `PATCH /api/RequestTicket/{requestId}/process` [Authorize: Admin, Staff]
Xử lý yêu cầu.

**Request Body** (`RequestProcessDTO`):
| Field | Type | Validation |
|-------|------|------------|
| status | enum | InReview, Approved, Rejected, Cancelled (không được Pending hoặc Completed) |
| replyNotes | string? | Required nếu Approved/Rejected/Cancelled |

---

### `POST /api/RequestTicket/{requestId}/message` [Authorize]
Gửi tin nhắn mới cho yêu cầu.

**Request Body** (`RequestMessageCreateDTO`)

> Tối đa 3 tin nhắn. Chỉ gửi được khi tất cả tin nhắn trước đó đã được phản hồi.

---

### `GET /api/RequestTicket/{requestId}` [Authorize]
Lấy thông tin yêu cầu theo ID.

---

### `GET /api/RequestTicket/my-requests` [Authorize]
Danh sách yêu cầu của người dùng hiện tại (phân trang).

---

### `GET /api/RequestTicket` [Authorize: Admin, Staff]
Danh sách tất cả yêu cầu (phân trang, filter).

**Query Parameters**:
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Số trang |
| pageSize | int | 10 | Số bản ghi/trang |
| requestType | enum? | null | RefundRequest, SupportRequest |
| requestStatus | enum? | null | Pending, InReview, Approved, Rejected, Completed, Cancelled |

---

## VendorProfilesController (`/api/VendorProfiles`)

### `GET /api/VendorProfiles`
Danh sách vendor theo phân trang.

**Query Parameters**: `page` (default 1), `pageSize` (default 10)

---

### `GET /api/VendorProfiles/{id}`
Chi tiết VendorProfile theo Id.

---

### `GET /api/VendorProfiles/by-user/{userId}`
VendorProfile theo UserId (vendorId).

---

### `POST /api/VendorProfiles` [multipart/form-data]
Tạo mới VendorProfile (vendor đăng ký).

**Form Fields**:
| Field | Type | Description |
|-------|------|-------------|
| VendorProfileCreateDTO | FromForm | Thông tin vendor |
| files | List&lt;IFormFile&gt; | File chứng chỉ (bắt buộc) |

> Tạo User (Role = Vendor) → VendorProfile → Address → UserAddress → VendorCertificate (Pending).

---

### `PUT /api/VendorProfiles/{id}`
Cập nhật VendorProfile.

**Request Body** (`VendorProfileUpdateDTO`)

---

### `DELETE /api/VendorProfiles/{id}`
Xóa VendorProfile (hard delete, không xóa user).

---

### `DELETE /api/VendorProfiles/account/{userId}`
Soft delete tài khoản vendor (set User.Status = Inactive).

---

### `POST /api/VendorProfiles/{id}/approve`
Duyệt vendor profile.

**Request Body** (`VendorProfileApproveDTO`)

> Duyệt → update User (IsVerified) → update VendorProfile → duyệt tất cả VendorCertificate → gửi email xác nhận.

---

### `POST /api/VendorProfiles/{id}/reject`
Từ chối vendor profile.

**Request Body** (`VendorProfileRejectDTO`):
| Field | Type | Validation |
|-------|------|------------|
| rejectionReason | string | Required |

---

## VendorCertificateController (`/api/VendorCertificate`)

### `GET /api/VendorCertificate/vendor/{vendorId}`
Danh sách certificates của vendor (phân trang).

---

### `GET /api/VendorCertificate/{id}`
Chi tiết certificate theo ID.

---

### `POST /api/VendorCertificate` [multipart/form-data]
Tạo nhiều certificates với file uploads.

**Form Fields**:
| Field | Type | Description |
|-------|------|-------------|
| CertificationCode[] | List&lt;string&gt; | Mã chứng nhận |
| CertificationName[] | List&lt;string&gt; | Tên chứng nhận |
| files | List&lt;IFormFile&gt; | File PDF (cùng số lượng) |

---

### `PUT /api/VendorCertificate/{id}` [multipart/form-data]
Cập nhật certificate.

---

### `DELETE /api/VendorCertificate/{id}`
Xóa certificate.

---

### `PATCH /api/VendorCertificate/{id}/change-status`
Thay đổi trạng thái certificate.

**Request Body** (`VendorCertificateChangeStatusDTO`)

---

## UserBankAccountsController (`/api/UserBankAccounts`)

### `POST /api/UserBankAccounts/user/{userId}` [Authorize]
Tạo tài khoản ngân hàng mới cho người dùng.

**Request Body** (`UserBankAccountCreateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| bankName | string | Required, max 100 |
| bankAccountHolderName | string | Required, max 100 |
| bankAccountNumber | string | Required, max 50 |

---

### `PATCH /api/UserBankAccounts/{accountId}/deactivate` [Authorize: Staff, Vendor, Admin]
Vô hiệu hóa tài khoản ngân hàng (soft delete).

---

### `GET /api/UserBankAccounts/user/{userId}` [Authorize: Customer, Vendor, Admin, Staff]
Danh sách tài khoản ngân hàng của user.

---

### `GET /api/UserBankAccounts/supported-banks`
Danh sách ngân hàng được hỗ trợ từ VietQR/PayOS.

> Lọc: `transferSupported=1`, `lookupSupported=1`, `isTransfer=1`, `support≠0`.

---

## WalletController (`/api/Wallet`)

### `POST /api/Wallet/{userId}/process-credits` [Authorize: Admin, Staff, Vendor]
Xử lý cộng tiền vào ví vendor từ các đơn hàng đã giao quá 7 ngày.

**Ràng buộc nghiệp vụ**:
- Tìm OrderDetail có `Order.Status == Delivered`, `DeliveredAt` <= 7 ngày trước, `IsCreditedToVendor == false`
- Ghi nhận giao dịch `Transaction` với `TransactionType = CommissionCredit`

---

### `POST /api/Wallet/cashout-request` [Authorize: Vendor]
Tạo yêu cầu rút tiền từ ví.

**Request Body** (`WalletCashoutRequestCreateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| bankAccountId | ulong | Required, > 0 |
| amount | int | Required, min 1000 |
| notes | string? | max 500 |

**Ràng buộc nghiệp vụ**:
- Vendor chỉ được có 1 yêu cầu `Pending` tại một thời điểm
- `bankAccountId` phải thuộc về vendor

---

### `GET /api/Wallet/{userId}/cashout-request` [Authorize: Admin, Staff, Vendor]
Lấy yêu cầu rút tiền đang `Pending` của vendor.

---

### `GET /api/Wallet/cashout-requests` [Authorize: Admin, Staff]
Danh sách tất cả yêu cầu rút tiền (phân trang).

---

### `GET /api/Wallet/{userId}/cashout-requests` [Authorize: Admin, Staff, Vendor]
Danh sách yêu cầu rút tiền của một vendor cụ thể (phân trang).

---

### `DELETE /api/Wallet/cashout-request` [Authorize: Vendor]
Xóa yêu cầu rút tiền đang `Pending`.

---

### `POST /api/Wallet/{userId}/process-cashout-manual` [Authorize: Admin, Staff]
Xử lý yêu cầu rút tiền thủ công.

**Request Body** (`WalletProcessCreateDTO`):
| Field | Type | Validation |
|-------|------|------------|
| status | enum | Completed, Failed, Cancelled |
| gatewayPaymentId | string? | Required nếu Completed |
| cancelReason | string? | Required nếu Failed/Cancelled |

---

### `POST /api/Wallet/{userId}/process-cashout` [Authorize: Admin, Staff]
Xử lý yêu cầu rút tiền tự động qua PayOS.

---

## CashoutController (`/api/Cashout`)

### `GET /api/Cashout/ip-address` [Authorize]
Lấy địa chỉ IP của server (chỉ sử dụng cho BE).

---

### `GET /api/Cashout/balance` [Authorize: Admin, Staff]
Lấy số dư tài khoản PayOS Payout.

---

### `POST /api/Cashout/refund/{requestId}` [Authorize: Admin, Staff]
Xử lý hoàn tiền cho yêu cầu refund qua PayOS.

**Request Body** (`RefundCreateDTO`):
| Field | Type | Description |
|-------|------|-------------|
| gatewayPaymentId | string? | Nếu có = đã thanh toán bằng tay, nếu null = tự động qua PayOS |

> Đơn hàng phải đã giao và chưa quá 7 ngày.

---

## PayOSController (`/api/PayOS`)

### `POST /api/PayOS/create/{orderId}` [Authorize]
Tạo payment link cho đơn hàng.

**Request Body** (`CreatePaymentDataDTO`)

---

### `POST /api/PayOS/webhook` [AllowAnonymous]
Webhook handler cho PayOS - nhận thông báo thanh toán.

> **IMPORTANT**: Luôn trả về 200 OK cho PayOS.

---

### `POST /api/PayOS/confirm-webhook` [Authorize]
Xác nhận webhook URL với PayOS.

**Request Body** (`ConfirmWebhookDTO`)

---

### `GET /api/PayOS/payment-info/{transactionId}` [Authorize]
Lấy thông tin payment link theo transaction ID.

---

## DashboardController (`/api/Dashboard`)

Tất cả endpoint yêu cầu `[Authorize: Admin, Staff]`.

### `GET /api/Dashboard/revenue`
Lấy doanh thu theo khoảng thời gian.

**Query Parameters**:
| Param | Type | Description |
|-------|------|-------------|
| from | DateOnly | Ngày bắt đầu (yyyy-MM-dd) |
| to | DateOnly | Ngày kết thúc (yyyy-MM-dd) |

---

### `GET /api/Dashboard/orders`
Thống kê đơn hàng theo khoảng thời gian.

**Query Parameters**: `from`, `to` (DateOnly)

---

### `GET /api/Dashboard/queues`
Số lượng items đang chờ xử lý.

> Vendor chưa verify, ProductRegistration/VendorCertificate/ProductCertificate/Request đang Pending.

---

### `GET /api/Dashboard/revenue/last-7-days`
Doanh thu 7 ngày gần nhất (bao gồm hôm nay).

---

## ForumCategoryController (`/api/ForumCategory`)

### `GET /api/ForumCategory`
Danh sách Forum Category (phân trang).

---

### `GET /api/ForumCategory/{id}`
Chi tiết Forum Category theo Id.

---

### `POST /api/ForumCategory`
Tạo mới Forum Category.

**Request Body** (`ForumCategoryCreateDTO`)

---

### `PUT /api/ForumCategory/{id}`
Cập nhật Forum Category.

**Request Body** (`ForumCategoryUpdateDTO`)

---

### `DELETE /api/ForumCategory/{id}`
Xóa Forum Category.

---

## ForumPostController (`/api/ForumPost`)

### `GET /api/ForumPost`
Danh sách bài viết forum (phân trang).

---

### `GET /api/ForumPost/category/{categoryId}`
Danh sách bài viết theo danh mục (phân trang).

---

### `GET /api/ForumPost/{id}`
Chi tiết bài viết (bao gồm content blocks, images, comments).

---

### `GET /api/ForumPost/{id}/with-comments`
Bài viết cùng toàn bộ comments.

---

### `POST /api/ForumPost` [multipart/form-data]
Tạo bài viết forum mới.

**Form Fields**:
| Field | Type | Description |
|-------|------|-------------|
| ForumPostCreateDTO | FromForm | Thông tin bài viết |
| AddImages | List&lt;IFormFile&gt;? | Hình ảnh đính kèm |

**Query Parameter**: `userId` (ulong)

---

### `PUT /api/ForumPost/{id}` [multipart/form-data]
Cập nhật bài viết forum.

---

### `DELETE /api/ForumPost/{id}`
Xóa bài viết forum.

---

### `PATCH /api/ForumPost/{id}/pin`
Pin/Unpin bài viết.

**Query Parameter**: `isPinned` (bool)

---

### `PATCH /api/ForumPost/{id}/status`
Thay đổi trạng thái hiển thị (Visible/Hidden).

**Query Parameter**: `status` (ForumPostStatus)

---

### `POST /api/ForumPost/{id}/view`
Tăng view cho bài viết.

---

### `POST /api/ForumPost/{id}/like`
Tăng like cho bài viết.

---

### `POST /api/ForumPost/{id}/dislike`
Tăng dislike cho bài viết.

---

## ForumCommentController (`/api/ForumComment`)

### `GET /api/ForumComment/post/{postId}`
Danh sách comment cha của bài viết (phân trang, bao gồm nested replies).

---

### `GET /api/ForumComment/{id}`
Chi tiết comment (bao gồm toàn bộ replies dạng tree).

---

### `POST /api/ForumComment`
Tạo comment mới / Reply comment.

**Request Body** (`ForumCommentCreateDTO`)

> Tự động gửi notification đến chủ bài viết hoặc chủ comment cha.

---

### `PUT /api/ForumComment/{id}`
Cập nhật nội dung comment.

**Request Body** (`ForumCommentUpdateDTO`)

> Người cập nhật phải trùng UserId trong comment.

---

### `DELETE /api/ForumComment/{id}`
Xóa comment và toàn bộ replies (cascade).

---

### `PATCH /api/ForumComment/{id}/status`
Đổi trạng thái comment.

**Query Parameter**: `status` (ForumCommentStatus: Visible, Hidden, Deleted)

---

### `POST /api/ForumComment/{id}/like`
Like comment.

---

### `POST /api/ForumComment/{id}/dislike`
Dislike comment.

---

## 6. Enums Reference

### UserStatus
- `Active`
- `Inactive`
- `Suspended`
- `Deleted`

### OrderStatus
- `Pending`
- `Paid`
- `Confirmed`
- `Processing`
- `Shipped`
- `Delivered`
- `Cancelled`
- `Refunded`

### OrderPaymentMethod
- `Banking`
- `COD`

### RequestType
- `RefundRequest`
- `SupportRequest`

### RequestStatus
- `Pending`
- `InReview`
- `Approved`
- `Rejected`
- `Completed`
- `Cancelled`

### CashoutStatus
- `Pending`
- `Completed`
- `Failed`
- `Cancelled`

### MovementType (ExportInventory)
- `Sale`
- `ReturnToVendor`
- `Damage`
- `Loss`
- `Adjustment`

### ProductCertificateStatus
- `Pending`
- `Verified`
- `Rejected`

### ForumPostStatus
- `Visible`
- `Hidden`

### ForumCommentStatus
- `Visible`
- `Hidden`
- `Deleted`

---

## 7. Pagination Response Format

```json
{
  "data": [ ... ],
  "currentPage": 1,
  "pageSize": 10,
  "totalPages": 5,
  "totalRecords": 50,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

## 8. Error Response Format

```json
{
  "status": 400,
  "message": "Error message",
  "data": null
}
```

### Common HTTP Status Codes
- `200` - OK
- `201` - Created
- `204` - No Content
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `409` - Conflict
- `500` - Internal Server Error
- `503` - Service Unavailable

---

## 9. Roles

| Role | Description |
|------|-------------|
| Customer | Khách hàng - người mua hàng |
| Vendor | Nhà cung cấp - bán hàng |
| Staff | Nhân viên - hỗ trợ xử lý |
| Admin | Quản trị viên - toàn quyền |

---

## 10. Notes

- Tất cả thời gian sử dụng **UTC**
- File upload sử dụng **Cloudinary**
- Payment gateway sử dụng **PayOS**
- Shipping rates từ **GoShip API**
- Weather data từ **Open-Meteo API**
