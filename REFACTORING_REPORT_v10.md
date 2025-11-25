# BÁO CÁO REFACTORING SCHEMA V10
## Cấu trúc Single Source of Truth cho Payment/Transaction/Cashout

---

## 1. TỔNG QUAN

### Thay đổi chính trong Schema v10:
- **transactions**: Bảng trung tâm lưu tất cả dữ liệu chung (amount, status, user_id, order_id, gateway_payment_id, timestamps)
- **payments**: Chỉ lưu thông tin đặc thù về payment gateway (payment_method, payment_gateway, gateway_response) + transaction_id FK (NOT NULL)
- **cashouts**: Chỉ lưu thông tin đặc thù về cashout (bank_account_id, reference_type, reference_id, processed_by) + transaction_id FK (NOT NULL)

### Các trường đã XÓA:
#### Từ `payments`:
- ❌ `amount` - Đã chuyển sang `transactions.amount`
- ❌ `status` - Đã chuyển sang `transactions.status`
- ❌ `gateway_payment_id` - Đã chuyển sang `transactions.gateway_payment_id`

#### Từ `cashouts`:
- ❌ `user_id` - Lấy từ `cashouts.Transaction.UserId`
- ❌ `amount` - Lấy từ `cashouts.Transaction.Amount`
- ❌ `status` - Lấy từ `cashouts.Transaction.Status`

### Các trường đã THÊM:
- ✅ `payments.transaction_id` (NOT NULL FK)
- ✅ `transactions.order_id` (NULL FK - vì không phải transaction nào cũng có order)
- ✅ `transactions.status` enum thêm giá trị 'pending'

### Các trường đã ĐỔI TRẠNG THÁI:
- ✅ `cashouts.transaction_id`: NULL → NOT NULL
- ✅ `cashouts.reference_type`: NULL → NOT NULL

---

## 2. CÁC FILE ĐÃ CÁP NHẬT TRONG DAL/DATA

### ✅ **DAL/Data/Models/Payment.cs**
**Đã xóa:**
```csharp
public decimal Amount { get; set; }
public PaymentStatus Status { get; set; }
public string? GatewayPaymentId { get; set; }
```

**Đã thêm:**
```csharp
public ulong TransactionId { get; set; }
public virtual Transaction Transaction { get; set; } = null!;
```

---

### ✅ **DAL/Data/Configurations/PaymentConfiguration.cs**
**Đã xóa:**
- Cấu hình cho `Amount`, `Status`, `GatewayPaymentId`
- Index `idx_gateway_payment`, `idx_status`
- Default value cho `Status`

**Đã thêm:**
```csharp
builder.Property(e => e.TransactionId)
    .HasColumnName("transaction_id")
    .HasColumnType("bigint unsigned")
    .IsRequired();

builder.HasOne(e => e.Transaction)
    .WithMany(t => t.Payments)
    .HasForeignKey(e => e.TransactionId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();

builder.HasIndex(e => e.TransactionId).HasDatabaseName("idx_transaction");
```

---

### ✅ **DAL/Data/Models/Transaction.cs**
**Đã thêm:**
```csharp
public ulong? OrderId { get; set; }
public virtual Order? Order { get; set; }
public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
```

**Đã sửa default status:**
```csharp
// Cũ: Status = TransactionStatus.Completed
// Mới: Status = TransactionStatus.Pending
public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
```

---

### ✅ **DAL/Data/Configurations/TransactionConfiguration.cs**
**Đã thêm:**
```csharp
builder.Property(e => e.OrderId)
    .HasColumnName("order_id")
    .HasColumnType("bigint unsigned");

builder.Property(e => e.Status)
    .HasColumnType("enum('pending','completed','failed','cancelled')")  // Thêm 'pending'
    .HasDefaultValue(TransactionStatus.Pending);  // Đổi default

builder.HasOne(e => e.Order)
    .WithMany(o => o.Transactions)
    .HasForeignKey(e => e.OrderId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasIndex(e => e.OrderId).HasDatabaseName("idx_order");
```

---

### ✅ **DAL/Data/Models/Cashout.cs**
**Đã xóa:**
```csharp
public ulong UserId { get; set; }
public decimal Amount { get; set; }
public CashoutStatus Status { get; set; }
public virtual User User { get; set; } = null!;
```

**Đã sửa:**
```csharp
// Cũ: public ulong? TransactionId { get; set; }
// Mới: public ulong TransactionId { get; set; }  (required)

// Cũ: public CashoutReferenceType? ReferenceType { get; set; }
// Mới: public CashoutReferenceType ReferenceType { get; set; }  (required)

// Cũ: public virtual Transaction? Transaction { get; set; }
// Mới: public virtual Transaction Transaction { get; set; } = null!;
```

---

### ✅ **DAL/Data/Configurations/CashoutConfiguration.cs**
**Đã xóa:**
- Cấu hình cho `UserId`, `Amount`, `Status`
- FK relationship với `User`
- Index `idx_user`, `idx_status`, `idx_processed`

**Đã sửa:**
```csharp
builder.Property(e => e.TransactionId)
    .IsRequired();  // Thêm IsRequired()

builder.Property(e => e.ReferenceType)
    .IsRequired();  // Thêm IsRequired()
    
builder.HasOne(e => e.Transaction)
    .WithMany(t => t.Cashouts)
    .HasForeignKey(e => e.TransactionId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();  // Thêm IsRequired()
```

**Đã thêm index mới:**
```csharp
builder.HasIndex(e => e.BankAccountId).HasDatabaseName("idx_bank_account");
builder.HasIndex(e => e.ProcessedBy).HasDatabaseName("idx_processed_by");
```

---

## 3. CÁC FILE CẦN NGƯỜI DÙNG TỰ SỬA (BUSINESS LOGIC)

