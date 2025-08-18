using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Base user authentication and profile table
/// </summary>
public partial class User
{
    public ulong Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public bool? IsVerified { get; set; }

    public string? VerificationToken { get; set; }

    public DateTime? VerificationSentAt { get; set; }

    public string? LanguagePreference { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Status { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<BlogComment> BlogCommentModeratedByNavigations { get; set; } = new List<BlogComment>();

    public virtual ICollection<BlogComment> BlogCommentUsers { get; set; } = new List<BlogComment>();

    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

    public virtual ICollection<ChatbotConversation> ChatbotConversations { get; set; } = new List<ChatbotConversation>();

    public virtual ICollection<EducationalMaterial> EducationalMaterials { get; set; } = new List<EducationalMaterial>();

    public virtual ICollection<EnvironmentalDatum> EnvironmentalData { get; set; } = new List<EnvironmentalDatum>();

    public virtual FarmProfile? FarmProfile { get; set; }

    public virtual ICollection<ForumComment> ForumCommentModeratedByNavigations { get; set; } = new List<ForumComment>();

    public virtual ICollection<ForumComment> ForumCommentUsers { get; set; } = new List<ForumComment>();

    public virtual ICollection<ForumPost> ForumPostModeratedByNavigations { get; set; } = new List<ForumPost>();

    public virtual ICollection<ForumPost> ForumPostUsers { get; set; } = new List<ForumPost>();

    public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();

    public virtual ICollection<KnowledgeBase> KnowledgeBaseCreatedByNavigations { get; set; } = new List<KnowledgeBase>();

    public virtual ICollection<KnowledgeBase> KnowledgeBaseVerifiedByNavigations { get; set; } = new List<KnowledgeBase>();

    public virtual ICollection<Order> OrderCustomers { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderVendors { get; set; } = new List<Order>();

    public virtual ICollection<PlantDiseaseDetection> PlantDiseaseDetections { get; set; } = new List<PlantDiseaseDetection>();

    public virtual ICollection<ProductReview> ProductReviewCustomers { get; set; } = new List<ProductReview>();

    public virtual ICollection<ProductReview> ProductReviewModeratedByNavigations { get; set; } = new List<ProductReview>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<SalesAnalyticsDaily> SalesAnalyticsDailies { get; set; } = new List<SalesAnalyticsDaily>();

    public virtual ICollection<UserActivityLog> UserActivityLogs { get; set; } = new List<UserActivityLog>();

    public virtual ICollection<UserInteraction> UserInteractions { get; set; } = new List<UserInteraction>();

    public virtual VendorProfile? VendorProfileUser { get; set; }

    public virtual ICollection<VendorProfile> VendorProfileVerifiedByNavigations { get; set; } = new List<VendorProfile>();
}
