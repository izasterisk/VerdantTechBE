## VerdantTech Solutions - Backend (.NET 8)

Nền tảng cung cấp thiết bị nông nghiệp xanh tích hợp AI hỗ trợ canh tác rau củ bền vững.

Lưu ý: Nhiều chức năng trong tài liệu dự án vẫn đang phát triển dần (AI chatbot, nhận diện bệnh cây trồng, mobile app, vendor portal, expert portal, monitoring engine...). README này mô tả phần backend hiện có.

### 1) Kiến trúc và thư mục
- **Solution**: `VerdantTechSolution.sln`
- **Projects**:
  - `Controller/` (ASP.NET Core Web API host, Swagger, JWT, DI)
  - `BLL/` (Business Logic Layer: DTOs, Services, Interfaces, Helpers)
  - `DAL/` (Data Access Layer: DbContext, Models, Configurations, Repositories)
  - `Infrastructure/` (Email sender, client tích hợp courier/weather/soil)
  - `DB/` (schema, seed SQL)

### 2) Yêu cầu môi trường
- .NET SDK 8.0+
- MySQL 8.x (hoặc tương thích)

### 3) Công nghệ chính đang dùng
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core + MySQL Provider
- JWT Authentication (Bearer)
- AutoMapper
- MailKit (SMTP)
### 4) Tài liệu API & validation
Tất cả API nằm trong base `/api/{ControllerName}` và trả về `APIResponse` gồm `status`, `message`, `data`. Authorization mặc định là Bearer token trừ khi gắn `[AllowAnonymous]`.

#### AuthController (`/api/Auth`)
- `POST /api/Auth/login` (AllowAnonymous)
  - Body `LoginDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | email | string | Required, email hợp lệ, tối đa 255 ký tự |
    | password | string | Required, tối thiểu 6 ký tự |

  - Ràng buộc nghiệp vụ:
    - Từ chối nếu user không tồn tại hoặc `Status` là `Deleted`/`Suspended`.
    - Yêu cầu mật khẩu đúng và user đã `IsVerified`.
    - Cập nhật refresh token, `LastLoginAt`, `UpdatedAt`.

- `POST /api/Auth/google-login` (AllowAnonymous)
  - Body `GoogleLoginDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | idToken | string | Required (token từ Google OAuth) |

  - Ràng buộc nghiệp vụ:
    - Gọi Google API để xác minh token.
    - Tự động tạo user mới với vai trò `Customer` nếu chưa tồn tại.
    - Cập nhật refresh token & timestamp tương tự login thường.

- `POST /api/Auth/send-verification` (AllowAnonymous)
  - Body `SendEmailDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | email | string | Required, email hợp lệ, tối đa 255 ký tự |

  - Ràng buộc nghiệp vụ:
    - User phải tồn tại, trạng thái không bị khóa, chưa `IsVerified`.
    - Phát sinh mã 8 chữ số, lưu vào user, gửi email xác minh.

- `POST /api/Auth/verify-email` (AllowAnonymous)
  - Body `VerifyEmailDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | email | string | Required, email hợp lệ, tối đa 255 ký tự |
    | code | string | Required, đúng 8 ký tự |

  - Ràng buộc nghiệp vụ:
    - Chỉ chấp nhận mã trong thời gian `AuthConstants.VERIFICATION_CODE_EXPIRE_MINUTES`.
    - Từ chối nếu user đã verified, mã sai hoặc đã hết hạn.

- `POST /api/Auth/refresh-token` (AllowAnonymous)
  - Body `RefreshTokenDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | refreshToken | string | Required |

  - Ràng buộc nghiệp vụ:
    - Ánh xạ refresh token với user, từ chối nếu token hết hạn/không tồn tại.
    - Phát sinh cặp token mới và gia hạn thời gian sống.

- `GET /api/Auth/profile` (Authorize)
  - Ràng buộc nghiệp vụ:
    - Đọc `NameIdentifier` từ JWT để lấy user id.
    - Trả về thông tin user, 404 nếu không tồn tại.

- `POST /api/Auth/forgot-password` (AllowAnonymous)
  - Body `SendEmailDTO` (như trên).
  - Ràng buộc nghiệp vụ:
    - User phải tồn tại và trạng thái hợp lệ.
    - Tạo mã 8 ký tự, gửi email quên mật khẩu, lưu `VerificationToken`.

- `POST /api/Auth/reset-password` (AllowAnonymous)
  - Body `ResetForgotPasswordDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | email | string | Required, email hợp lệ, tối đa 255 ký tự |
    | code | string | Required, đúng 8 ký tự |
    | newPassword | string | Required, tối thiểu 6 ký tự, tối đa 100 ký tự |

  - Ràng buộc nghiệp vụ:
    - Xác thực mã code giống forgot password, chưa hết hạn.
    - Cập nhật `PasswordHash` với mật khẩu mới.