### 🔴 **QUAN TRỌNG - BLL/Services/Payment/PayOSService.cs**

#### ❌ Lỗi 1: Lines 83-84, 94 - Tạo Payment với Amount và Status
**Vị trí:**
```csharp
var payment = new PaymentResponseDTO
{
    OrderId = order.Id,
    PaymentMethod = PaymentMethod.Payos,
    PaymentGateway = PaymentGateway.Payos,
    GatewayPaymentId = createdPayment.orderCode.ToString(),  // ❌ Không còn tồn tại trong Payment
    Amount = createdPayment.amount,  // ❌ Không còn tồn tại trong Payment
    Status = PaymentStatus.Pending,  // ❌ Không còn tồn tại trong Payment
    GatewayResponse = new Dictionary<string, object>
    {
        ...
        { "status", createdPayment.status },  // ❌ Line 94
    }
};
await _paymentRepository.CreatePaymentWithTransactionAsync(_mapper.Map<DAL.Data.Models.Payment>(payment));
```

**Hướng sửa:**
```csharp
// Bước 1: Tạo Transaction trước
var transaction = new Transaction
{
    TransactionType = TransactionType.PaymentIn,
    Amount = createdPayment.amount,
    Currency = "VND",
    UserId = order.UserId,
    OrderId = orderId,
    Status = TransactionStatus.Pending,
    GatewayPaymentId = createdPayment.orderCode.ToString(),
    Note = $"Payment for order {orderId}",
    CreatedBy = order.UserId
};
var createdTransaction = await _transactionRepository.CreateTransactionAsync(transaction);

// Bước 2: Tạo Payment với TransactionId
var payment = new PaymentResponseDTO
{
    OrderId = order.Id,
    TransactionId = createdTransaction.Id,  // ✅ Thêm mới
    PaymentMethod = PaymentMethod.Payos,
    PaymentGateway = PaymentGateway.Payos,
    // ❌ Xóa: GatewayPaymentId, Amount, Status
    GatewayResponse = new Dictionary<string, object>
    {
        { "bin", createdPayment.bin },
        { "accountNumber", createdPayment.accountNumber },
        { "amount", createdPayment.amount },  // Vẫn lưu trong gateway response
        { "description", createdPayment.description },
        { "orderCode", createdPayment.orderCode },
        { "currency", createdPayment.currency },
        { "paymentLinkId", createdPayment.paymentLinkId },
        { "status", createdPayment.status },  // Vẫn lưu trong gateway response
        { "expiredAt", createdPayment.expiredAt ?? 0 },
        { "checkoutUrl", createdPayment.checkoutUrl },
    }
};
await _paymentRepository.CreatePaymentAsync(payment);
```

**LƯU Ý:** Cần cập nhật `CreatePaymentWithTransactionAsync` thành logic mới hoặc tạo method riêng.

---

#### ❌ Lỗi 2: Line 108-114 - Tìm Payment bằng GatewayPaymentId
**Vị trí:**
```csharp
var payment = await _paymentRepository.GetPaymentByGatewayPaymentIdAsync(
    webhookData.orderCode.ToString(), cancellationToken);
if (payment == null)
    throw new KeyNotFoundException($"Không tìm thấy thanh toán với mã đơn hàng: {webhookData.orderCode}");

if (webhookData.code == "00" || webhookData.desc == "Thành công")
{
    payment.Status = PaymentStatus.Completed;  // ❌ Payment không còn có Status
    payment.Order.Status = OrderStatus.Paid;
```

**Hướng sửa:**
```csharp
// Tìm Transaction thay vì Payment (vì gateway_payment_id nằm ở transactions)
var transaction = await _transactionRepository.GetTransactionByGatewayPaymentIdAsync(
    webhookData.orderCode.ToString(), cancellationToken);
if (transaction == null)
    throw new KeyNotFoundException($"Không tìm thấy giao dịch với mã đơn hàng: {webhookData.orderCode}");

// Lấy Payment từ Transaction
var payment = await _paymentRepository.GetPaymentByTransactionIdAsync(
    transaction.Id, cancellationToken);
if (payment == null)
    throw new KeyNotFoundException($"Không tìm thấy payment cho transaction {transaction.Id}");

if (webhookData.code == "00" || webhookData.desc == "Thành công")
{
    transaction.Status = TransactionStatus.Completed;  // ✅ Sửa transaction status
    transaction.CompletedAt = DateTime.UtcNow;
    await _transactionRepository.UpdateTransactionAsync(transaction, cancellationToken);
    
    payment.Order.Status = OrderStatus.Paid;
    payment.Order.UpdatedAt = DateTime.UtcNow;
    await _orderRepository.UpdateAsync(payment.Order, cancellationToken);
}
```

**LƯU Ý:** Cần tạo method mới:
- `ITransactionRepository.GetTransactionByGatewayPaymentIdAsync(string, CancellationToken)`
- `IPaymentRepository.GetPaymentByTransactionIdAsync(ulong, CancellationToken)`

---

#### ❌ Lỗi 3: Line 114 onwards - Webhook transaction creation
**Vị trí (tiếp từ Lỗi 2):**
```csharp
var transaction = new TransactionCreateDTO
{
    TransactionType = TransactionType.PaymentIn,
    Amount = webhookData.amount,  // ✅ OK - Transaction vẫn có Amount
    Currency = webhookData.currency,
    ...
}
```

**Hướng sửa:**
Transaction đã được tạo lúc CreatePaymentLink, bây giờ chỉ cần UPDATE status.
```csharp
// Không cần tạo transaction mới, chỉ update status
transaction.Status = TransactionStatus.Completed;
transaction.CompletedAt = DateTime.UtcNow;
transaction.Note = $"Payment webhook confirmed: {webhookData.desc}";
await _transactionRepository.UpdateTransactionAsync(transaction, cancellationToken);
```

