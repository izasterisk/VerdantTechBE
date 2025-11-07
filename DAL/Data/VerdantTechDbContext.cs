using Microsoft.EntityFrameworkCore;
using DAL.Data.Models;
using DAL.Data.Configurations;

namespace DAL.Data;

public class VerdantTechDbContext : DbContext
{
    public VerdantTechDbContext(DbContextOptions<VerdantTechDbContext> options) : base(options) { }
    
    // User Management DbSets
    public DbSet<Address> Addresses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<FarmProfile> FarmProfiles { get; set; }
    public DbSet<VendorProfile> VendorProfiles { get; set; }
    public DbSet<UserBankAccount> UserBankAccounts { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    
    // Certificate DbSets
    public DbSet<VendorCertificate> VendorCertificates { get; set; }
    public DbSet<ProductCertificate> ProductCertificates { get; set; }
    
    // Media Management DbSets (v8.1)
    public DbSet<MediaLink> MediaLinks { get; set; }
    
    // Product and Inventory DbSets
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductRegistration> ProductRegistrations { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<BatchInventory> BatchInventories { get; set; }
    public DbSet<ProductSerial> ProductSerials { get; set; }
    public DbSet<ExportInventory> ExportInventories { get; set; }
    
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
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserAddressConfiguration());
        modelBuilder.ApplyConfiguration(new FarmProfileConfiguration());
        modelBuilder.ApplyConfiguration(new VendorProfileConfiguration());
        modelBuilder.ApplyConfiguration(new UserBankAccountConfiguration());
        modelBuilder.ApplyConfiguration(new WalletConfiguration());
        modelBuilder.ApplyConfiguration(new VendorCertificateConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCertificateConfiguration());
        modelBuilder.ApplyConfiguration(new MediaLinkConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductRegistrationConfiguration());
        modelBuilder.ApplyConfiguration(new CartConfiguration());
        modelBuilder.ApplyConfiguration(new CartItemConfiguration());
        modelBuilder.ApplyConfiguration(new BatchInventoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductSerialConfiguration());
        modelBuilder.ApplyConfiguration(new ExportInventoryConfiguration());
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
        modelBuilder.ApplyConfiguration(new ChatbotConversationConfiguration());
        modelBuilder.ApplyConfiguration(new ChatbotMessageConfiguration());
        modelBuilder.ApplyConfiguration(new ForumCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ForumPostConfiguration());
        modelBuilder.ApplyConfiguration(new ForumCommentConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}