- `POST /api/Auth/change-password` (Authorize)
  - Body `ChangePasswordDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | email | string | Required, email hợp lệ, tối đa 255 ký tự |
    | oldPassword | string | Required |
    | newPassword | string | Required, tối thiểu 6 ký tự, tối đa 100 ký tự |

  - Ràng buộc nghiệp vụ:
    - Email phải thuộc user hiện tại và mật khẩu cũ chính xác.
    - User phải có trạng thái hợp lệ (không `Deleted`/`Suspended`).

- `POST /api/Auth/logout` (Authorize)
  - Ràng buộc nghiệp vụ:
    - Lấy user id từ JWT; trả lỗi nếu thiếu claim hoặc không parse được.
    - Xóa refresh token khỏi database nếu tồn tại.

#### CartController (`/api/Cart`)
- `POST /api/Cart/add` (Authorize)
  - Body `CartDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | productId | ulong | Required, >= 1 |
    | quantity | int | Required, DataAnnotation `>= 0`; service yêu cầu `>= 1` |

  - Ràng buộc nghiệp vụ:
    - Tạo cart mới nếu user chưa có.
    - Từ chối nếu sản phẩm đã có trong cart.
    - Trả về cart sau khi thêm, kèm danh sách ảnh sản phẩm.

- `PUT /api/Cart/update` (Authorize)
  - Body `CartDTO` (giống trên).
  - Ràng buộc nghiệp vụ:
    - Từ chối nếu cart hoặc item không tồn tại với user.
    - Nếu `quantity == 0` sẽ xóa item (hard delete); ngược lại cập nhật số lượng.

- `GET /api/Cart` (Authorize)
  - Ràng buộc nghiệp vụ:
    - Trả về cart của user, hoặc thông báo giỏ hàng rỗng nếu chưa có item.
#### CO2Controller (`/api/CO2`)
- `POST /api/CO2/farm/{farmId}` (Authorize)
  - Path `farmId`: ulong > 0.
  - Body `CO2FootprintCreateDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | measurementStartDate | DateOnly | Required |
    | measurementEndDate | DateOnly | Required |
    | notes | string? | Optional, tối đa 500 ký tự |
    | electricityKwh | decimal | Required, 0-99.999.999,99 |
    | gasolineLiters | decimal | Required, >= 0 |
    | dieselLiters | decimal | Required, >= 0 |
    | organicFertilizer | decimal | Required, >= 0 |
    | npkFertilizer | decimal | Required, >= 0 |
    | ureaFertilizer | decimal | Required, >= 0 |
    | phosphateFertilizer | decimal | Required, >= 0 |

  - Ràng buộc nghiệp vụ:
    - Không cho phép trùng khoảng ngày trên cùng farm.
    - Farm phải có tọa độ lat/long hợp lệ; ngược lại trả lỗi.
    - Gọi SoilGrids & Weather API, tính toán và lưu dữ liệu trong transaction.

- `GET /api/CO2/farm/{farmId}` (Authorize)
  - Trả về danh sách footprint của farm.

- `GET /api/CO2/{id}` (Authorize)
  - Trả về footprint theo id, 404 nếu không tồn tại.

- `DELETE /api/CO2/{id}` (Authorize)
  - Hard delete; trả về thông báo thành công/thất bại.

#### AddressController (`/api/Address`)
Các endpoint đều yêu cầu Bearer token.
- `GET /api/Address/provinces`
  - Gọi GoShip API, trả về danh sách tỉnh/thành.

- `GET /api/Address/districts?provinceId={provinceId}`
  - Query `provinceId`: chuỗi ID của tỉnh/thành.

- `GET /api/Address/communes?districtId={districtId}`
  - Query `districtId`: chuỗi ID của quận/huyện.

#### FarmProfileController (`/api/FarmProfile`)
- `POST /api/FarmProfile` (Authorize)
  - Body `FarmProfileCreateDto`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | farmName | string | Required, tối đa 255 ký tự |
    | farmSizeHectares | decimal | Required, > 0 |
    | locationAddress | string | Required, tối đa 500 ký tự |
    | province | string | Required, tối đa 100 ký tự |
    | district | string | Required, tối đa 100 ký tự |
    | commune | string | Required, tối đa 100 ký tự |
    | provinceCode | int | Required, >= 1 |
    | districtCode | int | Required, >= 1 |
    | communeCode | int | Required, >= 1 |
    | latitude | decimal | Required, -90 đến 90 |
    | longitude | decimal | Required, -180 đến 180 |
    | primaryCrops | string? | Optional, tối đa 500 ký tự |

  - Ràng buộc nghiệp vụ:
    - Service gán `UserId` từ JWT.
    - Tạo farm profile + address cùng transaction.

- `GET /api/FarmProfile/{id}` (Authorize)
  - 404 nếu không tìm thấy.

- `GET /api/FarmProfile/User/{userId}` (Authorize)
  - Trả về danh sách farm profile theo user id.

- `PATCH /api/FarmProfile/{id}` (Authorize)
  - Body `FarmProfileUpdateDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | farmName | string? | tối đa 255 ký tự |
    | farmSizeHectares | decimal? | > 0 |
    | locationAddress | string? | tối đa 500 ký tự |
    | province/district/commune | string? | mỗi field tối đa 100 ký tự |
    | provinceCode/districtCode/communeCode | int? | >= 1; phải đồng bộ với field tên tương ứng |
    | latitude | decimal? | -90 đến 90 |
    | longitude | decimal? | -180 đến 180 |
    | status | FarmProfileStatus? | Optional enum (Active, Maintenance, Deleted) |
    | primaryCrops | string? | tối đa 500 ký tự |

  - Ràng buộc nghiệp vụ:
    - Farm phải tồn tại và có địa chỉ.
    - `AddressHelper.ValidateAddressFields` yêu cầu các cặp tên/mã cùng null hoặc cùng có giá trị.