---

### 🔴 **QUAN TRỌNG - BLL/Services/WalletService.cs**

#### ❌ Lỗi 1: Line 73 - Tạo Cashout với UserId và Status
**Vị trí:**
```csharp
var cashout = _mapper.Map<Cashout>(dto);
cashout.UserId = userId;  // ❌ Cashout không còn có UserId
cashout.Status = CashoutStatus.Processing;  // ❌ Cashout không còn có Status
cashout.ReferenceType = CashoutReferenceType.VendorWithdrawal;
cashout.ReferenceId = wallet.Id;
await _cashoutRepository.CreateWalletCashoutAsync(cashout, cancellationToken);
```

**Hướng sửa:**
```csharp
// Bước 1: Tạo Transaction trước
var transaction = new Transaction
{
    TransactionType = TransactionType.WalletCashout,
    Amount = dto.Amount,
    Currency = "VND",
    UserId = userId,  // ✅ UserId nằm trong Transaction
    Status = TransactionStatus.Pending,  // ✅ Status nằm trong Transaction
    Note = $"Yêu cầu rút tiền từ ví người bán",
    CreatedBy = userId
};
var createdTransaction = await _transactionRepository.CreateTransactionAsync(transaction, cancellationToken);

// Bước 2: Tạo Cashout với TransactionId
var cashout = _mapper.Map<Cashout>(dto);
cashout.TransactionId = createdTransaction.Id;  // ✅ Required
// ❌ Xóa: cashout.UserId, cashout.Amount, cashout.Status
cashout.ReferenceType = CashoutReferenceType.VendorWithdrawal;  // ✅ Required
cashout.ReferenceId = wallet.Id;
await _cashoutRepository.CreateWalletCashoutAsync(cashout, cancellationToken);
```

---

#### ❌ Lỗi 2: Lines 89-96 - Kiểm tra và cập nhật Cashout Status
**Vị trí:**
```csharp
if(dto.Status == CashoutStatus.Processing)  // ❌ Nên check Transaction.Status
    throw new InvalidOperationException("Trạng thái không thể là 'processing' khi xử lý yêu cầu rút tiền.");

walletCashout.Status = dto.Status;  // ❌ Cashout không còn có Status
walletCashout.ProcessedBy = staffId;
walletCashout.ProcessedAt = DateTime.UtcNow;
Cashout c;

if (dto.Status == CashoutStatus.Completed)  // ❌ Nên check Transaction.Status
```

**Hướng sửa:**
```csharp
// Lấy Transaction của Cashout
var transaction = await _transactionRepository.GetAsync(
    t => t.Id == walletCashout.TransactionId, cancellationToken);

if(dto.Status == CashoutStatus.Processing)
    throw new InvalidOperationException("Trạng thái không thể là 'processing' khi xử lý yêu cầu rút tiền.");

// Cập nhật Transaction Status thay vì Cashout Status
transaction.Status = dto.Status == CashoutStatus.Completed 
    ? TransactionStatus.Completed 
    : TransactionStatus.Failed;
transaction.ProcessedBy = staffId;
transaction.CompletedAt = dto.Status == CashoutStatus.Completed ? DateTime.UtcNow : null;

walletCashout.ProcessedBy = staffId;
walletCashout.ProcessedAt = DateTime.UtcNow;
Cashout c;

if (dto.Status == CashoutStatus.Completed)
```

**LƯU Ý:** `WalletProcessCreateDTO.Status` có thể cần đổi thành `TransactionStatus` thay vì `CashoutStatus`.

---

#### ❌ Lỗi 3: Lines 104, 114, 132, 146, 153, 161, 175, 187 - Truy cập walletCashout.Amount
**Vị trí (nhiều chỗ):**
```csharp
Amount = walletCashout.Amount,  // ❌ Line 104
wallet.Balance -= walletCashout.Amount;  // ❌ Line 114
$"Số tiền {walletCashout.Amount:N0} VNĐ..."  // ❌ Line 132
if(await _payOSApiClient.GetBalanceAsync(cancellationToken) < (int)Math.Ceiling(walletCashout.Amount))  // ❌ Line 146
(int)Math.Ceiling(walletCashout.Amount),  // ❌ Line 153
Amount = walletCashout.Amount,  // ❌ Line 161
wallet.Balance -= walletCashout.Amount;  // ❌ Line 175
$"Số tiền {walletCashout.Amount:N0} VNĐ..."  // ❌ Line 187
```

**Hướng sửa (tất cả các chỗ):**
```csharp
// Thay tất cả walletCashout.Amount thành walletCashout.Transaction.Amount
Amount = walletCashout.Transaction.Amount,
wallet.Balance -= walletCashout.Transaction.Amount;
$"Số tiền {walletCashout.Transaction.Amount:N0} VNĐ..."
if(await _payOSApiClient.GetBalanceAsync(cancellationToken) < (int)Math.Ceiling(walletCashout.Transaction.Amount))
(int)Math.Ceiling(walletCashout.Transaction.Amount),
Amount = walletCashout.Transaction.Amount,
wallet.Balance -= walletCashout.Transaction.Amount;
$"Số tiền {walletCashout.Transaction.Amount:N0} VNĐ..."
```

**LƯU Ý:** Cần đảm bảo khi query `walletCashout`, phải `.Include(c => c.Transaction)` để tránh null reference.

---

#### ❌ Lỗi 4: Line 171 - Cập nhật walletCashout.Status
**Vị trí:**
```csharp
walletCashout.Status = CashoutStatus.Completed;  // ❌ Cashout không còn có Status
```

