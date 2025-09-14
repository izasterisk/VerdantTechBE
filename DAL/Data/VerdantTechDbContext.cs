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
    public DbSet<SupportedBank> SupportedBanks { get; set; }
    public DbSet<VendorBankAccount> VendorBankAccounts { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    
    // Sustainability DbSets
    public DbSet<SustainabilityCertification> SustainabilityCertifications { get; set; }
    public DbSet<VendorSustainabilityCredential> VendorSustainabilityCredentials { get; set; }
    public DbSet<ProductSustainabilityCredential> ProductSustainabilityCredentials { get; set; }
    
    // Product and Inventory DbSets
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Cart> Cart { get; set; }
    public DbSet<PurchaseInventory> PurchaseInventories { get; set; }
    public DbSet<SalesInventory> SalesInventories { get; set; }
    
    // Request Management DbSets
    public DbSet<Request> Requests { get; set; }
    
    // Order and Payment DbSets
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Cashout> Cashouts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }
    
    // Environmental Data DbSets
    public DbSet<EnvironmentalDatum> EnvironmentalData { get; set; }
    public DbSet<Fertilizer> Fertilizers { get; set; }
    public DbSet<EnergyUsage> EnergyUsage { get; set; }
    public DbSet<WeatherDataCache> WeatherDataCache { get; set; }
    
    // AI Chatbot DbSets
    public DbSet<ChatbotConversation> ChatbotConversations { get; set; }
    public DbSet<ChatbotMessage> ChatbotMessages { get; set; }
    
    // Community DbSets
    public DbSet<ForumCategory> ForumCategories { get; set; }
    public DbSet<ForumPost> ForumPosts { get; set; }
    public DbSet<ForumComment> ForumComments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new FarmProfileConfiguration());
        modelBuilder.ApplyConfiguration(new VendorProfileConfiguration());
        modelBuilder.ApplyConfiguration(new SupportedBankConfiguration());
        modelBuilder.ApplyConfiguration(new VendorBankAccountConfiguration());
        modelBuilder.ApplyConfiguration(new WalletConfiguration());
        modelBuilder.ApplyConfiguration(new SustainabilityCertificationConfiguration());
        modelBuilder.ApplyConfiguration(new VendorSustainabilityCredentialConfiguration());
        modelBuilder.ApplyConfiguration(new ProductSustainabilityCredentialConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new CartConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseInventoryConfiguration());
        modelBuilder.ApplyConfiguration(new SalesInventoryConfiguration());
        modelBuilder.ApplyConfiguration(new RequestConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderDetailConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new CashoutConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new ProductReviewConfiguration());
        modelBuilder.ApplyConfiguration(new EnvironmentalDataConfiguration());
        modelBuilder.ApplyConfiguration(new FertilizerConfiguration());
        modelBuilder.ApplyConfiguration(new EnergyUsageConfiguration());
        modelBuilder.ApplyConfiguration(new WeatherDataCacheConfiguration());
        modelBuilder.ApplyConfiguration(new ChatbotConversationConfiguration());
        modelBuilder.ApplyConfiguration(new ChatbotMessageConfiguration());
        modelBuilder.ApplyConfiguration(new ForumCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ForumPostConfiguration());
        modelBuilder.ApplyConfiguration(new ForumCommentConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}