#### OrderController (`/api/Order`)

**`POST /api/Order/preview`** (Authorize)  
Tạo order preview để xem trước tổng tiền, phí ship và các dịch vụ vận chuyển khả dụng.

- Body `OrderPreviewCreateDTO`:

  | Field | Type | Validation |
  | --- | --- | --- |
  | addressId | ulong | Required, > 0 |
  | orderPaymentMethod | enum | Required: `Banking`, `COD`, `Rent` |
  | taxAmount | decimal | >= 0 |
  | discountAmount | decimal | >= 0 |
  | notes | string? | Optional, max 500 ký tự |
  | orderDetails | List<OrderDetailPreviewCreateDTO> | Required, min 1 item |

- `OrderDetailPreviewCreateDTO`:

  | Field | Type | Validation |
  | --- | --- | --- |
  | productId | ulong | Required, > 0 |
  | quantity | int | Required, >= 1 |
  | discountAmount | decimal | >= 0 |

- Ràng buộc nghiệp vụ:
  - User (từ JWT) phải tồn tại, `IsVerified == true`.
  - `addressId` phải tồn tại và thuộc user (qua `UserAddress` hoặc `FarmProfile`).
  - Mỗi `productId` phải tồn tại, `IsActive == true`, `StockQuantity >= quantity`.
  - Nếu `orderPaymentMethod == Rent`: sản phẩm phải `ForRent == true`.
  - Tự động tính: subtotal, dimensions, weight từ sản phẩm.
  - Gọi GoShip API lấy rates, cache preview 10 phút với `OrderPreviewId` (Guid).
- Exceptions:
  - `KeyNotFoundException`: User/address/product không tồn tại, address không thuộc user.
  - `InvalidOperationException`: Sản phẩm hết hàng, không cho thuê khi chọn Rent.

**`POST /api/Order/{orderPreviewId}`** (Authorize)  
Tạo đơn hàng thực từ preview, chọn shipping service.

- Path: `orderPreviewId` (Guid)
- Body `OrderCreateDTO`:

  | Field | Type | Validation |
  | --- | --- | --- |
  | priceTableId | string | Required (ID shipping service từ preview) |

- Ràng buộc nghiệp vụ:
  - Preview phải tồn tại trong cache (chưa hết 10 phút).
  - `priceTableId` phải nằm trong `shippingDetails` của preview.
  - User phải tồn tại, `IsVerified == true`.
  - Sản phẩm phải còn đủ hàng (kiểm tra lại `StockQuantity`).
  - Transaction: tạo Order + OrderDetail, trừ stock, commit.
  - Xóa preview khỏi cache sau khi tạo thành công.
- Exceptions:
  - `KeyNotFoundException`: Preview hết hạn, priceTableId không hợp lệ, user/product không tồn tại.
  - `InvalidOperationException`: Hết hàng, lỗi transaction.

**`PUT /api/Order/{orderId}`** (Authorize)  
Cập nhật trạng thái đơn hàng (Pending → Paid → Processing → Shipped → Delivered) hoặc hủy.

- Path: `orderId` (ulong)
- Body `OrderUpdateDTO`:

  | Field | Type | Validation |
  | --- | --- | --- |
  | status | enum | Required: `Pending`, `Paid`, `Processing`, `Shipped`, `Delivered`, `Cancelled`, `Refunded` |
  | cancelledReason | string? | Optional, max 500 ký tự |

- Ràng buộc nghiệp vụ:
  - Order phải tồn tại.
  - Nếu có `cancelledReason` → `status` phải là `Cancelled`.
  - Validate chuyển trạng thái hợp lệ (`OrderHelper.ValidateOrderStatusTransition`): không lùi trạng thái, không chuyển từ Cancelled/Refunded.
  - **Tác động theo status:**
    - `Processing`: set `ConfirmedAt`.
    - `Delivered`: set `DeliveredAt`.
    - `Cancelled`: set `CancelledAt`, `CancelledReason`.
    - `Shipped`: gọi GoShip API tạo shipment, lưu `TrackingNumber`. Nếu COD: payer=0, codAmount=totalAmount.
  - Transaction: update order.
- Exceptions:
  - `KeyNotFoundException`: Order/address không tồn tại.
  - `InvalidOperationException`: Cung cấp cancelledReason nhưng status không phải Cancelled, chuyển trạng thái không hợp lệ, lỗi GoShip API.

**`GET /api/Order/{orderId}`** (Authorize)  
Lấy chi tiết 1 đơn hàng.

- Path: `orderId` (ulong)
- Response: `OrderResponseDTO` (customer, address, orderDetails với images).
- Exceptions: `KeyNotFoundException` nếu order không tồn tại.

**`GET /api/Order`** (Authorize)  
Danh sách đơn hàng (phân trang, lọc theo status).

