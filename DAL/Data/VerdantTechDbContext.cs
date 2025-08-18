using Microsoft.EntityFrameworkCore;
using DAL.Data.Models;
using DAL.Data.Configurations;

namespace DAL.Data;

public class VerdantTechDbContext : DbContext
{
    public VerdantTechDbContext(DbContextOptions<VerdantTechDbContext> options) : base(options) { }
    
    // User Management DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<FarmProfile> FarmProfiles { get; set; }
    public DbSet<VendorProfile> VendorProfiles { get; set; }
    
    // Product and Inventory DbSets
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<InventoryLog> InventoryLogs { get; set; }
    
    // Order and Payment DbSets
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }
    
    // Environmental Data DbSets
    public DbSet<EnvironmentalDatum> EnvironmentalData { get; set; }
    public DbSet<WeatherDataCache> WeatherDataCache { get; set; }
    
    // AI Chatbot DbSets
    public DbSet<ChatbotConversation> ChatbotConversations { get; set; }
    public DbSet<ChatbotMessage> ChatbotMessages { get; set; }
    public DbSet<KnowledgeBase> KnowledgeBase { get; set; }
    
    // Plant Disease Detection DbSets
    public DbSet<PlantDiseaseDetection> PlantDiseaseDetections { get; set; }
    
    // Community DbSets
    public DbSet<ForumCategory> ForumCategories { get; set; }
    public DbSet<ForumPost> ForumPosts { get; set; }
    public DbSet<ForumComment> ForumComments { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<BlogComment> BlogComments { get; set; }
    public DbSet<UserInteraction> UserInteractions { get; set; }
    
    // Educational Content DbSets
    public DbSet<EducationalMaterial> EducationalMaterials { get; set; }
    
    // Analytics DbSets
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    public DbSet<SalesAnalyticsDaily> SalesAnalyticsDailies { get; set; }
    
    // System DbSets
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new FarmProfileConfiguration());
        modelBuilder.ApplyConfiguration(new VendorProfileConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new WeatherDataCacheConfiguration());
        modelBuilder.ApplyConfiguration(new ChatbotMessageConfiguration());
        modelBuilder.ApplyConfiguration(new BlogPostConfiguration());
        modelBuilder.ApplyConfiguration(new BlogCommentConfiguration());
        modelBuilder.ApplyConfiguration(new ForumPostConfiguration());
        modelBuilder.ApplyConfiguration(new ForumCommentConfiguration());
        
        modelBuilder.ApplyConfiguration(new ProductReviewConfiguration());
        modelBuilder.ApplyConfiguration(new EnvironmentalDataConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryLogConfiguration());
        modelBuilder.ApplyConfiguration(new ChatbotConversationConfiguration());
        modelBuilder.ApplyConfiguration(new KnowledgeBaseConfiguration());
        modelBuilder.ApplyConfiguration(new PlantDiseaseDetectionConfiguration());
        modelBuilder.ApplyConfiguration(new ForumCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new UserInteractionConfiguration());
        modelBuilder.ApplyConfiguration(new EducationalMaterialConfiguration());
        modelBuilder.ApplyConfiguration(new UserActivityLogConfiguration());
        modelBuilder.ApplyConfiguration(new SalesAnalyticsDailyConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSettingConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}
