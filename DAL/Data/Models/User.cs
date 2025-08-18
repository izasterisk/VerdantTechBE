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

    public Language LanguagePreference { get; set; } = Language.Vi;

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    public virtual FarmProfile? FarmProfile { get; set; }
    public virtual VendorProfile? VendorProfile { get; set; }
    
    // One-to-many relationships
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Order> CustomerOrders { get; set; } = new List<Order>();
    public virtual ICollection<Order> VendorOrders { get; set; } = new List<Order>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();
    public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
    public virtual ICollection<ForumComment> ForumComments { get; set; } = new List<ForumComment>();
    public virtual ICollection<UserInteraction> UserInteractions { get; set; } = new List<UserInteraction>();
    public virtual ICollection<ChatbotConversation> ChatbotConversations { get; set; } = new List<ChatbotConversation>();
    public virtual ICollection<PlantDiseaseDetection> PlantDiseaseDetections { get; set; } = new List<PlantDiseaseDetection>();
    public virtual ICollection<EnvironmentalDatum> EnvironmentalData { get; set; } = new List<EnvironmentalDatum>();
    public virtual ICollection<EducationalMaterial> EducationalMaterials { get; set; } = new List<EducationalMaterial>();
    public virtual ICollection<KnowledgeBase> KnowledgeBaseEntries { get; set; } = new List<KnowledgeBase>();
    public virtual ICollection<UserActivityLog> UserActivityLogs { get; set; } = new List<UserActivityLog>();
    public virtual ICollection<SalesAnalyticsDaily> SalesAnalytics { get; set; } = new List<SalesAnalyticsDaily>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    
    // Verification relationships
    public virtual ICollection<VendorProfile> VerifiedVendorProfiles { get; set; } = new List<VendorProfile>();
    public virtual ICollection<ProductReview> ModeratedProductReviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<BlogComment> ModeratedBlogComments { get; set; } = new List<BlogComment>();
    public virtual ICollection<ForumPost> ModeratedForumPosts { get; set; } = new List<ForumPost>();
    public virtual ICollection<ForumComment> ModeratedForumComments { get; set; } = new List<ForumComment>();
    public virtual ICollection<KnowledgeBase> VerifiedKnowledgeBaseEntries { get; set; } = new List<KnowledgeBase>();
}
