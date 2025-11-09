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

**`POST /api/ProductCertificate/create`** (Authorize)  
Vendor tạo chứng nhận sản phẩm.

- Body `ProductCertificateCreateDTO`:

  | Field | Type | Validation |
  | --- | --- | --- |
  | productId | ulong | Required, > 0 |
  | certificateName | string | Required, max 255 ký tự |
  | issuedBy | string | Required, max 255 ký tự |
  | issuedDate | DateTime | Required |
  | expiryDate | DateTime? | Optional |
  | certificateUrl | string? | max 1000 ký tự |
  | notes | string? | max 500 ký tự |

- Ràng buộc nghiệp vụ:
  - Không validate `productId` tồn tại (có thể tạo trước khi product được approve).
  - Set `CreatedAt = DateTime.UtcNow`.
- Response: `ProductCertificateResponseDTO`.
- Exceptions:
  - `ArgumentNullException`: DTO null.

**`GET /api/ProductCertificate/get-by-product-id/{productId}`** (Authorize)  
Lấy danh sách chứng nhận của sản phẩm.

- Path: `productId` (ulong)
- Response: `IReadOnlyList<ProductCertificateResponseDTO>`.
- Exceptions:
  - `ArgumentException`: `productId == 0`.

**`GET /api/ProductCertificate/get-by-id/{id}`** (Authorize)  
Lấy chứng nhận theo ID.

- Path: `id` (ulong)
- Response: `ProductCertificateResponseDTO` hoặc null.
- Exceptions:
  - `ArgumentException`: `id == 0`.

**`PUT /api/ProductCertificate/update/{id}`** (Authorize)  
Cập nhật chứng nhận sản phẩm.

- Path: `id` (ulong)
- Body `ProductCertificateUpdateDTO` (tất cả field optional, validation giống Create).
- Ràng buộc nghiệp vụ:
  - Certificate phải tồn tại.
  - Set `UpdatedAt = DateTime.UtcNow`.
  - Transaction: update certificate.
- Response: `ProductCertificateResponseDTO`.
- Exceptions:
  - `ArgumentNullException`: DTO null.
  - `KeyNotFoundException`: Certificate không tồn tại (từ repository).

#### ProductRegistrationController (`/api/ProductRegistrations`)

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

**`POST /api/ProductRegistrations`** (Authorize)  
Tạo đăng ký sản phẩm (multipart/form-data).

- Content-Type: `multipart/form-data`
- Form fields:
  - `Data`: `ProductRegistrationCreateDTO` (JSON string trong form field hoặc bracket-form)
  - `ManualFile`: IFormFile? (upload manual PDF)
  - `Images`: List<IFormFile>? (ảnh sản phẩm)
  - `Certificate`: List<IFormFile>? (file chứng chỉ)

- `ProductRegistrationCreateDTO`:

  | Field | Type | Validation |
  | --- | --- | --- |
  | vendorId | ulong | Required |
  | categoryId | ulong | Required |
  | proposedProductCode | string | Required, max 100 ký tự |
  | proposedProductName | string | Required, max 255 ký tự |
  | description | string? | max 500 ký tự |
  | unitPrice | decimal | Required, > 0 |
  | energyEfficiencyRating | string? | Parse thành int 0-5 |
  | specifications | Dictionary<string, object>? | Parse từ JSON string hoặc bracket-form |
  | warrantyMonths | int | >= 0 |
  | weightKg | decimal | > 0 |
  | dimensionsCm | { width, height, length } | Required, decimal > 0 |

- Ràng buộc nghiệp vụ:
  - `vendorId`, `categoryId` phải tồn tại.
  - `energyEfficiencyRating`: parse thành int, phải 0-5 nếu có giá trị.
  - `dimensionsCm` phải có đủ width/height/length.
  - Upload files lên Cloudinary:
    - `ManualFile` → `product-registrations/manuals` → lưu `manualUrl`, `manualPublicUrl`.
    - `Images` → `product-registrations/images` → tạo `MediaLink` với `OwnerType=ProductRegistrations`.
    - `Certificate` → `product-registrations/certificates` → tạo `MediaLink` với `OwnerType=ProductCertificates`.
  - Set `Status = Pending`, `CreatedAt/UpdatedAt = DateTime.UtcNow`.
  - Insert entity + MediaLinks trong transaction.
