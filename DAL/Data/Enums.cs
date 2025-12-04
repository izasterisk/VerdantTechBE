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

public enum PlantingMethod
{
    GieoHatTrucTiep,      // direct_seeding -> gieo_hat_truc_tiep
    UomTrongKhay,         // tray_nursery -> uom_trong_khay
    CayCayCon,            // transplanting -> cay_cay_con
    SinhSanSinhDuong,     // vegetative_propagation -> sinh_san_sinh_duong
    GiamCanh              // cutting -> giam_canh
}

public enum CropType
{
    RauAnLa,              // leafy_green -> rau_an_la
    RauAnQua,             // fruiting -> rau_an_qua
    RauCu,                // root_vegetable -> rau_cu
    RauThom               // herb -> rau_thom
}

public enum FarmingType
{
    ThamCanh,             // intensive -> tham_canh
    LuanCanh,             // crop_rotation -> luan_canh
    XenCanh,              // intercropping -> xen_canh
    NhaLuoi,              // greenhouse -> nha_luoi
    ThuyCanh              // hydroponics -> thuy_canh
}

public enum CropStatus
{
    Growing,              // growing
    Harvested,            // harvested
    Failed,               // failed
    Deleted               // deleted
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

public enum MediaOwnerType
{
    VendorCertificates,
    ChatbotMessages,
    Products,
    ProductRegistrations,
    ProductCertificates,
    ProductReviews,
    ForumPosts,
    RequestMessage
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