- Query params:

  | Param | Type | Default | Description |
  | --- | --- | --- | --- |
  | page | int | 1 | Trang hiện tại |
  | pageSize | int | 10 | Số item/trang |
  | status | string? | null | Lọc: `Pending`, `Paid`, `Processing`, `Shipped`, `Delivered`, `Cancelled`, `Refunded` |

- Ràng buộc nghiệp vụ:
  - Parse `status` thành enum, bỏ qua nếu không hợp lệ.
  - Eager load: OrderDetails, Product, Customer, Address.
  - Mỗi order: `customer.address[0]` = địa chỉ order, `orderDetails[].product.images` đầy đủ.
- Response: `PagedResponse<OrderResponseDTO>` (data, currentPage, pageSize, totalPages, totalRecords, hasNextPage, hasPreviousPage).
- Exceptions: Không có (trả rỗng nếu không tìm thấy).

**`POST /api/Order/{orderId}/ship`** (Authorize, Roles: Admin, Staff)  
Xuất kho và gán số serial/số lô cho sản phẩm trong đơn hàng trước khi ship.

- Path: `orderId` (ulong)
- Body: `List<OrderDetailsShippingDTO>`

  | Field | Type | Validation |
  | --- | --- | --- |
  | productId | ulong | Required, >= 1 |
  | serialNumber | string? | Optional, max 50 ký tự (bắt buộc với máy móc - category 1,2) |
  | lotNumber | string? | Optional, max 50 ký tự (bắt buộc với vật tư - category 3,4) |

- Ràng buộc nghiệp vụ:
  - Order phải tồn tại.
  - Tạo Dictionary từ `order.OrderDetails` để theo dõi số lượng sản phẩm.
  - Mỗi `productId` trong DTO phải:
    - Tồn tại trong đơn hàng (có trong Dictionary).
    - Chưa xuất đủ số lượng (quantity > 0).
    - Có số serial (category 1,2) hoặc số lô (category 3,4) hợp lệ (so sánh case-insensitive với `.ToUpper()`).
  - Sau khi duyệt hết DTO, kiểm tra Dictionary:
    - Tất cả sản phẩm phải có quantity = 0 (đã xuất đủ).
  - Tạo `ExportInventory` với `MovementType=Sale`, `CreatedBy=staffId` (từ JWT).
- Response: `OrderResponseDTO` sau khi xuất kho thành công.
- Exceptions:
  - `ArgumentNullException`: DTO null hoặc rỗng.
  - `KeyNotFoundException`: Order không tồn tại, số serial/lô không tồn tại hoặc không thuộc sản phẩm.
  - `InvalidOperationException`: 
    - Sản phẩm không nằm trong đơn hàng.
    - Xuất nhiều hơn số lượng đã đặt.
    - Xuất ít hơn số lượng đã đặt (kiểm tra cuối).
    - Thiếu số serial (category 1,2) hoặc số lô (category 3,4).

#### ProductCategoryController (`/api/ProductCategory`)

**`POST /api/ProductCategory`** (Authorize)
Tạo danh mục sản phẩm mới.
- Body `ProductCategoryCreateDTO`:
  - `Name` (string, required, max 100)
  - `Description` (string, optional, max 500)
  - `ParentId` (ulong, optional)

**`GET /api/ProductCategory`** (Authorize)
Lấy danh sách tất cả danh mục sản phẩm.

**`GET /api/ProductCategory/{id}`** (Authorize)
Lấy thông tin danh mục sản phẩm theo ID.

**`PATCH /api/ProductCategory/{id}`** (Authorize)
Cập nhật danh mục sản phẩm.
- Body `ProductCategoryUpdateDTO` (các trường giống Create nhưng optional).
- Ràng buộc: Không thể gán `ParentId` nếu category đã là cha của một category khác.

**`DELETE /api/ProductCategory/{id}`** (Authorize)
Xóa một danh mục sản phẩm.
- Ràng buộc: Không thể xóa nếu category là cha của một category khác.

#### ProductCertificateController (`/api/ProductCertificate`)
Quản lý chứng nhận sản phẩm, bao gồm cả việc upload file.

**`POST /api/ProductCertificate/upload`** (Authorize)
Tạo hàng loạt chứng nhận kèm file PDF.
- Content-Type: `multipart/form-data`
- Form fields:
  - `ProductId` (long): ID sản phẩm.
  - `CertificationCode` (List<string>): Danh sách mã chứng nhận.
  - `CertificationName` (List<string>): Danh sách tên chứng nhận.
  - `Files` (List<IFormFile>): Danh sách file PDF.
- Ràng buộc nghiệp vụ:
  - Số lượng `CertificationCode`, `CertificationName`, và `Files` phải bằng nhau.
  - Upload file lên Cloudinary, sau đó tạo các `ProductCertificate` và `MediaLink` tương ứng trong một transaction.
- Response: `List<ProductCertificateResponseDTO>`.

**`POST /api/ProductCertificate`** (Authorize)
Tạo một chứng nhận kèm file.
- Content-Type: `multipart/form-data`
- Form fields:
  - `[FromForm] ProductCertificateCreateDTO dto`: Dữ liệu chứng nhận.
  - `[FromForm] List<IFormFile> files`: File PDF.