- Response: `ProductRegistrationReponseDTO` đầy đủ (sau khi tạo + hydrate images/certificates).
- Exceptions:
  - `InvalidOperationException`: Vendor/Category không tồn tại, energyEfficiencyRating không hợp lệ (< 0 hoặc > 5), dimensions không hợp lệ.

**`PUT /api/ProductRegistrations/{id}`** (Authorize)  
Cập nhật đăng ký sản phẩm (multipart/form-data).

- Path: `id` (ulong)
- Content-Type: `multipart/form-data`
- Form fields:
  - `Data`: `ProductRegistrationUpdateDTO` (tất cả field optional, validation giống Create)
  - `ManualFile`: IFormFile? (thay thế manual)
  - `Images`: List<IFormFile>? (thêm ảnh mới)
  - `Certificate`: List<IFormFile>? (thêm certificate mới)
  - `RemoveImagePublicIds`: List<string>? (xóa ảnh cũ)
  - `RemoveCertificatePublicIds`: List<string>? (xóa certificate cũ)

- Ràng buộc nghiệp vụ:
  - Registration phải tồn tại.
  - Validate giống Create.
  - Upload files mới lên Cloudinary.
  - Xóa `MediaLink` với publicId trong remove lists.
  - Update `UpdatedAt = DateTime.UtcNow`.
- Response: `ProductRegistrationReponseDTO` đầy đủ (sau update).
- Exceptions:
  - `KeyNotFoundException`: Registration không tồn tại.
  - `InvalidOperationException`: Validate fields không hợp lệ.

**`PATCH /api/ProductRegistrations/{id}/status`** (Authorize)  
Duyệt/từ chối đăng ký sản phẩm.

- Path: `id` (ulong)
- Body `ProductRegistrationChangeStatusDTO`:
  ```json
  {
    "status": "Approved",  // Pending, Approved, Rejected
    "rejectionReason": "...",  // Required nếu status = Rejected
    "approvedBy": 123  // ulong, user ID của người duyệt
  }
  ```

- Ràng buộc nghiệp vụ:
  - Registration phải tồn tại.
  - Nếu `status == Rejected`: `rejectionReason` bắt buộc.
  - Nếu `status == Approved`: tự động tạo `Product` từ registration, copy thông tin sang bảng Products.
- Response: 204 NoContent nếu thành công, 404 nếu không tìm thấy.
- Exceptions:
  - `KeyNotFoundException`: Registration không tồn tại.
  - `InvalidOperationException`: Thiếu rejectionReason khi reject.

**`DELETE /api/ProductRegistrations/{id}`** (Authorize)  
Xóa đăng ký sản phẩm (và MediaLinks liên quan).

- Path: `id` (ulong)
- Response: 204 NoContent nếu thành công, 404 nếu không tồn tại.
- Exceptions: Không có.

#### ProductController (`/api/Product`)

**`GET /api/Product`** (Authorize)  
Danh sách sản phẩm (phân trang).

- Query params:

  | Param | Type | Default |
  | --- | --- | --- |
  | page | int | 1 |
  | pageSize | int | 20 |

- Response: `PagedResponse<ProductListItemDTO>` (thông tin cơ bản, thumbnail đầu tiên nếu có).
- Exceptions: Không có.

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
Xóa sản phẩm (và có thể dọn ảnh `MediaLink` liên quan).

- Path: `id` (long/ulong)
- Response: 204 NoContent nếu thành công, 404 nếu không tồn tại.
- Exceptions: Không có.
    | viewCount | long | Optional |
    | soldCount | long | Optional |
    | ratingAverage | decimal | 0-5 |

  - Ràng buộc nghiệp vụ:
    - Sản phẩm phải tồn tại trước khi cập nhật.