**Hướng sửa:**
```csharp
// Cập nhật Transaction Status thay vì Cashout Status
walletCashout.Transaction.Status = TransactionStatus.Completed;
walletCashout.Transaction.CompletedAt = DateTime.UtcNow;
await _transactionRepository.UpdateAsync(walletCashout.Transaction, cancellationToken);
```

---

### 🟠 **DAL/Repository/CashoutRepository.cs**

#### ❌ Lỗi 1: Line 39 - GetWalletCashoutRequestByUserIdAsync
**Vị trí:**
```csharp
var existing = await _walletRepository.GetWalletCashoutRequestByUserIdAsync(
    cashout.UserId, cancellationToken);  // ❌ cashout.UserId không còn tồn tại
```

**Hướng sửa:**
```csharp
// Lấy UserId từ Transaction
var transaction = await _transactionRepository.GetAsync(
    t => t.Id == cashout.TransactionId, cancellationToken);
if (transaction == null)
    throw new InvalidOperationException("Transaction không tồn tại");

var existing = await _walletRepository.GetWalletCashoutRequestByUserIdAsync(
    transaction.UserId, cancellationToken);
```

**HOẶC:** Truyền `userId` như một parameter riêng vào method `CreateWalletCashoutAsync`.

---

#### ❌ Lỗi 2: Lines 79-82 - Cập nhật Payment.Status
**Vị trí:**
```csharp
var payment = await _paymentRepository.GetAsync(u => u.OrderId == order.Id, true, cancellationToken) ?? 
    throw new KeyNotFoundException("Không tìm thấy thanh toán liên quan đến đơn hàng.");
if (payment.Status != PaymentStatus.Completed)  // ❌ Payment không còn có Status
    throw new InvalidOperationException("Chỉ có thể hoàn tiền cho các thanh toán đã hoàn tất.");
payment.UpdatedAt = DateTime.UtcNow;
payment.Status = PaymentStatus.Refunded;  // ❌ Payment không còn có Status
await _paymentRepository.UpdateAsync(payment, cancellationToken);
```

**Hướng sửa:**
```csharp
var payment = await _paymentRepository.GetAsync(
    u => u.OrderId == order.Id, 
    includeFunc: q => q.Include(p => p.Transaction),  // ✅ Include Transaction
    cancellationToken: cancellationToken) 
    ?? throw new KeyNotFoundException("Không tìm thấy thanh toán liên quan đến đơn hàng.");

// Kiểm tra Transaction Status thay vì Payment Status
if (payment.Transaction.Status != TransactionStatus.Completed)
    throw new InvalidOperationException("Chỉ có thể hoàn tiền cho các thanh toán đã hoàn tất.");

// Cập nhật Transaction Status
payment.Transaction.Status = TransactionStatus.Refunded;  // ❌ TransactionStatus không có Refunded!
// ⚠️ CẦN THÊM enum value 'refunded' vào TransactionStatus hoặc dùng 'cancelled'
payment.Transaction.UpdatedAt = DateTime.UtcNow;
await _transactionRepository.UpdateAsync(payment.Transaction, cancellationToken);
```

**LƯU Ý QUAN TRỌNG:** 
- Schema v10 không có `TransactionStatus.Refunded`
- Cần quyết định: Thêm enum value 'refunded' HOẶC dùng 'cancelled' HOẶC dùng 'failed'
- Nếu thêm 'refunded', cần update schema SQL và TransactionConfiguration.cs

---

### 🟠 **DAL/Repository/PaymentRepository.cs**

#### ❌ Lỗi: Line 63-69 - GetPaymentByGatewayPaymentIdAsync
**Vị trí:**
```csharp
public async Task<Payment?> GetPaymentByGatewayPaymentIdAsync(string paymentGatewayId, CancellationToken cancellationToken = default)
{
    return await _paymentRepository.GetWithRelationsAsync(
        filter: p => p.GatewayPaymentId == paymentGatewayId,  // ❌ Payment không còn có GatewayPaymentId
        useNoTracking: true, includeFunc: q => q.Include(p => p.Order),
        cancellationToken: cancellationToken);
}
```

**Hướng sửa:**
```csharp
// CÁCH 1: Xóa method này, tìm qua Transaction thay thế
// public async Task<Payment?> GetPaymentByGatewayPaymentIdAsync(...) - XÓA

// CÁCH 2: Tìm Payment qua Transaction.GatewayPaymentId
public async Task<Payment?> GetPaymentByGatewayPaymentIdAsync(string paymentGatewayId, CancellationToken cancellationToken = default)
{
    return await _paymentRepository.GetWithRelationsAsync(
        filter: p => p.Transaction.GatewayPaymentId == paymentGatewayId,
        useNoTracking: true, 
        includeFunc: q => q.Include(p => p.Order).Include(p => p.Transaction),
        cancellationToken: cancellationToken);
}

// CÁCH 3 (TỐT NHẤT): Tạo method mới trong TransactionRepository
// ITransactionRepository:
public async Task<Transaction?> GetTransactionByGatewayPaymentIdAsync(string gatewayPaymentId, CancellationToken cancellationToken = default)
{
    return await _transactionRepository.GetWithRelationsAsync(
        filter: t => t.GatewayPaymentId == gatewayPaymentId,
        useNoTracking: true,
        includeFunc: q => q.Include(t => t.Payments).ThenInclude(p => p.Order),
        cancellationToken: cancellationToken);
}
```

**KHUYẾN NGHỊ:** Dùng CÁCH 3, tìm Transaction trước rồi lấy Payment từ navigation property.

---

### 🟠 **DAL/Repository/WalletRepository.cs**

#### ❌ Lỗi: Lines 139, 144 - Tìm Cashout bằng Status
**Vị trí:**
```csharp
await _cashoutRepository.GetAsync(
    c => c.UserId == vendorId && c.Status == CashoutStatus.Processing  // ❌ Cashout không còn có UserId và Status
    ...
await _cashoutRepository.GetWithRelationsAsync(
    c => c.UserId == vendorId && c.Status == CashoutStatus.Processing  // ❌ Cashout không còn có UserId và Status
```