- Ràng buộc nghiệp vụ:
  - Upload file lên Cloudinary, tạo `ProductCertificate` và `MediaLink`.
- Response: `ProductCertificateResponseDTO`.

**`GET /api/ProductCertificate`** (Authorize)
Lấy danh sách tất cả chứng nhận (phân trang).
- Query: `page`, `pageSize`.
- Response: `PagedResponse<ProductCertificateResponseDTO>`.

**`GET /api/ProductCertificate/by-product/{productId}`** (Authorize)
Lấy danh sách chứng nhận của một sản phẩm (phân trang).
- Path: `productId` (ulong).
- Query: `page`, `pageSize`.
- Response: `PagedResponse<ProductCertificateResponseDTO>`.

**`GET /api/ProductCertificate/{id}`** (Authorize)
Lấy chi tiết chứng nhận theo ID.
- Path: `id` (long).
- Response: `ProductCertificateResponseDTO`.

**`PUT /api/ProductCertificate/{id}`** (Authorize)
Cập nhật chứng nhận, có thể thêm/xóa file.
- Content-Type: `multipart/form-data`
- Path: `id` (long).
- Form fields:
  - `[FromForm] ProductCertificateUpdateDTO form`: Dữ liệu cập nhật.
  - `[FromForm] List<IFormFile>? addFiles`: File mới để thêm.
  - `[FromForm] List<string>? removedFilePublicIds`: Public ID của file cần xóa.
- Response: `ProductCertificateResponseDTO`.

**`PATCH /api/ProductCertificate/status/{id}`** (Authorize)
Duyệt hoặc từ chối chứng nhận.
- Path: `id` (long).
- Body `ProductCertificateChangeStatusDTO`:
  - `Status` (ProductCertificateStatus: `Verified`, `Rejected`).
  - `RejectionReason` (string, optional, required nếu `Status` là `Rejected`).
- Response: `ProductCertificateResponseDTO`.

**`DELETE /api/ProductCertificate/{id}`** (Authorize)
Xóa chứng nhận.
- Path: `id` (long).
- Response: `true` nếu thành công.

#### ProductRegistrationsController (`/api/ProductRegistrations`)

**`GET /api/ProductRegistrations`** (Authorize)  
Danh sách đăng ký sản phẩm (phân trang).

- Query: `page` (default 1), `pageSize` (default 20)
- Response: `PagedResponse<ProductRegistrationReponseDTO>` (bao gồm images, certificates, manual URLs).
- Exceptions: Không có.

**`GET /api/ProductRegistrations/{id}`** (Authorize)  
Chi tiết đăng ký sản phẩm.

- Path: `id` (ulong)
- Response: `ProductRegistrationReponseDTO` hoặc 404.
- Exceptions: Không có (trả null nếu không tìm thấy).

**`GET /api/ProductRegistrations/vendor/{vendorId}`** (Authorize)  
Danh sách đăng ký theo vendor (phân trang).

- Path: `vendorId` (ulong)
- Query: `page`, `pageSize`
- Response: `PagedResponse<ProductRegistrationReponseDTO>`.
- Exceptions: Không có.

**`POST /api/ProductRegistrations`** (Authorize, Consumes: "multipart/form-data")
Tạo đăng ký sản phẩm.
- Form fields:
  - `[FromForm] CreateForm req`:
    - `Data` (`ProductRegistrationCreateDTO`): Dữ liệu chính của đơn đăng ký.
    - `ManualFile` (IFormFile?): File hướng dẫn sử dụng (PDF).
    - `Images` (List<IFormFile>?): Danh sách ảnh sản phẩm.
    - `Certificate` (List<IFormFile>?): Danh sách file chứng nhận.
- Ràng buộc nghiệp vụ:
  - Upload các file lên Cloudinary.
  - Tạo `ProductRegistration` và các `MediaLink` liên quan trong một transaction.
- Response: `ProductRegistrationReponseDTO`.

**`PUT /api/ProductRegistrations/{id}`** (Authorize, Consumes: "multipart/form-data")
Cập nhật đăng ký sản phẩm.
- Path: `id` (ulong).
- Form fields:
  - `[FromForm] UpdateForm req`:
    - `Data` (`ProductRegistrationUpdateDTO`): Dữ liệu cập nhật.
    - `ManualFile` (IFormFile?): File hướng dẫn sử dụng mới.
    - `Images` (List<IFormFile>?): Ảnh mới để thêm.
    - `Certificate` (List<IFormFile>?): Chứng nhận mới để thêm.
    - `RemoveImagePublicIds` (List<string>?): Public ID của ảnh cần xóa.
    - `RemoveCertificatePublicIds` (List<string>?): Public ID của chứng nhận cần xóa.
- Response: `ProductRegistrationReponseDTO`.

**`PATCH /api/ProductRegistrations/{id}/status`** (Authorize)  
Duyệt/từ chối đăng ký sản phẩm.
- Path: `id` (ulong)
- Body `ProductRegistrationChangeStatusDTO`:
  - `status` (enum: `Approved`, `Rejected`).
  - `rejectionReason` (string, required nếu `status` là `Rejected`).
- Ràng buộc nghiệp vụ:
  - Nếu `status` là `Approved`, tự động tạo một `Product` mới từ thông tin đăng ký.
