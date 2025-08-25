namespace DAL.Data;

public enum UserRole
{
    Customer,
    Seller,
    Admin,
    Manager
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    Deleted
}

public enum SoilType
{
    DatPhuSa,       // Đất phù sa
    DatDoBazan,     // Đất đỏ Bazan
    DatFeralit,     // Đất Feralit
    DatThit,        // Đất thịt
    DatSet,         // Đất sét
    DatCat          // Đất cát
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

public enum SustainabilityCertificationCategory
{
    Organic,
    Environmental,
    FairTrade,
    FoodSafety,
    Social,
    Energy
}

public enum VendorSustainabilityCredentialStatus
{
    Pending,
    Verified,
    Rejected
}