#### UserController (`/api/User`)
- `POST /api/User` (AllowAnonymous)
  - Body `UserCreateDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | email | string | Required, email hợp lệ, tối đa 255 ký tự |
    | password | string | Required, tối đa 255 ký tự |
    | fullName | string | Required, 2-255 ký tự |
    | phoneNumber | string? | Optional, <= 20 ký tự, regex số điện thoại Việt Nam |

  - Ràng buộc nghiệp vụ:
    - Email phải unique.
    - Service hash password, mặc định `Role = Customer`, `Status = Active`, `IsVerified = false`.

- `POST /api/User/staff` (Authorize, Role `Admin`)
  - Body `StaffCreateDTO` (email, fullName bắt buộc, phone optional).
  - Ràng buộc nghiệp vụ:
    - Email phải unique.
    - Service phát sinh mật khẩu ngẫu nhiên, gửi email thông báo.
    - Tạo user với `Role = Staff`, `IsVerified = true`.

- `GET /api/User/{id}` (Authorize, Roles `Admin,Staff`)
  - 404 nếu user không tồn tại.

- `GET /api/User` (Authorize, Roles `Admin,Staff`)
  - Query params:
    - `page` (mặc định 1, > 0).
    - `pageSize` (mặc định 10, trong khoảng 1-100).
    - `role` (optional). Giá trị hợp lệ theo enum `UserRole` (không phân biệt hoa thường). Nếu bỏ trống -> chỉ trả về `Customer`.
  - Kết quả là `PagedResponse`.

- `PATCH /api/User/{id}` (Authorize)
  - Body `UserUpdateDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | fullName | string? | 2-255 ký tự |
    | phoneNumber | string? | <= 20 ký tự, regex Việt Nam |
    | avatarUrl | string? | <= 500 ký tự, URL hợp lệ |
    | status | UserStatus? | Optional enum (Active, Inactive, Suspended, Deleted) |

  - Ràng buộc nghiệp vụ:
    - User phải tồn tại.
    - Nếu đổi status sang `Deleted` thì set `DeletedAt = DateTime.UtcNow`.
    - Không cho phép đổi status nếu user đã `Deleted`.

- `POST /api/User/{userId}/address` (Authorize)
  - Body `UserAddressCreateDTO` (tất cả trường địa chỉ bắt buộc, code >= 1, lat [-90,90], long [-180,180]).
  - Ràng buộc nghiệp vụ:
    - User phải tồn tại.
    - Tạo địa chỉ mới và gắn cho user.

- `PATCH /api/User/address/{addressId}` (Authorize)
  - Body `UserAddressUpdateDTO` (tất cả optional, validation giống create, thêm `isDeleted` bool).
  - Ràng buộc nghiệp vụ:
    - Address phải tồn tại và thuộc user.
    - `AddressHelper.ValidateAddressFields` yêu cầu các cặp tên/mã đồng bộ.

#### WeatherController (`/api/Weather`)
- `GET /api/Weather/hourly/{farmId}` (Authorize)
  - Lấy thông tin thời tiết dự báo theo giờ trong ngày cho farm.

- `GET /api/Weather/daily/{farmId}` (Authorize)
  - Lấy thông tin thời tiết dự báo cho 7 ngày tới cho farm.

- `GET /api/Weather/current/{farmId}` (Authorize)
  - Lấy thông tin thời tiết hiện tại của farm.

#### Enum hợp lệ
| Enum | Giá trị | Ghi chú |
| --- | --- | --- |
| `UserRole` | Customer, Staff, Vendor, Admin | Dùng trong query `role`, hiển thị user |
| `UserStatus` | Active, Inactive, Suspended, Deleted | Dùng khi cập nhật user và trong auth |
| `VendorCertificateStatus` | Pending, Verified, Rejected | Chưa expose qua controller hiện tại |
| `ProductCertificateStatus` | Pending, Verified, Rejected | Chưa expose |
| `ProductRegistrationStatus` | Pending, Approved, Rejected | Chưa expose |
| `FarmProfileStatus` | Active, Maintenance, Deleted | Dùng trong `FarmProfileUpdateDTO` |
| `MessageType` | User, Bot, System | Dùng cho chatbot module tương lai |
| `ForumPostStatus` | Visible, Hidden | Dành cho forum module |
| `ForumCommentStatus` | Visible, Moderated, Deleted | Forum module |
| `RequestType` | RefundRequest, SupportRequest | Request module |
| `RequestStatus` | Pending, InReview, Approved, Rejected, Completed, Cancelled | Request module |
| `RequestPriority` | Low, Medium, High, Urgent | Request module |
| `OrderStatus` | Pending, Confirmed, Processing, Shipped, Delivered, Cancelled, Refunded | Hiển thị trong order response |
| `OrderPaymentMethod` | Banking, COD, Rent | Dùng trong order preview |
| `PaymentMethod` | CreditCard, DebitCard, Paypal, Stripe, BankTransfer, Cod | Hiện chưa expose qua controller |
| `PaymentGateway` | Stripe, Paypal, Vnpay, Momo, Manual | Giữ cho module thanh toán |
| `PaymentStatus` | Pending, Processing, Completed, Failed, Refunded, PartiallyRefunded | Trả về từ module payment |
| `TransactionType` | PaymentIn, WalletCashout, Refund, Adjustment | Tài chính |
| `TransactionStatus` | Pending, Completed, Failed, Cancelled | Tài chính |
| `CashoutStatus` | Pending, Processing, Completed, Failed, Cancelled | Tài chính |
| `CashoutType` | CommissionPayout, VendorPayment, Expense, Refund | Tài chính |
| `MovementType` | Sale, ReturnToVendor, Damage, Loss, Adjustment | Quản lý kho |
| `QualityCheckStatus` | NotRequired, Pending, Passed, Failed | Quản lý kho |
| `ConditionOnArrival` | New, Good, Fair, Damaged | Quản lý kho |
| `MediaOwnerType` | VendorCertificates, ChatbotMessages, Products, ProductRegistrations, ProductCertificates, ProductReviews, ForumPosts | Quản lý media |
| `MediaPurpose` | None, Front, Back | Quản lý media |