- Response: 204 NoContent.

**`DELETE /api/ProductRegistrations/{id}`** (Authorize)  
Xóa đăng ký sản phẩm.
- Path: `id` (ulong).
- Response: 204 NoContent.

#### ProductController (`/api/Product`)

**`GET /api/Product`** (Authorize)  
Danh sách sản phẩm (phân trang).
- Query: `page`, `pageSize`.
- Response: `PagedResponse<ProductListItemDTO>`.

**`GET /api/Product/{id}`** (Authorize)  
Chi tiết sản phẩm.

- Path: `id` (long/ulong)
- Response: `ProductResponseDTO` (đầy đủ thông tin, toàn bộ images).
- Exceptions: 404 nếu không tồn tại.

**`GET /api/Product/category/{categoryId}`** (Authorize)  
Danh sách sản phẩm theo category (phân trang).

- Path: `categoryId` (long/ulong)
- Query: `page`, `pageSize`
- Response: `PagedResponse<ProductListItemDTO>`.
- Exceptions: Không có (trả rỗng nếu category không có sản phẩm).

**`GET /api/Product/vendor/{vendorId}`** (Authorize)  
Danh sách sản phẩm theo vendor (phân trang).

- Path: `vendorId` (long/ulong)
- Query: `page`, `pageSize`
- Response: `PagedResponse<ProductListItemDTO>`.
- Exceptions: Không có.

**`PUT /api/Product/{id}`** (Authorize)  
Cập nhật sản phẩm + quản lý ảnh (add/remove).

- Path: `id` (long/ulong)
- Body `UpdateRequest`:
  ```json
  {
    "data": ProductUpdateDTO,
    "addImages": [ { "imageUrl", "imagePublicId", "purpose", "sortOrder" }, ... ],
    "removeImagePublicIds": [ "publicId1", "publicId2" ]
  }
  ```

- `ProductUpdateDTO` (không cần `id` trong body, lấy từ route):

  | Field | Type | Validation |
  | --- | --- | --- |
  | categoryId | ulong | Required |
  | vendorId | ulong | Required |
  | productCode | string | Required, max 100 ký tự |
  | productName | string | Required, max 255 ký tự |
  | description | string? | max 500 ký tự |
  | unitPrice | decimal | Required, > 0 |
  | commissionRate | decimal | 0-100 |
  | discountPercentage | decimal | 0-100 |
  | energyEfficiencyRating | string? | max 10 ký tự (parse thành int 0-5) |
  | specifications | Dictionary<string, object>? | Optional |
  | manualUrls | string? | max 1000 ký tự |
  | publicUrl | string? | max 1000 ký tự |
  | warrantyMonths | int | >= 0 |
  | stockQuantity | int | >= 0 |
  | weightKg | decimal? | Optional |
  | dimensionsCm | { width, height, length }? | Optional, decimal |
  | isActive | bool | Default true |
  | viewCount | int | >= 0 |
  | soldCount | int | >= 0 |
  | ratingAverage | decimal | 0-5 |

- Ràng buộc nghiệp vụ:
  - Product phải tồn tại.
  - `categoryId`, `vendorId` phải tồn tại.
  - `energyEfficiencyRating` parse thành int nullable (0-5).
  - Xóa ảnh: tìm `MediaLink` với `OwnerType=Products`, `OwnerId=productId`, `ImagePublicId` trong `removeImagePublicIds`.
  - Thêm ảnh: insert `MediaLink` với `OwnerType=Products`, `OwnerId=productId`.
  - Update `UpdatedAt = DateTime.UtcNow`.
- Response: `ProductResponseDTO` đầy đủ (sau khi update + add/remove images).
- Exceptions:
  - `KeyNotFoundException`: Product không tồn tại.
  - `InvalidOperationException`: Category/Vendor không tồn tại.

**`PATCH /api/Product/{id}/emission`** (Authorize)  
Cập nhật `CommissionRate` của sản phẩm.

- Path: `id` (long/ulong)
- Body `ProductUpdateEmissionDTO`:
  ```json
  {
    "commissionRate": 0.05
  }
  ```
- Ràng buộc nghiệp vụ:
  - Product phải tồn tại.
  - `commissionRate` phải 0-1 (0-100%).
- Response: 204 NoContent nếu thành công, 404 nếu không tìm thấy.
- Exceptions: `KeyNotFoundException`.

**`DELETE /api/Product/{id}`** (Authorize)  
Xoá sản phẩm (và có thể dọn ảnh `MediaLink` liên quan).

- Path: `id` (long/ulong)
- Response: 204 NoContent nếu thành công, 404 nếu không tồn tại.
- Exceptions: Không có.

#### RequestTicketController (`/api/RequestTicket`)
Quản lý các yêu cầu hỗ trợ (support request) hoặc yêu cầu hoàn tiền (refund request).

**`POST /api/RequestTicket`** (Authorize, Roles: Customer,Vendor)
Tạo một "request ticket" mới.
- Body `RequestCreateDTO`:
  | Field | Type | Validation |
  | --- | --- | --- |
  | requestType | enum | Required: `RefundRequest`, `SupportRequest` |
  | title | string | Required, 3-255 ký tự |
  | description | string | Required, 10-2000 ký tự |
  | images | List<MediaLinkItemDTO>? | Optional, danh sách ảnh đính kèm |
