namespace DAL.Data;

// =====================================================
// USER MANAGEMENT ENUMS
// =====================================================

public enum UserRole
{
    Customer,
    Staff,
    Vendor,
    Admin
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    Deleted
}

// =====================================================
// CERTIFICATE ENUMS
// =====================================================

public enum VendorCertificateStatus
{
    Pending,
    Verified,
    Rejected
}

public enum ProductCertificateStatus
{
    Pending,
    Verified,
    Rejected
}

public enum ProductRegistrationStatus
{
    Pending,
    Approved,
    Rejected
}

// =====================================================
// ENVIRONMENTAL DATA ENUMS
// =====================================================

public enum SoilType
{
    DatPhuSa,       // Đất phù sa
    DatDoBazan,     // Đất đỏ Bazan
    DatFeralit,     // Đất Feralit
    DatThit,        // Đất thịt
    DatSet,         // Đất sét
    DatCat          // Đất cát
}

// =====================================================
// AI CHATBOT ENUMS
// =====================================================

public enum MessageType
{
    User,
    Bot,
    System
}

// =====================================================
// COMMUNITY ENUMS
// =====================================================

public enum ForumPostStatus
{
    Visible,
    Hidden
}

public enum ForumCommentStatus
{
    Visible,
    Moderated,
    Deleted
}

// =====================================================
// REQUEST MANAGEMENT ENUMS
// =====================================================

public enum RequestType
{
    RefundRequest,
    PayoutRequest,
    SupportRequest,
    VendorRegister
}

public enum RequestStatus
{
    Pending,
    InReview,
    Approved,
    Rejected,
    Completed,
    Cancelled
}

public enum RequestPriority
{
    Low,
    Medium,
    High,
    Urgent
}

// =====================================================
// ORDER AND PAYMENT ENUMS
// =====================================================

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    Paypal,
    Stripe,
    BankTransfer,
    Cod
}

public enum PaymentGateway
{
    Stripe,
    Paypal,
    Vnpay,
    Momo,
    Manual
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded,
    PartiallyRefunded
}

// =====================================================
// FINANCIAL SYSTEM ENUMS
// =====================================================

public enum TransactionType
{
    PaymentIn,
    Cashout,
    WalletCredit,
    WalletDebit,
    Commission,
    Refund,
    Adjustment
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
}

public enum CashoutStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public enum CashoutType
{
    CommissionPayout,
    VendorPayment,
    Expense,
    Refund
}

// =====================================================
// INVENTORY ENUMS
// =====================================================

public enum MovementType
{
    Sale,
    Return,
    Damage,
    Loss,
    Adjustment
}

public enum QualityCheckStatus
{
    NotRequired,
    Pending,
    Passed,
    Failed
}

public enum ConditionOnArrival
{
    New,
    Good,
    Fair,
    Damaged
}