**Hướng sửa:**
```csharp
// Tìm qua Transaction
await _cashoutRepository.GetAsync(
    c => c.Transaction.UserId == vendorId && c.Transaction.Status == TransactionStatus.Pending,
    includeFunc: q => q.Include(c => c.Transaction),
    cancellationToken: cancellationToken);

await _cashoutRepository.GetWithRelationsAsync(
    c => c.Transaction.UserId == vendorId && c.Transaction.Status == TransactionStatus.Pending,
    includeFunc: q => q.Include(c => c.Transaction).Include(c => c.BankAccount)...,
    cancellationToken: cancellationToken);
```

**LƯU Ý:** 
- `CashoutStatus.Processing` mapping sang `TransactionStatus.Pending`
- Luôn phải `.Include(c => c.Transaction)` khi query Cashout

---

### 🟠 **DAL/Repository/DashboardRepository.cs**

#### ❌ Lỗi: Line 41 - Tìm Payment bằng Status
**Vị trí:**
```csharp
.Where(p => p.Status == PaymentStatus.Completed && ...  // ❌ Payment không còn có Status
```

**Hướng sửa:**
```csharp
.Where(p => p.Transaction.Status == TransactionStatus.Completed && ...
// Cần thêm .Include(p => p.Transaction) vào query trước đó
```

---

### 🔵 **BLL/DTO Files - CẦN REVIEW**

#### BLL/DTO/Payment/PayOS/PaymentResponseDTO.cs
**Hiện tại:**
```csharp
public decimal Amount { get; set; }  // ❌ Không còn map được với Payment.Amount
public PaymentStatus Status { get; set; } = PaymentStatus.Pending;  // ❌ Không còn map được với Payment.Status
```

**Hướng sửa:**
```csharp
// CÁCH 1: Xóa 2 properties này, lấy từ Transaction
// CÁCH 2: Rename thành TransactionAmount, TransactionStatus và map từ Payment.Transaction
public decimal TransactionAmount { get; set; }  // Map từ Payment.Transaction.Amount
public TransactionStatus TransactionStatus { get; set; }  // Map từ Payment.Transaction.Status

// CÁCH 3 (TỐT NHẤT): Thêm nested DTO
public TransactionResponseDTO Transaction { get; set; }
```

---

#### BLL/DTO/Wallet/WalletCashoutRequestCreateDTO.cs
**Hiện tại:**
```csharp
public decimal Amount { get; set; }  // ✅ OK - Dùng để tạo Transaction.Amount
// public CashoutStatus Status { get; set; } = CashoutStatus.Pending;  // Đã comment - OK
```
**Hành động:** ✅ Không cần sửa (Amount dùng cho Transaction)

---

#### BLL/DTO/Wallet/WalletProcessCreateDTO.cs
**Hiện tại:**
```csharp
public CashoutStatus Status { get; set; }  // ⚠️ Nên đổi thành TransactionStatus?
```

**Hướng sửa:**
```csharp
// CÁCH 1: Đổi sang TransactionStatus
public TransactionStatus Status { get; set; }

// CÁCH 2: Giữ CashoutStatus nhưng map sang TransactionStatus trong service
// CashoutStatus.Processing → TransactionStatus.Pending
// CashoutStatus.Completed → TransactionStatus.Completed
// CashoutStatus.Failed → TransactionStatus.Failed
// CashoutStatus.Cancelled → TransactionStatus.Cancelled
```

**KHUYẾN NGHỊ:** Dùng CÁCH 2 để giữ nguyên API contract.

---

#### BLL/DTO/Wallet/WalletCashoutResponseDTO.cs
**Hiện tại:**
```csharp
// public decimal Amount { get; set; }  // Đã comment - OK
public CashoutStatus Status { get; set; } = CashoutStatus.Processing;  // ⚠️ Không map được với Cashout.Status

// Line 49 - Nested class
public decimal Amount { get; set; }  // ⚠️ Cần map từ Transaction.Amount
```

**Hướng sửa:**
```csharp
// Map từ Cashout.Transaction.Status thông qua AutoMapper
public CashoutStatus Status { get; set; }  // Map: Cashout.Transaction.Status → CashoutStatus

// Line 49
public decimal Amount { get; set; }  // Map từ Transaction.Amount
```

**AutoMapper Profile cần thêm:**
```csharp
CreateMap<Cashout, WalletCashoutResponseDTO>()
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 
        src.Transaction.Status == TransactionStatus.Pending ? CashoutStatus.Processing :
        src.Transaction.Status == TransactionStatus.Completed ? CashoutStatus.Completed :
        src.Transaction.Status == TransactionStatus.Failed ? CashoutStatus.Failed :
        CashoutStatus.Cancelled))
    .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Transaction.Amount));
```

---

#### BLL/DTO/Wallet/WalletCashoutRequestResponseDTO.cs
**Hiện tại:**
```csharp
public decimal Amount { get; set; }  // ⚠️ Cần map từ Transaction.Amount
public CashoutStatus Status { get; set; } = CashoutStatus.Processing;  // ⚠️ Cần map từ Transaction.Status
```

**Hướng sửa:** Giống như `WalletCashoutResponseDTO` ở trên.

---

## 4. CÁC METHOD MỚI CẦN TẠO

### 🔧 **ITransactionRepository & TransactionRepository**

