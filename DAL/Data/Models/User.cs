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

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    public virtual FarmProfile? FarmProfile { get; set; }
    public virtual VendorProfile? VendorProfile { get; set; }
    
    // One-to-many relationships
    public virtual ICollection<Order> CustomerOrders { get; set; } = new List<Order>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
    public virtual ICollection<ForumComment> ForumComments { get; set; } = new List<ForumComment>();
    public virtual ICollection<ChatbotConversation> ChatbotConversations { get; set; } = new List<ChatbotConversation>();
    public virtual ICollection<EnvironmentalDatum> EnvironmentalData { get; set; } = new List<EnvironmentalDatum>();
    public virtual ICollection<PurchaseInventory> PurchaseInventories { get; set; } = new List<PurchaseInventory>();
    public virtual ICollection<SalesInventory> SalesInventories { get; set; } = new List<SalesInventory>();
    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
    public virtual ICollection<Transaction> TransactionsCreated { get; set; } = new List<Transaction>();
    public virtual ICollection<Transaction> TransactionsProcessed { get; set; } = new List<Transaction>();
    public virtual ICollection<Cashout> CashoutsProcessed { get; set; } = new List<Cashout>();
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
    
    // Verification relationships
    public virtual ICollection<VendorProfile> VerifiedVendorProfiles { get; set; } = new List<VendorProfile>();
    public virtual ICollection<ForumPost> ModeratedForumPosts { get; set; } = new List<ForumPost>();
    public virtual ICollection<ForumComment> ModeratedForumComments { get; set; } = new List<ForumComment>();
    public virtual ICollection<VendorSustainabilityCredential> VerifiedSustainabilityCredentials { get; set; } = new List<VendorSustainabilityCredential>();
    public virtual ICollection<Request> RequestsAssigned { get; set; } = new List<Request>();
    public virtual ICollection<Request> RequestsProcessed { get; set; } = new List<Request>();
}
