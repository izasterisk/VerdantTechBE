namespace DAL.Data;

public enum UserRole
{
    Customer,
    Farmer,
    Seller,
    Vendor,
    Admin,
    Expert,
    ContentManager
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    Deleted
}

public enum Language
{
    Vi,
    En
}

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

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded,
    PartiallyRefunded
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

public enum ReviewStatus
{
    Pending,
    Approved,
    Rejected
}

public enum MaterialType
{
    Guide,
    Tutorial,
    Research,
    CaseStudy,
    Infographic,
    Video
}

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}

public enum ContentStatus
{
    Draft,
    Published,
    Archived
}

public enum ForumPostStatus
{
    Published,
    Draft,
    Moderated,
    Deleted
}

public enum ForumCommentStatus
{
    Visible,
    Moderated,
    Deleted
}

public enum BlogStatus
{
    Draft,
    Published,
    Scheduled,
    Archived
}

public enum BlogCommentStatus
{
    Approved,
    Pending,
    Spam,
    Deleted
}

public enum InteractionType
{
    Like,
    Dislike,
    Helpful,
    Unhelpful
}

public enum TargetType
{
    ForumPost,
    ForumComment,
    BlogPost,
    BlogComment,
    ProductReview
}

public enum InventoryType
{
    In,
    Out,
    Adjustment
}

public enum MessageType
{
    User,
    Bot,
    System
}

public enum WeatherApiSource
{
    Openweathermap,
    Accuweather
}

public enum SettingType
{
    String,
    Number,
    Boolean,
    Json
}

public enum UserFeedback
{
    Helpful,
    NotHelpful,
    PartiallyHelpful
}