```csharp
// Interface
Task<Transaction?> GetTransactionByGatewayPaymentIdAsync(string gatewayPaymentId, CancellationToken cancellationToken = default);
Task<Transaction> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
Task<Transaction> UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);

// Implementation
public async Task<Transaction?> GetTransactionByGatewayPaymentIdAsync(string gatewayPaymentId, CancellationToken cancellationToken = default)
{
    return await _transactionRepository.GetWithRelationsAsync(
        filter: t => t.GatewayPaymentId == gatewayPaymentId,
        useNoTracking: true,
        includeFunc: q => q.Include(t => t.Payments).ThenInclude(p => p.Order)
                          .Include(t => t.User),
        cancellationToken: cancellationToken);
}

public async Task<Transaction> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
{
    transaction.CreatedAt = DateTime.UtcNow;
    transaction.UpdatedAt = DateTime.UtcNow;
    return await _transactionRepository.CreateAsync(transaction, cancellationToken);
}

public async Task<Transaction> UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
{
    transaction.UpdatedAt = DateTime.UtcNow;
    return await _transactionRepository.UpdateAsync(transaction, cancellationToken);
}
```

---

### 🔧 **IPaymentRepository & PaymentRepository**

```csharp
// Interface
Task<Payment?> GetPaymentByTransactionIdAsync(ulong transactionId, CancellationToken cancellationToken = default);

// Implementation
public async Task<Payment?> GetPaymentByTransactionIdAsync(ulong transactionId, CancellationToken cancellationToken = default)
{
    return await _paymentRepository.GetWithRelationsAsync(
        filter: p => p.TransactionId == transactionId,
        useNoTracking: true,
        includeFunc: q => q.Include(p => p.Order).Include(p => p.Transaction),
        cancellationToken: cancellationToken);
}
```

---

### 🔧 **Cập nhật CreatePaymentWithTransactionAsync**

**Cũ:**
```csharp
public async Task<Payment> CreatePaymentWithTransactionAsync(Payment payment, CancellationToken cancellationToken = default)
{
    // Chỉ tạo Payment
}
```

**Mới:**
```csharp
public async Task<Payment> CreatePaymentWithTransactionAsync(
    Payment payment, 
    Transaction transaction,  // ✅ Thêm parameter
    CancellationToken cancellationToken = default)
{
    await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    try
    {
        // Tạo Transaction trước
        transaction.CreatedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;
        var createdTransaction = await _transactionRepository.CreateAsync(transaction, cancellationToken);
        
        // Tạo Payment với TransactionId
        payment.TransactionId = createdTransaction.Id;
        payment.CreatedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        var createdPayment = await _paymentRepository.CreateAsync(payment, cancellationToken);
        
        await tx.CommitAsync(cancellationToken);
        return createdPayment;
    }
    catch
    {
        await tx.RollbackAsync(cancellationToken);
        throw;
    }
}
```

---

## 5. MAPPING STATUS GIỮA CASHOUT VÀ TRANSACTION

Vì DTO và frontend vẫn dùng `CashoutStatus`, cần mapping 2 chiều:

### CashoutStatus → TransactionStatus
```csharp
CashoutStatus.Processing → TransactionStatus.Pending
CashoutStatus.Completed → TransactionStatus.Completed
CashoutStatus.Failed → TransactionStatus.Failed
CashoutStatus.Cancelled → TransactionStatus.Cancelled
```

### TransactionStatus → CashoutStatus
```csharp
TransactionStatus.Pending → CashoutStatus.Processing
TransactionStatus.Completed → CashoutStatus.Completed
TransactionStatus.Failed → CashoutStatus.Failed
TransactionStatus.Cancelled → CashoutStatus.Cancelled
```

**⚠️ LƯU Ý:** `PaymentStatus.Refunded` KHÔNG TỒN TẠI trong `TransactionStatus` schema v10!

**QUYẾT ĐỊNH CẦN LÀM:**
1. **Thêm `refunded` vào TransactionStatus enum** (Khuyến nghị)
   - Update SQL schema: `enum('pending','completed','failed','cancelled','refunded')`
   - Update `TransactionConfiguration.cs`
   - Update `TransactionStatus` enum trong Models

2. **HOẶC dùng `TransactionStatus.Cancelled` cho refund**
   - Không cần sửa schema
   - Cần document rõ: "cancelled" bao gồm cả "refunded"

---

## 6. CHECKLIST TESTING

### ✅ Các workflow cần test kỹ:

1. **Tạo Payment qua PayOS**
   - [ ] Tạo Transaction trước với Status = Pending
   - [ ] Tạo Payment với TransactionId
   - [ ] Không còn lưu Amount, Status, GatewayPaymentId trong Payment
   - [ ] GatewayPaymentId chỉ lưu trong Transaction

2. **Webhook PayOS callback**
   - [ ] Tìm Transaction bằng GatewayPaymentId (không phải Payment)
   - [ ] Update Transaction.Status = Completed
   - [ ] Update Order.Status = Paid
   - [ ] Payment không bị update Status (vì không còn field này)

3. **Tạo Wallet Cashout Request**
   - [ ] Tạo Transaction trước với Status = Pending
   - [ ] Tạo Cashout với TransactionId (required)
   - [ ] Không còn lưu UserId, Amount, Status trong Cashout
   - [ ] ReferenceType là required (không được null)

4. **Xử lý Cashout Request (Manual)**
   - [ ] Lấy Amount từ Cashout.Transaction.Amount
   - [ ] Cập nhật Transaction.Status (không phải Cashout.Status)
   - [ ] Nếu Completed: Trừ Wallet.Balance bằng Transaction.Amount

5. **Xử lý Cashout Request (PayOS)**
   - [ ] Gọi PayOS API với Cashout.Transaction.Amount
   - [ ] Lưu PayOS response ID vào Transaction.GatewayPaymentId
   - [ ] Update Transaction.Status = Completed