### 5) Xử lý lỗi và ngoại lệ
- `BaseController.HandleException` quy về các HTTP status:
  - 400: sai định dạng, validation, mapping.
  - 401: chưa đăng nhập hoặc token sai.
  - 403: thiếu quyền.
  - 404: không tìm thấy dữ liệu.
  - 409: xung đột dữ liệu (duplicate, foreign key).
  - 410: order đã bị xóa trong khi patch.
  - 499/408: request bị hủy/timeout (`OperationCanceledException`).
  - 500/503: lỗi hệ thống hoặc gọi API ngoài thất bại.
- `ValidateModel()` tự động gom thông điệp từ `ModelState` và trả về 400 dạng `APIResponse.ValidationError`.
- Các service chủ động ném:
  - `InvalidOperationException` khi vi phạm business rule (vd: product đã có trong cart, farm chưa có tọa độ, shipping detail không tồn tại).
  - `ArgumentException` khi tham số không hợp lệ (vd: parentId không tồn tại, mã courier sai định dạng).
  - `KeyNotFoundException` khi entity không tồn tại.
  - `ValidationException` khi vi phạm `AddressHelper` hoặc địa chỉ không thuộc user.

### 6) Định hướng phát triển tiếp
Theo `VerdantTech_Project_Info.txt`, các hạng mục dự kiến:
- Khách hàng (Web/Mobile): hồ sơ nông trại, dữ liệu pH/CO₂, forum, blog, cảnh báo thời tiết, chatbot AI, upload ảnh bệnh.
- Người bán (Web): hồ sơ vendor, quản lý danh mục sản phẩm xanh, analytics, tài liệu kỹ thuật.
- Chuyên gia/Admin (Web Portal): phân quyền, giám sát hiệu năng, kiểm duyệt nội dung, đảm bảo độ chính xác AI.
- AI & Tự động hóa: tích hợp Botpress, nhận diện bệnh cây qua dịch vụ CV, khuyến nghị canh tác theo pH/CO₂/thời tiết.
- Environmental Monitoring Engine: cập nhật thời tiết 15 phút, cảm biến pH/độ ẩm (thiết kế API device ingestion).

#### PayOSController (`/api/PayOS`)

**`POST /api/PayOS/create/{orderId}`** (Authorize)
Tạo link thanh toán PayOS cho một đơn hàng.
- Path `orderId`: ID của đơn hàng.
- Body `CreatePaymentDataDTO`:
  - `productName` (string, required)
  - `description` (string, required)
  - `returnUrl` (string, required, URL)
  - `cancelUrl` (string, required, URL)

**`POST /api/PayOS/webhook`** (AllowAnonymous)
Webhook nhận thông báo trạng thái thanh toán từ PayOS.

**`POST /api/PayOS/confirm-webhook`** (Authorize)
Xác nhận URL webhook với PayOS.
- Body `ConfirmWebhookDTO`:
  - `webhookUrl` (string, required, URL)

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
