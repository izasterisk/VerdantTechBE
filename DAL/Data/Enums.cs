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

public enum FarmProfileStatus
{
    Active,
    Maintenance,
    Deleted
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
    SupportRequest
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

// =====================================================
// ORDER AND PAYMENT ENUMS
// =====================================================

public enum OrderStatus
{
    Pending,
    Processing,
    Paid,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

public enum OrderPaymentMethod
{
    Banking,
    COD
}

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    Stripe,
    Cod,
    Payos
}

public enum PaymentGateway
{
    Stripe,
    Manual,
    Payos
}

// =====================================================
// FINANCIAL SYSTEM ENUMS
// =====================================================

public enum TransactionType
{
    PaymentIn,
    WalletCashout,
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

public enum CashoutReferenceType
{
    VendorWithdrawal,
    Refund,
    AdminAdjustment
}

// =====================================================
// INVENTORY ENUMS
// =====================================================

public enum MovementType
{
    Sale,
    ReturnToVendor,
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

public enum ProductSerialStatus
{
    Stock,
    Sold,
    Refund,
    Adjustment
}

// =====================================================
// MEDIA MANAGEMENT ENUMS (v8.1)
// =====================================================

public enum MediaOwnerType
{
    VendorCertificates,
    ChatbotMessages,
    Products,
    ProductRegistrations,
    ProductCertificates,
    ProductReviews,
    ForumPosts,
    Request
}

public enum MediaPurpose
{
    None,
    Front,
    Back,
    ProductCertificatePdf,
    VendorCertificatesPdf,
    ProductImage
}

public enum NotificationReferenceType
{
    Order,
    Payment,
    Request,
    ForumPost,
    ForumComment,
    ChatbotConversation,
    Refund,
    WalletCashout,
    ProductRegistration,
    EnvironmentalData
}