6. **Refund Process**
   - [ ] Tìm Payment → lấy Transaction
   - [ ] Check Transaction.Status == Completed
   - [ ] Update Transaction.Status = Refunded (⚠️ Cần thêm enum!)
   - [ ] Tạo Transaction mới cho refund cashout
   - [ ] Tạo Cashout với TransactionId của refund transaction

7. **Query Dashboard/Reports**
   - [ ] Tất cả query Payment phải `.Include(p => p.Transaction)`
   - [ ] Tất cả query Cashout phải `.Include(c => c.Transaction)`
   - [ ] Filter theo Status: dùng `p.Transaction.Status` thay vì `p.Status`

8. **AutoMapper Profiles**
   - [ ] Payment → PaymentResponseDTO: Map Transaction.Amount, Transaction.Status
   - [ ] Cashout → WalletCashoutResponseDTO: Map Transaction.Amount, Transaction.Status
   - [ ] Tất cả map có liên quan phải Include Transaction

---

## 7. TÓM TẮT NGUYÊN TẮC

### ✅ QUY TẮC BẮT BUỘC:

1. **Luôn tạo Transaction TRƯỚC khi tạo Payment/Cashout**
   ```csharp
   var transaction = await _transactionRepository.CreateAsync(...);
   payment.TransactionId = transaction.Id;  // hoặc cashout.TransactionId
   ```

2. **Không bao giờ truy cập Payment.Amount, Payment.Status, Payment.GatewayPaymentId**
   ```csharp
   ❌ payment.Amount
   ✅ payment.Transaction.Amount
   
   ❌ payment.Status
   ✅ payment.Transaction.Status
   
   ❌ payment.GatewayPaymentId
   ✅ payment.Transaction.GatewayPaymentId
   ```

3. **Không bao giờ truy cập Cashout.UserId, Cashout.Amount, Cashout.Status**
   ```csharp
   ❌ cashout.UserId
   ✅ cashout.Transaction.UserId
   
   ❌ cashout.Amount
   ✅ cashout.Transaction.Amount
   
   ❌ cashout.Status
   ✅ cashout.Transaction.Status
   ```

4. **Luôn .Include(Transaction) khi query Payment hoặc Cashout**
   ```csharp
   await _paymentRepository.GetWithRelationsAsync(
       filter: p => p.OrderId == orderId,
       includeFunc: q => q.Include(p => p.Transaction),  // ✅ BẮT BUỘC
       ...
   );
   
   await _cashoutRepository.GetAsync(
       filter: c => c.Id == cashoutId,
       includeFunc: q => q.Include(c => c.Transaction),  // ✅ BẮT BUỘC
       ...
   );
   ```

5. **Tìm Payment/Cashout bằng GatewayPaymentId → Phải tìm qua Transaction**
   ```csharp
   ❌ await _paymentRepository.GetPaymentByGatewayPaymentIdAsync(gatewayId);
   ✅ var transaction = await _transactionRepository.GetTransactionByGatewayPaymentIdAsync(gatewayId);
   ✅ var payment = transaction.Payments.FirstOrDefault();
   ```

6. **Update Status → Update Transaction.Status, không phải Payment/Cashout**
   ```csharp
   ❌ payment.Status = PaymentStatus.Completed;
   ✅ payment.Transaction.Status = TransactionStatus.Completed;
   
   ❌ cashout.Status = CashoutStatus.Completed;
   ✅ cashout.Transaction.Status = TransactionStatus.Completed;
   ```

---

## 8. PRIORITY EXECUTION ORDER

### 🔴 **HIGH PRIORITY - PHẢI SỬA TRƯỚC KHI BUILD**

1. ✅ **Cập nhật User model** - Thêm navigation properties
   - `public virtual ICollection<Transaction> TransactionsAsUser { get; set; }`
   - `public virtual ICollection<Transaction> TransactionsCreated { get; set; }`
   - `public virtual ICollection<Transaction> TransactionsProcessed { get; set; }`
   - ❌ Xóa `public virtual ICollection<Cashout> CashoutsAsUser { get; set; }`

2. ✅ **Cập nhật Order model** - Thêm navigation property
   - `public virtual ICollection<Transaction> Transactions { get; set; }`

3. ✅ **Quyết định về TransactionStatus.Refunded**
   - Nếu THÊM: Update schema SQL + TransactionConfiguration.cs + enum
   - Nếu KHÔNG: Document rõ dùng Cancelled thay thế

4. ✅ **Tạo các method mới trong Repository**
   - `ITransactionRepository.GetTransactionByGatewayPaymentIdAsync()`
   - `ITransactionRepository.CreateTransactionAsync()`
   - `ITransactionRepository.UpdateTransactionAsync()`
   - `IPaymentRepository.GetPaymentByTransactionIdAsync()`
   - Update `CreatePaymentWithTransactionAsync()` signature

### 🟠 **MEDIUM PRIORITY - SỬA THEO THỨ TỰ**

5. **PayOSService.cs - CreatePaymentLinkAsync**
   - Tạo Transaction trước
   - Cập nhật Payment creation logic
   - Update DTO mapping

6. **PayOSService.cs - HandlePayOSWebhookAsync**
   - Đổi từ tìm Payment sang tìm Transaction
   - Update Transaction.Status thay vì Payment.Status
   - Xóa logic tạo Transaction mới (vì đã tạo lúc CreatePaymentLink)

7. **WalletService.cs - CreateWalletCashoutRequestAsync**
   - Tạo Transaction trước
   - Cập nhật Cashout creation logic

8. **WalletService.cs - ProcessWalletCashoutRequestAsync**
   - Update tất cả `walletCashout.Amount` → `walletCashout.Transaction.Amount`
   - Update Transaction.Status thay vì Cashout.Status

9. **WalletService.cs - ProcessWalletCashoutRequestByPayOSAsync**
   - Update tất cả `walletCashout.Amount` → `walletCashout.Transaction.Amount`
   - Update Transaction.Status thay vì Cashout.Status