- Ràng buộc nghiệp vụ:
  - `UserId` được lấy từ JWT.
  - Status mặc định là `Pending`.
  - Nếu có `images`, các ảnh sẽ được lưu và liên kết với request ticket.
- Response: `RequestResponseDTO`.

**`GET /api/RequestTicket/{requestId}`** (Authorize, Roles: Admin,Staff,Customer,Vendor)
Lấy thông tin chi tiết một request ticket.
- Path: `requestId` (ulong).
- Ràng buộc nghiệp vụ:
  - User phải là chủ sở hữu ticket hoặc là Admin/Staff.
- Response: `RequestResponseDTO` (bao gồm cả ảnh).

**`GET /api/RequestTicket/user/{userId}`** (Authorize, Roles: Admin,Staff,Customer,Vendor)
Lấy tất cả request tickets của một user.
- Path: `userId` (ulong).
- Ràng buộc nghiệp vụ:
  - User phải là chính user đó hoặc là Admin/Staff.
- Response: `List<RequestResponseDTO>`.

**`GET /api/RequestTicket`** (Authorize, Roles: Admin,Staff)
Lấy danh sách tất cả request tickets với filter và phân trang.
- Query params:
  | Param | Type | Default | Description |
  | --- | --- | --- | --- |
  | page | int | 1 | Trang hiện tại |
  | pageSize | int | 10 | Số item/trang (max 100) |
  | requestType | enum? | null | Lọc theo `RefundRequest` hoặc `SupportRequest` |
  | requestStatus | enum? | null | Lọc theo `Pending`, `InReview`, `Approved`, `Rejected`, `Completed`, `Cancelled` |
- Response: `PagedResponse<RequestResponseDTO>`.

**`PUT /api/RequestTicket/{requestId}/process`** (Authorize, Roles: Admin,Staff)
Xử lý một request ticket.
- Path: `requestId` (ulong).
- Body `RequestUpdateDTO`:
  | Field | Type | Validation |
  | --- | --- | --- |
  | status | enum | Required: `InReview`, `Approved`, `Rejected`, `Completed`, `Cancelled` |
  | replyNotes | string? | Required nếu status là `Approved`, `Rejected`, `Completed`, `Cancelled` |
- Ràng buộc nghiệp vụ:
  - Chỉ có thể xử lý các ticket có status là `Pending` hoặc `InReview`.
  - Không thể cập nhật trạng thái ngược về `Pending`.
  - Khi chuyển sang `InReview`, không được phép có `replyNotes`.
  - Khi chuyển sang các trạng thái cuối (`Approved`, `Rejected`, `Completed`, `Cancelled`), `replyNotes` là bắt buộc.
  - `ProcessedBy` và `ProcessedAt` sẽ được tự động gán.
- Response: `RequestResponseDTO` sau khi đã cập nhật.

#### UserBankAccountsController (`/api/UserBankAccounts`)
Các endpoint đều yêu cầu Bearer token.

**`POST /api/UserBankAccounts/user/{userId}`** (Authorize)
Tạo tài khoản ngân hàng mới cho người dùng.
- Path: `userId` (ulong).
- Body `UserBankAccountCreateDTO`:
  | Field | Type | Validation |
  | --- | --- | --- |
  | bankName | string | Required, max 255 |
  | bankAccountName | string | Required, max 255 |
  | bankAccountNumber | string | Required, max 50 |
- Ràng buộc nghiệp vụ:
  - User phải tồn tại.
  - Service gán `UserId` từ path.
- Response: `UserBankAccountResponseDTO`.

**`PATCH /api/UserBankAccounts/{accountId}`** (Authorize)
Cập nhật tài khoản ngân hàng.
- Path: `accountId` (ulong).
- Body `UserBankAccountUpdateDTO` (các trường optional, validation giống create).
- Ràng buộc nghiệp vụ:
  - Account phải tồn tại.
- Response: `UserBankAccountResponseDTO`.

**`DELETE /api/UserBankAccounts/{accountId}`** (Authorize, Roles: Customer,Vendor,Admin)
Xóa tài khoản ngân hàng.
- Path: `accountId` (ulong).
- Ràng buộc nghiệp vụ:
  - Chỉ chủ tài khoản hoặc Admin mới có quyền xóa.
- Response: `true` nếu thành công.

**`GET /api/UserBankAccounts/user/{userId}`** (Authorize, Roles: Customer,Vendor,Admin,Staff)
Lấy danh sách tài khoản ngân hàng của user.
- Path: `userId` (ulong).
- Response: `List<UserBankAccountResponseDTO>`.

**`GET /api/UserBankAccounts/supported-banks`**
Lấy danh sách ngân hàng được hỗ trợ từ PayOS/VietQR.
- Ràng buộc nghiệp vụ:
  - Gọi `IPayOSApiClient.GetAllSupportedBanksAsync`.
  - Lọc các ngân hàng có `transferSupported=1`, `lookupSupported=1`, `isTransfer=1`, `support!=0`.
- Response: Danh sách các ngân hàng.

