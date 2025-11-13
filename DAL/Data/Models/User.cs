using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Base user authentication and profile table
/// </summary>
public partial class User
{
    public ulong Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    public UserRole Role { get; set; } = UserRole.Customer;

    [Required]
    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    public string? TaxCode { get; set; }

    public bool IsVerified { get; set; } = false;

    [StringLength(255)]
    public string? VerificationToken { get; set; }

    public DateTime? VerificationSentAt { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? LastLoginAt { get; set; }

    [StringLength(500)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
    public virtual ICollection<FarmProfile> FarmProfiles { get; set; } = new List<FarmProfile>();
    public virtual VendorProfile? VendorProfile { get; set; }
    public virtual Cart? Cart { get; set; }
    
    // One-to-many relationships
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
    public virtual ICollection<ForumComment> ForumComments { get; set; } = new List<ForumComment>();
    public virtual ICollection<ChatbotConversation> ChatbotConversations { get; set; } = new List<ChatbotConversation>();
    public virtual ICollection<EnvironmentalDatum> EnvironmentalDataAsCustomer { get; set; } = new List<EnvironmentalDatum>();
    public virtual ICollection<BatchInventory> BatchInventoriesQualityChecked { get; set; } = new List<BatchInventory>();
    public virtual ICollection<ExportInventory> ExportInventories { get; set; } = new List<ExportInventory>();
    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
    public virtual ICollection<Transaction> TransactionsCreated { get; set; } = new List<Transaction>();
    public virtual ICollection<Transaction> TransactionsProcessed { get; set; } = new List<Transaction>();
    public virtual ICollection<Transaction> TransactionsAsUser { get; set; } = new List<Transaction>();
    public virtual ICollection<Cashout> CashoutsProcessed { get; set; } = new List<Cashout>();
    public virtual ICollection<Cashout> CashoutsAsUser { get; set; } = new List<Cashout>();
    public virtual ICollection<UserBankAccount> UserBankAccounts { get; set; } = new List<UserBankAccount>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        
    // Vendor-related relationships (when user is vendor)
    public virtual ICollection<Product> ProductsAsVendor { get; set; } = new List<Product>();
    public virtual ICollection<ProductRegistration> ProductRegistrationsAsVendor { get; set; } = new List<ProductRegistration>();
    public virtual ICollection<VendorCertificate> VendorCertificates { get; set; } = new List<VendorCertificate>();
    public virtual ICollection<BatchInventory> BatchInventoriesAsVendor { get; set; } = new List<BatchInventory>();
    public virtual Wallet? WalletAsVendor { get; set; }
    
    // Verification relationships
    public virtual ICollection<VendorProfile> VerifiedVendorProfiles { get; set; } = new List<VendorProfile>();
    public virtual ICollection<VendorCertificate> VerifiedVendorCertificates { get; set; } = new List<VendorCertificate>();
    public virtual ICollection<ProductCertificate> VerifiedProductCertificates { get; set; } = new List<ProductCertificate>();
    public virtual ICollection<Request> RequestsProcessed { get; set; } = new List<Request>();
}