10. **CashoutRepository.cs**
    - Fix `CreateWalletCashoutAsync` - lấy UserId từ parameter hoặc Transaction
    - Fix `CreateRefundCashoutWithTransactionAsync` - update Transaction.Status

11. **PaymentRepository.cs**
    - Fix hoặc xóa `GetPaymentByGatewayPaymentIdAsync`

12. **WalletRepository.cs**
    - Fix query tìm Cashout theo UserId và Status

13. **DashboardRepository.cs**
    - Fix query Payment với Status

### 🟢 **LOW PRIORITY - POLISH & OPTIMIZE**

14. **AutoMapper Profiles**
    - Update Payment → PaymentResponseDTO
    - Update Cashout → WalletCashoutResponseDTO
    - Update Cashout → WalletCashoutRequestResponseDTO

15. **DTO Files**
    - Cập nhật comments/documentation
    - Review và cleanup các properties không dùng

16. **Testing & Validation**
    - Unit tests cho các Repository methods mới
    - Integration tests cho payment workflow
    - Integration tests cho cashout workflow
    - Test error handling khi Transaction null

---

## 9. POTENTIAL BREAKING CHANGES

### ⚠️ **API Response Changes**

Nếu API response đang trả về:
```json
{
  "payment": {
    "id": 123,
    "orderId": 456,
    "amount": 100000,  // ❌ Sẽ mất
    "status": "completed",  // ❌ Sẽ mất
    "gatewayPaymentId": "ABC123"  // ❌ Sẽ mất
  }
}
```

Cần đổi thành:
```json
{
  "payment": {
    "id": 123,
    "orderId": 456,
    "transactionId": 789,
    "transaction": {  // ✅ Nested object
      "amount": 100000,
      "status": "completed",
      "gatewayPaymentId": "ABC123"
    }
  }
}
```

**HOẶC flatten:**
```json
{
  "payment": {
    "id": 123,
    "orderId": 456,
    "transactionId": 789,
    "transactionAmount": 100000,  // ✅ Renamed
    "transactionStatus": "completed",  // ✅ Renamed
    "gatewayPaymentId": "ABC123"  // ✅ Từ transaction
  }
}
```

### ⚠️ **Frontend Impact**

Nếu frontend đang:
```typescript
// ❌ Không còn hoạt động
const amount = payment.amount;
const status = payment.status;
const gatewayId = payment.gatewayPaymentId;

// ✅ Phải đổi thành
const amount = payment.transaction.amount;
const status = payment.transaction.status;
const gatewayId = payment.transaction.gatewayPaymentId;

// HOẶC nếu dùng flatten approach
const amount = payment.transactionAmount;
const status = payment.transactionStatus;
```

**KHUYẾN NGHỊ:** 
- Tạo API versioning (v1 vs v2)
- Hoặc duy trì backward compatibility bằng AutoMapper custom resolver

---

## 10. ROLLBACK PLAN

Nếu gặp vấn đề không thể fix kịp:

1. **Revert DAL Models**
   - Restore từ git: `Payment.cs`, `Cashout.cs`, `Transaction.cs`
   - Restore configurations tương ứng

2. **Revert Database Schema**
   ```sql
   -- Rollback script
   ALTER TABLE payments 
       ADD COLUMN amount DECIMAL(12,2) NOT NULL,
       ADD COLUMN status ENUM('pending','completed','failed','refunded') DEFAULT 'pending',
       ADD COLUMN gateway_payment_id VARCHAR(255),
       MODIFY COLUMN transaction_id BIGINT UNSIGNED NULL;
   
   ALTER TABLE cashouts
       ADD COLUMN user_id BIGINT UNSIGNED NOT NULL,
       ADD COLUMN amount DECIMAL(12,2) NOT NULL,
       ADD COLUMN status ENUM('processing','completed','failed','cancelled') DEFAULT 'processing',
       MODIFY COLUMN transaction_id BIGINT UNSIGNED NULL,
       MODIFY COLUMN reference_type ENUM('vendor_withdrawal','refund','admin_adjustment') NULL;
   
   ALTER TABLE transactions
       DROP COLUMN order_id,
       MODIFY COLUMN status ENUM('completed','failed','cancelled') DEFAULT 'completed';
   ```

3. **Restore BLL Services**
   - Revert từ git commit trước khi refactor

---

## KẾT LUẬN

Schema v10 là một **breaking change lớn** nhưng mang lại:

### ✅ **Ưu điểm:**
- Data integrity cao hơn (FK required, không duplicate data)
- Single source of truth cho tất cả financial data
- Dễ audit và reconciliation
- Performance tốt hơn (ít duplicate data, index tốt hơn)
- Extensibility cao (dễ thêm transaction types mới)

### ⚠️ **Thách thức:**
- Cần refactor nhiều business logic
- Breaking changes cho API/Frontend
- Cần test kỹ tất cả payment workflows
- Migration data phức tạp nếu có data cũ

### 📋 **Next Steps:**
1. Review và approve báo cáo này
2. Quyết định về TransactionStatus.Refunded
3. Tạo các Repository methods mới (Priority 1-4)
4. Fix PayOSService.cs (Priority 5-6)
5. Fix WalletService.cs (Priority 7-9)
6. Fix các Repository còn lại (Priority 10-13)
7. Update AutoMapper và DTO (Priority 14-15)
8. Comprehensive testing (Priority 16)
9. Update API documentation
10. Coordinate với Frontend team về API changes

---

**Người tạo báo cáo:** GitHub Copilot (Claude Sonnet 4.5)  
**Ngày tạo:** 2024  
**Schema version:** v10  
**Base commit:** Latest DAL/Data changes