#### WalletController (`/api/Wallet`)
Quản lý ví điện tử và luồng rút tiền của Vendor.

**`POST /api/Wallet/{userId}/process-credits`** (Authorize, Roles: Admin,Staff,Vendor)
Xử lý cộng tiền vào ví vendor từ các đơn hàng đã giao thành công và quá 7 ngày.
- Path: `userId` (ulong) - ID của vendor.
- Ràng buộc nghiệp vụ:
  - Tìm các `OrderDetail` có `Order.Status == Delivered`, `Order.DeliveredAt` <= 7 ngày trước, và `IsCreditedToVendor == false`.
  - Tính tổng tiền (sau khi trừ commission), cộng vào `Wallet.Balance`.
  - Đánh dấu `IsCreditedToVendor = true` cho các `OrderDetail` đã xử lý.
  - Ghi nhận giao dịch `Transaction` với `TransactionType = CommissionCredit`.
- Response: `WalletResponseDTO` (thông tin ví sau khi cập nhật).

**`POST /api/Wallet/cashout-request`** (Authorize, Roles: Vendor)
Vendor tạo yêu cầu rút tiền từ ví.
- Body `WalletCashoutRequestCreateDTO`:
  | Field | Type | Validation |
  | --- | --- | --- |
  | amount | decimal | Required, > 0 |
  | userBankAccountId | ulong | Required, > 0 |
- Ràng buộc nghiệp vụ:
  - Vendor chỉ được có 1 yêu cầu `Pending` tại một thời điểm.
  - `amount` phải nhỏ hơn hoặc bằng `Wallet.Balance`.
  - `userBankAccountId` phải thuộc về vendor.
- Response: `WalletCashoutRequestResponseDTO`.

**`GET /api/Wallet/{userId}/cashout-request`** (Authorize, Roles: Admin,Staff,Vendor)
Lấy thông tin yêu cầu rút tiền đang `Pending` của vendor.
- Path: `userId` (ulong) - ID của vendor.
- Response: `WalletCashoutRequestResponseDTO` hoặc 404 nếu không có.

**`GET /api/Wallet/cashout-requests`** (Authorize, Roles: Admin,Staff)
Lấy danh sách tất cả yêu cầu rút tiền (phân trang).
- Query: `page` (default 1), `pageSize` (default 10).
- Response: `PagedResponse<WalletCashoutRequestResponseDTO>`.

**`GET /api/Wallet/{userId}/cashout-requests`** (Authorize, Roles: Admin,Staff,Vendor)
Lấy danh sách yêu cầu rút tiền của một vendor cụ thể (phân trang).
- Path: `userId` (ulong) - ID của vendor.
- Query: `page` (default 1), `pageSize` (default 10).
- Response: `PagedResponse<WalletCashoutRequestResponseDTO>`.

**`DELETE /api/Wallet/cashout-request`** (Authorize, Roles: Vendor)
Vendor xóa yêu cầu rút tiền đang `Pending` của mình.
- Ràng buộc nghiệp vụ:
  - Yêu cầu phải tồn tại và có status là `Pending`.
- Response: `true` nếu thành công.

**`POST /api/Wallet/{userId}/process-cashout-manual`** (Authorize, Roles: Admin,Staff)
Admin/Staff xử lý yêu cầu rút tiền thủ công.
- Path: `userId` (ulong) - ID của vendor.
- Body `WalletProcessCreateDTO`:
  | Field | Type | Validation |
  | --- | --- | --- |
  | status | CashoutStatus | Required: `Completed`, `Failed`, `Cancelled` |
  | gatewayPaymentId | string? | Required nếu status là `Completed` |
  | cancelReason | string? | Required nếu status là `Failed` hoặc `Cancelled` |
- Ràng buộc nghiệp vụ:
  - Yêu cầu rút tiền `Pending` của user phải tồn tại.
  - Nếu `Completed`: Trừ tiền trong ví, cập nhật trạng thái `Cashout`, tạo `Transaction`.
  - Nếu `Failed`/`Cancelled`: Không trừ tiền, chỉ cập nhật trạng thái `Cashout`.
- Response: `CashoutResponseDTO`.

**`POST /api/Wallet/{userId}/process-cashout`** (Authorize, Roles: Admin,Staff)
Admin/Staff xử lý yêu cầu rút tiền tự động qua PayOS.
- Path: `userId` (ulong) - ID của vendor.
- Ràng buộc nghiệp vụ:
  - Yêu cầu rút tiền `Pending` của user phải tồn tại.
  - Gọi PayOS API để tạo lệnh chuyển tiền (`createTransfer`).
  - Nếu thành công: Trừ tiền trong ví, cập nhật trạng thái `Cashout`, tạo `Transaction`.
  - Nếu thất bại: Cập nhật trạng thái `Cashout` thành `Failed` và ghi lại lý do.
- Response: `CashoutResponseDTO`.

#### CashoutController (`/api/Cashout`)
Các endpoint hỗ trợ cho việc xử lý cashout.

**`GET /api/Cashout/ip-address`** (Authorize)
Lấy địa chỉ IP của server để sử dụng trong các yêu cầu API cần whitelist IP.
- Response: `{ "ipv4": "...", "ipv6": "..." }`.
