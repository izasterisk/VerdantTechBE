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

#### CourierController (`/api/Courier`)
Các endpoint đều yêu cầu Bearer token.
- `GET /api/Courier/cities`
  - Gọi Courier API, trả về danh sách tỉnh/thành.

- `GET /api/Courier/districts/{cityId}`
  - Path `cityId`: chuỗi 6 chữ số (service từ chối nếu sai định dạng).

- `GET /api/Courier/wards/{districtId}`
  - Path `districtId`: chuỗi 6 chữ số.

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
- `POST /api/Order/preview` (Authorize)
  - Body `OrderPreviewCreateDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | taxAmount | decimal | >= 0 |
    | discountAmount | decimal | >= 0 |
    | addressId | ulong | Required |
    | orderPaymentMethod | OrderPaymentMethod | Required enum (Banking, COD, Installment) |
    | notes | string? | Optional, tối đa 500 ký tự |
    | orderDetails | List<OrderDetailPreviewCreateDTO> | Required, tối thiểu 1 item |

  - `OrderDetailPreviewCreateDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | productId | ulong | Required |
    | quantity | int | Required, >= 1 |
    | discountAmount | decimal | >= 0 |

  - Ràng buộc nghiệp vụ:
    - User phải tồn tại.
    - Địa chỉ phải tồn tại và thuộc user.
    - Mỗi sản phẩm phải tồn tại; tự động tính subtotal, trọng lượng và kích thước cho shipping.
    - Gọi Courier API lấy danh sách giao hàng, cache preview 10 phút.

- `POST /api/Order/{orderPreviewId}` (Authorize)
  - Path `orderPreviewId`: Guid.
  - Body `OrderCreateDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | shippingDetailId | string | Required (chọn từ preview) |

  - Ràng buộc nghiệp vụ:
    - Preview phải có trong cache và chưa hết hạn.
    - `shippingDetailId` phải tồn tại trong preview; nếu COD thì COD = tổng tiền trước phí ship.
    - Lưu order + order detail, trả về order mới.

- `PATCH /api/Order/{orderId}` (Authorize)
  - Body `OrderUpdateDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | notes | string? | Optional, tối đa 500 ký tự |
    | cancelledReason | string? | Optional, tối đa 500 ký tự |

  - Ràng buộc nghiệp vụ:
    - Order phải tồn tại.
    - Nếu có `cancelledReason`, set status = `Cancelled`; từ chối nếu status hiện tại là `Shipped` hoặc `Delivered`.
    - Nếu order bị xóa trong quá trình patch, service ném `OrderHelper.OrderDeletedException` -> controller trả 204.

- `GET /api/Order/{orderId}` (Authorize)
  - Trả về order chi tiết, 404 nếu không tồn tại.

- `GET /api/Order/me` (Authorize)
  - Trả về danh sách order của user từ JWT.

- `GET /api/Order/user/{userId}` (Authorize, Roles `Admin,Staff`)
  - Trả về danh sách order của user bất kỳ.

#### ProductCategoryController (`/api/ProductCategory`)
- `POST /api/ProductCategory` (Authorize)
  - Body `ProductCategoryCreateDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | name | string | Required, tối đa 255 ký tự |
    | parentId | ulong? | Optional, >= 1 |
    | description | string? | Optional, tối đa 255 ký tự |

  - Ràng buộc nghiệp vụ:
    - Tên category phải unique.
    - Nếu có `parentId`, parent phải tồn tại và `IsActive = true`.

- `GET /api/ProductCategory/{id}` (Authorize)
  - Trả về category, 404 nếu không tồn tại.

- `GET /api/ProductCategory` (Authorize)
  - Trả về toàn bộ danh sách category.

- `PATCH /api/ProductCategory/{id}` (Authorize)
  - Body `ProductCategoryUpdateDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | name | string? | tối đa 255 ký tự |
    | parentId | ulong? | >= 1 |
    | description | string? | tối đa 255 ký tự |
    | isActive | bool? | Optional |

  - Ràng buộc nghiệp vụ:
    - Category phải tồn tại và đang active.
    - Nếu đổi `name` cần unique, slug được cập nhật theo tên mới.
    - Nếu đặt `parentId`:
      - Không được đặt parent là chính nó.
      - Không được đổi parent nếu category đang là cha của category khác.
      - Parent phải tồn tại.

#### ProductController (`/api/Product`)
Các endpoint đều yêu cầu Bearer token.
- `GET /api/Product/{id}`
  - 404 nếu không tìm thấy.

- `GET /api/Product`
  - Trả về toàn bộ sản phẩm.

- `GET /api/Product/category/{id}`
  - Trả về sản phẩm theo category id.

- `PUT /api/Product/{id}`
  - Body `ProductUpdateDTO`:

    | Field | Type | Validation |
    | --- | --- | --- |
    | categoryId | ulong | Required |
    | vendorId | ulong | Required |
    | productCode | string | Required, tối đa 100 ký tự |
    | productName | string | Required, tối đa 255 ký tự |
    | description | string? | tối đa 500 ký tự |
    | unitPrice | decimal | Required, > 0 |
    | commissionRate | decimal | 0-100 |
    | discountPercentage | decimal | 0-100 |
    | energyEfficiencyRating | string? | tối đa 10 ký tự |
    | specifications | Dictionary<string, object> | Optional |
    | manualUrls | string? | tối đa 1000 ký tự |
    | images | string? | tối đa 1000 ký tự |
    | warrantyMonths | int | >= 1 |
    | stockQuantity | int | >= 0 |
    | weightKg | decimal? | Optional |
    | dimensionsCm | Dictionary<string, decimal> | Optional (length/width/height) |
    | isActive | bool | Mặc định true |
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
Các endpoint đều yêu cầu Bearer token.
- `GET /api/Weather/hourly/{farmId}`
- `GET /api/Weather/daily/{farmId}`
- `GET /api/Weather/current/{farmId}`

  - Ràng buộc nghiệp vụ chung:
    - Farm profile phải tồn tại và có tọa độ lat/long.
    - Gọi Weather API (hourly/daily/current); `TimeoutException` được ném thẳng, lỗi khác trả `InvalidOperationException` với thông điệp thân thiện.

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
| `OrderPaymentMethod` | Banking, COD, Installment | Dùng trong order preview |
| `PaymentMethod` | CreditCard, DebitCard, Paypal, Stripe, BankTransfer, Cod | Hiện chưa expose qua controller |
| `PaymentGateway` | Stripe, Paypal, Vnpay, Momo, Manual | Giữ cho module thanh toán |
| `PaymentStatus` | Pending, Processing, Completed, Failed, Refunded, PartiallyRefunded | Trả về từ module payment |
| `TransactionType` | PaymentIn, Cashout, WalletCredit, WalletDebit, Commission, Refund, Adjustment | Tài chính |
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
