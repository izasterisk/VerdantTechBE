using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace DAL.Models;

public partial class VerdantTechContext : DbContext
{
    public VerdantTechContext()
    {
    }

    public VerdantTechContext(DbContextOptions<VerdantTechContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<BlogComment> BlogComments { get; set; }

    public virtual DbSet<BlogPost> BlogPosts { get; set; }

    public virtual DbSet<ChatbotConversation> ChatbotConversations { get; set; }

    public virtual DbSet<ChatbotMessage> ChatbotMessages { get; set; }

    public virtual DbSet<EducationalMaterial> EducationalMaterials { get; set; }

    public virtual DbSet<EnvironmentalDatum> EnvironmentalData { get; set; }

    public virtual DbSet<FarmProfile> FarmProfiles { get; set; }

    public virtual DbSet<ForumCategory> ForumCategories { get; set; }

    public virtual DbSet<ForumComment> ForumComments { get; set; }

    public virtual DbSet<ForumPost> ForumPosts { get; set; }

    public virtual DbSet<InventoryLog> InventoryLogs { get; set; }

    public virtual DbSet<KnowledgeBase> KnowledgeBases { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PlantDiseaseDetection> PlantDiseaseDetections { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<SalesAnalyticsDaily> SalesAnalyticsDailies { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserActivityLog> UserActivityLogs { get; set; }

    public virtual DbSet<UserInteraction> UserInteractions { get; set; }

    public virtual DbSet<VendorProfile> VendorProfiles { get; set; }

    public virtual DbSet<WeatherDataCache> WeatherDataCaches { get; set; }

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseMySql("server=localhost;port=3306;database=verdanttech_db;uid=root;pwd=12345;charset=utf8", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("audit_logs", tb => tb.HasComment("Audit trail for important system changes"));

            entity.HasIndex(e => e.Action, "idx_action");

            entity.HasIndex(e => e.CreatedAt, "idx_created");

            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "idx_entity");

            entity.HasIndex(e => e.UserId, "idx_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(50)
                .HasColumnName("entity_type");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.NewValues)
                .HasColumnType("json")
                .HasColumnName("new_values");
            entity.Property(e => e.OldValues)
                .HasColumnType("json")
                .HasColumnName("old_values");
            entity.Property(e => e.UserAgent)
                .HasColumnType("text")
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("audit_logs_ibfk_1");
        });

        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blog_comments", tb => tb.HasComment("Blog post comments"));

            entity.HasIndex(e => new { e.PostId, e.Status }, "idx_post_status");

            entity.HasIndex(e => e.UserId, "idx_user");

            entity.HasIndex(e => e.ModeratedBy, "moderated_by");

            entity.HasIndex(e => e.ParentId, "parent_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DislikeCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("dislike_count");
            entity.Property(e => e.LikeCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("like_count");
            entity.Property(e => e.ModeratedAt)
                .HasColumnType("timestamp")
                .HasColumnName("moderated_at");
            entity.Property(e => e.ModeratedBy).HasColumnName("moderated_by");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'pending'")
                .HasColumnType("enum('approved','pending','spam','deleted')")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ModeratedByNavigation).WithMany(p => p.BlogCommentModeratedByNavigations)
                .HasForeignKey(d => d.ModeratedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("blog_comments_ibfk_4");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("blog_comments_ibfk_3");

            entity.HasOne(d => d.Post).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("blog_comments_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.BlogCommentUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("blog_comments_ibfk_2");
        });

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blog_posts", tb => tb.HasComment("Blog articles and educational content"));

            entity.HasIndex(e => e.AuthorId, "idx_author");

            entity.HasIndex(e => e.PublishedAt, "idx_published");

            entity.HasIndex(e => new { e.Title, e.Excerpt, e.Content }, "idx_search").HasAnnotation("MySql:FullTextIndex", true);

            entity.HasIndex(e => e.Slug, "idx_slug").IsUnique();

            entity.HasIndex(e => new { e.Status, e.IsFeatured }, "idx_status_featured");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.CommentCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("comment_count");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DislikeCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("dislike_count");
            entity.Property(e => e.Excerpt)
                .HasColumnType("text")
                .HasColumnName("excerpt");
            entity.Property(e => e.FeaturedImageUrl)
                .HasMaxLength(500)
                .HasColumnName("featured_image_url");
            entity.Property(e => e.IsFeatured)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_featured");
            entity.Property(e => e.LikeCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("like_count");
            entity.Property(e => e.PublishedAt)
                .HasColumnType("timestamp")
                .HasColumnName("published_at");
            entity.Property(e => e.ReadingTimeMinutes).HasColumnName("reading_time_minutes");
            entity.Property(e => e.ScheduledAt)
                .HasColumnType("timestamp")
                .HasColumnName("scheduled_at");
            entity.Property(e => e.SeoDescription)
                .HasColumnType("text")
                .HasColumnName("seo_description");
            entity.Property(e => e.SeoKeywords)
                .HasColumnType("json")
                .HasColumnName("seo_keywords");
            entity.Property(e => e.SeoTitle)
                .HasMaxLength(255)
                .HasColumnName("seo_title");
            entity.Property(e => e.Slug).HasColumnName("slug");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'draft'")
                .HasColumnType("enum('draft','published','scheduled','archived')")
                .HasColumnName("status");
            entity.Property(e => e.Tags)
                .HasComment("Array of tags")
                .HasColumnType("json")
                .HasColumnName("tags");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.ViewCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("view_count");

            entity.HasOne(d => d.Author).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("blog_posts_ibfk_1");
        });

        modelBuilder.Entity<ChatbotConversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("chatbot_conversations", tb => tb.HasComment("Chatbot conversation sessions"));

            entity.HasIndex(e => e.IsActive, "idx_active");

            entity.HasIndex(e => e.StartedAt, "idx_started");

            entity.HasIndex(e => new { e.UserId, e.SessionId }, "idx_user_session");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Context)
                .HasComment("Conversation context and metadata")
                .HasColumnType("json")
                .HasColumnName("context");
            entity.Property(e => e.EndedAt)
                .HasColumnType("timestamp")
                .HasColumnName("ended_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("started_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.ChatbotConversations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("chatbot_conversations_ibfk_1");
        });

        modelBuilder.Entity<ChatbotMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("chatbot_messages", tb => tb.HasComment("Individual chatbot messages"));

            entity.HasIndex(e => e.ConversationId, "idx_conversation");

            entity.HasIndex(e => e.CreatedAt, "idx_created");

            entity.HasIndex(e => e.MessageType, "idx_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Attachments)
                .HasComment("Image or file attachments")
                .HasColumnType("json")
                .HasColumnName("attachments");
            entity.Property(e => e.ConfidenceScore)
                .HasPrecision(3, 2)
                .HasComment("AI confidence score")
                .HasColumnName("confidence_score");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Entities)
                .HasComment("Extracted entities from message")
                .HasColumnType("json")
                .HasColumnName("entities");
            entity.Property(e => e.Intent)
                .HasMaxLength(100)
                .HasComment("Detected user intent")
                .HasColumnName("intent");
            entity.Property(e => e.MessageText)
                .HasColumnType("text")
                .HasColumnName("message_text");
            entity.Property(e => e.MessageType)
                .HasColumnType("enum('user','bot','system')")
                .HasColumnName("message_type");
            entity.Property(e => e.SuggestedActions)
                .HasComment("Quick reply suggestions")
                .HasColumnType("json")
                .HasColumnName("suggested_actions");

            entity.HasOne(d => d.Conversation).WithMany(p => p.ChatbotMessages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("chatbot_messages_ibfk_1");
        });

        modelBuilder.Entity<EducationalMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("educational_materials", tb => tb.HasComment("Educational content created by experts and vendors"));

            entity.HasIndex(e => e.CreatedBy, "idx_creator");

            entity.HasIndex(e => e.Language, "idx_language");

            entity.HasIndex(e => e.RatingAverage, "idx_rating");

            entity.HasIndex(e => new { e.Title, e.Description }, "idx_search").HasAnnotation("MySql:FullTextIndex", true);

            entity.HasIndex(e => e.Status, "idx_status");

            entity.HasIndex(e => e.MaterialType, "idx_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContentUrl)
                .HasMaxLength(500)
                .HasComment("URL to file or video")
                .HasColumnName("content_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.DifficultyLevel)
                .HasDefaultValueSql("'beginner'")
                .HasColumnType("enum('beginner','intermediate','advanced')")
                .HasColumnName("difficulty_level");
            entity.Property(e => e.DownloadCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("download_count");
            entity.Property(e => e.DurationMinutes)
                .HasComment("For video content")
                .HasColumnName("duration_minutes");
            entity.Property(e => e.FileSizeMb)
                .HasPrecision(10, 2)
                .HasColumnName("file_size_mb");
            entity.Property(e => e.IsPremium)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_premium");
            entity.Property(e => e.Language)
                .HasDefaultValueSql("'vi'")
                .HasColumnType("enum('vi','en')")
                .HasColumnName("language");
            entity.Property(e => e.MaterialType)
                .HasColumnType("enum('guide','tutorial','research','case_study','infographic','video')")
                .HasColumnName("material_type");
            entity.Property(e => e.PublishedAt)
                .HasColumnType("timestamp")
                .HasColumnName("published_at");
            entity.Property(e => e.RatingAverage)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("rating_average");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'draft'")
                .HasColumnType("enum('draft','published','archived')")
                .HasColumnName("status");
            entity.Property(e => e.TargetAudience)
                .HasComment("Array of audience types")
                .HasColumnType("json")
                .HasColumnName("target_audience");
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(500)
                .HasColumnName("thumbnail_url");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Topics)
                .HasComment("Array of related topics")
                .HasColumnType("json")
                .HasColumnName("topics");
            entity.Property(e => e.TotalRatings)
                .HasDefaultValueSql("'0'")
                .HasColumnName("total_ratings");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.ViewCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("view_count");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.EducationalMaterials)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("educational_materials_ibfk_1");
        });

        modelBuilder.Entity<EnvironmentalDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("environmental_data", tb => tb.HasComment("Manual environmental data input by farmers"));

            entity.HasIndex(e => e.MeasurementDate, "idx_date");

            entity.HasIndex(e => new { e.FarmProfileId, e.MeasurementDate, e.SoilPh }, "idx_env_data_analysis");

            entity.HasIndex(e => new { e.FarmProfileId, e.MeasurementDate }, "idx_farm_date");

            entity.HasIndex(e => e.UserId, "idx_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Co2Footprint)
                .HasPrecision(10, 2)
                .HasComment("CO2 emissions in kg")
                .HasColumnName("co2_footprint");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.FarmProfileId).HasColumnName("farm_profile_id");
            entity.Property(e => e.MeasurementDate).HasColumnName("measurement_date");
            entity.Property(e => e.NitrogenLevel)
                .HasPrecision(10, 2)
                .HasComment("N content in mg/kg")
                .HasColumnName("nitrogen_level");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.OrganicMatterPercentage)
                .HasPrecision(5, 2)
                .HasColumnName("organic_matter_percentage");
            entity.Property(e => e.PhosphorusLevel)
                .HasPrecision(10, 2)
                .HasComment("P content in mg/kg")
                .HasColumnName("phosphorus_level");
            entity.Property(e => e.PotassiumLevel)
                .HasPrecision(10, 2)
                .HasComment("K content in mg/kg")
                .HasColumnName("potassium_level");
            entity.Property(e => e.SoilMoisturePercentage)
                .HasPrecision(5, 2)
                .HasColumnName("soil_moisture_percentage");
            entity.Property(e => e.SoilPh)
                .HasPrecision(3, 1)
                .HasColumnName("soil_ph");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.FarmProfile).WithMany(p => p.EnvironmentalData)
                .HasForeignKey(d => d.FarmProfileId)
                .HasConstraintName("environmental_data_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.EnvironmentalData)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("environmental_data_ibfk_2");
        });

        modelBuilder.Entity<FarmProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("farm_profiles", tb => tb.HasComment("Farm profile details for farmer users"));

            entity.HasIndex(e => e.FarmSizeHectares, "idx_farm_size");

            entity.HasIndex(e => new { e.Province, e.District }, "idx_location");

            entity.HasIndex(e => e.UserId, "user_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificationTypes)
                .HasComment("Array of certifications like organic, VietGAP, GlobalGAP")
                .HasColumnType("json")
                .HasColumnName("certification_types");
            entity.Property(e => e.Commune)
                .HasMaxLength(100)
                .HasColumnName("commune");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasColumnName("district");
            entity.Property(e => e.FarmName)
                .HasMaxLength(255)
                .HasColumnName("farm_name");
            entity.Property(e => e.FarmSizeHectares)
                .HasPrecision(10, 2)
                .HasColumnName("farm_size_hectares");
            entity.Property(e => e.FarmingExperienceYears).HasColumnName("farming_experience_years");
            entity.Property(e => e.IrrigationType)
                .HasMaxLength(100)
                .HasColumnName("irrigation_type");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 8)
                .HasColumnName("latitude");
            entity.Property(e => e.LocationAddress)
                .HasColumnType("text")
                .HasColumnName("location_address");
            entity.Property(e => e.Longitude)
                .HasPrecision(11, 8)
                .HasColumnName("longitude");
            entity.Property(e => e.PrimaryCrops)
                .HasComment("Array of main crops grown")
                .HasColumnType("json")
                .HasColumnName("primary_crops");
            entity.Property(e => e.Province)
                .HasMaxLength(100)
                .HasColumnName("province");
            entity.Property(e => e.SoilType)
                .HasMaxLength(100)
                .HasColumnName("soil_type");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.FarmProfile)
                .HasForeignKey<FarmProfile>(d => d.UserId)
                .HasConstraintName("farm_profiles_ibfk_1");
        });

        modelBuilder.Entity<ForumCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("forum_categories", tb => tb.HasComment("Forum discussion categories"));

            entity.HasIndex(e => new { e.IsActive, e.SortOrder }, "idx_active_sort");

            entity.HasIndex(e => e.Slug, "idx_slug").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IconUrl)
                .HasMaxLength(500)
                .HasColumnName("icon_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NameEn)
                .HasMaxLength(255)
                .HasColumnName("name_en");
            entity.Property(e => e.Slug).HasColumnName("slug");
            entity.Property(e => e.SortOrder)
                .HasDefaultValueSql("'0'")
                .HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ForumComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("forum_comments", tb => tb.HasComment("Forum post comments"));

            entity.HasIndex(e => e.ParentId, "idx_parent");

            entity.HasIndex(e => e.PostId, "idx_post");

            entity.HasIndex(e => e.Status, "idx_status");

            entity.HasIndex(e => e.UserId, "idx_user");

            entity.HasIndex(e => e.ModeratedBy, "moderated_by");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DislikeCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("dislike_count");
            entity.Property(e => e.IsSolution)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_solution");
            entity.Property(e => e.LikeCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("like_count");
            entity.Property(e => e.ModeratedBy).HasColumnName("moderated_by");
            entity.Property(e => e.ModeratedReason)
                .HasColumnType("text")
                .HasColumnName("moderated_reason");
            entity.Property(e => e.ParentId)
                .HasComment("For nested comments")
                .HasColumnName("parent_id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'visible'")
                .HasColumnType("enum('visible','moderated','deleted')")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ModeratedByNavigation).WithMany(p => p.ForumCommentModeratedByNavigations)
                .HasForeignKey(d => d.ModeratedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("forum_comments_ibfk_4");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("forum_comments_ibfk_3");

            entity.HasOne(d => d.Post).WithMany(p => p.ForumComments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("forum_comments_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.ForumCommentUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("forum_comments_ibfk_2");
        });

        modelBuilder.Entity<ForumPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("forum_posts", tb => tb.HasComment("Forum discussion posts"));

            entity.HasIndex(e => e.CategoryId, "idx_category");

            entity.HasIndex(e => e.LastActivityAt, "idx_last_activity");

            entity.HasIndex(e => new { e.Title, e.Content }, "idx_search").HasAnnotation("MySql:FullTextIndex", true);

            entity.HasIndex(e => new { e.Status, e.IsPinned }, "idx_status_pinned");

            entity.HasIndex(e => e.UserId, "idx_user");

            entity.HasIndex(e => e.ModeratedBy, "moderated_by");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DislikeCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("dislike_count");
            entity.Property(e => e.IsLocked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_locked");
            entity.Property(e => e.IsPinned)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_pinned");
            entity.Property(e => e.LastActivityAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("last_activity_at");
            entity.Property(e => e.LikeCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("like_count");
            entity.Property(e => e.ModeratedBy).HasColumnName("moderated_by");
            entity.Property(e => e.ModeratedReason)
                .HasColumnType("text")
                .HasColumnName("moderated_reason");
            entity.Property(e => e.ReplyCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("reply_count");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'published'")
                .HasColumnType("enum('published','draft','moderated','deleted')")
                .HasColumnName("status");
            entity.Property(e => e.Tags)
                .HasComment("Array of tags")
                .HasColumnType("json")
                .HasColumnName("tags");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ViewCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("view_count");

            entity.HasOne(d => d.Category).WithMany(p => p.ForumPosts)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("forum_posts_ibfk_1");

            entity.HasOne(d => d.ModeratedByNavigation).WithMany(p => p.ForumPostModeratedByNavigations)
                .HasForeignKey(d => d.ModeratedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("forum_posts_ibfk_3");

            entity.HasOne(d => d.User).WithMany(p => p.ForumPostUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("forum_posts_ibfk_2");
        });

        modelBuilder.Entity<InventoryLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("inventory_logs", tb => tb.HasComment("Inventory movement tracking"));

            entity.HasIndex(e => e.CreatedBy, "created_by");

            entity.HasIndex(e => e.CreatedAt, "idx_created_at");

            entity.HasIndex(e => new { e.ProductId, e.Type }, "idx_product_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BalanceAfter).HasColumnName("balance_after");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasColumnName("reason");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(50)
                .HasComment("order, return, manual")
                .HasColumnName("reference_type");
            entity.Property(e => e.Type)
                .HasColumnType("enum('in','out','adjustment')")
                .HasColumnName("type");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryLogs)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("inventory_logs_ibfk_2");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryLogs)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("inventory_logs_ibfk_1");
        });

        modelBuilder.Entity<KnowledgeBase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("knowledge_base", tb => tb.HasComment("Knowledge base for AI chatbot responses"));

            entity.HasIndex(e => e.CreatedBy, "created_by");

            entity.HasIndex(e => new { e.Category, e.Subcategory }, "idx_category");

            entity.HasIndex(e => e.Language, "idx_language");

            entity.HasIndex(e => new { e.Question, e.Answer }, "idx_search").HasAnnotation("MySql:FullTextIndex", true);

            entity.HasIndex(e => e.IsVerified, "idx_verified");

            entity.HasIndex(e => e.VerifiedBy, "verified_by");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Answer)
                .HasColumnType("text")
                .HasColumnName("answer");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.HelpfulCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("helpful_count");
            entity.Property(e => e.IsVerified)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_verified");
            entity.Property(e => e.Keywords)
                .HasComment("Array of keywords for matching")
                .HasColumnType("json")
                .HasColumnName("keywords");
            entity.Property(e => e.Language)
                .HasDefaultValueSql("'vi'")
                .HasColumnType("enum('vi','en')")
                .HasColumnName("language");
            entity.Property(e => e.Question)
                .HasColumnType("text")
                .HasColumnName("question");
            entity.Property(e => e.SourceUrl)
                .HasMaxLength(500)
                .HasColumnName("source_url");
            entity.Property(e => e.Subcategory)
                .HasMaxLength(100)
                .HasColumnName("subcategory");
            entity.Property(e => e.UnhelpfulCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("unhelpful_count");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UsageCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("usage_count");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.KnowledgeBaseCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("knowledge_base_ibfk_2");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.KnowledgeBaseVerifiedByNavigations)
                .HasForeignKey(d => d.VerifiedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("knowledge_base_ibfk_1");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("orders", tb => tb.HasComment("Customer orders"));

            entity.HasIndex(e => e.CreatedAt, "idx_created_at");

            entity.HasIndex(e => e.CustomerId, "idx_customer");

            entity.HasIndex(e => e.OrderNumber, "idx_order_number").IsUnique();

            entity.HasIndex(e => new { e.CreatedAt, e.Status }, "idx_orders_date_range");

            entity.HasIndex(e => e.Status, "idx_status");

            entity.HasIndex(e => e.VendorId, "idx_vendor");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BillingAddress)
                .HasColumnType("json")
                .HasColumnName("billing_address");
            entity.Property(e => e.CancelledAt)
                .HasColumnType("timestamp")
                .HasColumnName("cancelled_at");
            entity.Property(e => e.CancelledReason)
                .HasColumnType("text")
                .HasColumnName("cancelled_reason");
            entity.Property(e => e.ConfirmedAt)
                .HasColumnType("timestamp")
                .HasColumnName("confirmed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .HasDefaultValueSql("'VND'")
                .HasColumnName("currency_code");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DeliveredAt)
                .HasColumnType("timestamp")
                .HasColumnName("delivered_at");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("discount_amount");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(50)
                .HasColumnName("order_number");
            entity.Property(e => e.ShippedAt)
                .HasColumnType("timestamp")
                .HasColumnName("shipped_at");
            entity.Property(e => e.ShippingAddress)
                .HasColumnType("json")
                .HasColumnName("shipping_address");
            entity.Property(e => e.ShippingFee)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("shipping_fee");
            entity.Property(e => e.ShippingMethod)
                .HasMaxLength(100)
                .HasColumnName("shipping_method");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'pending'")
                .HasColumnType("enum('pending','confirmed','processing','shipped','delivered','cancelled','refunded')")
                .HasColumnName("status");
            entity.Property(e => e.Subtotal)
                .HasPrecision(12, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("tax_amount");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.TrackingNumber)
                .HasMaxLength(100)
                .HasColumnName("tracking_number");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.VendorId).HasColumnName("vendor_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.OrderCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_ibfk_1");

            entity.HasOne(d => d.Vendor).WithMany(p => p.OrderVendors)
                .HasForeignKey(d => d.VendorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_ibfk_2");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("order_items", tb => tb.HasComment("Order line items"));

            entity.HasIndex(e => e.OrderId, "idx_order");

            entity.HasIndex(e => e.ProductId, "idx_product");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("discount_amount");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(255)
                .HasComment("Snapshot of product name")
                .HasColumnName("product_name");
            entity.Property(e => e.ProductSku)
                .HasMaxLength(100)
                .HasComment("Snapshot of SKU")
                .HasColumnName("product_sku");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Specifications)
                .HasComment("Snapshot of product specs")
                .HasColumnType("json")
                .HasColumnName("specifications");
            entity.Property(e => e.Subtotal)
                .HasPrecision(12, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(12, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_items_ibfk_1");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_items_ibfk_2");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("payments", tb => tb.HasComment("Payment transactions"));

            entity.HasIndex(e => e.OrderId, "idx_order");

            entity.HasIndex(e => e.PaymentMethod, "idx_payment_method");

            entity.HasIndex(e => e.Status, "idx_status");

            entity.HasIndex(e => e.TransactionId, "idx_transaction").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .HasDefaultValueSql("'VND'")
                .HasColumnName("currency_code");
            entity.Property(e => e.FailedAt)
                .HasColumnType("timestamp")
                .HasColumnName("failed_at");
            entity.Property(e => e.GatewayResponse)
                .HasComment("Raw response from payment gateway")
                .HasColumnType("json")
                .HasColumnName("gateway_response");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaidAt)
                .HasColumnType("timestamp")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentGateway)
                .HasColumnType("enum('stripe','paypal','vnpay','momo','manual')")
                .HasColumnName("payment_gateway");
            entity.Property(e => e.PaymentMethod)
                .HasColumnType("enum('credit_card','debit_card','paypal','stripe','bank_transfer','cod')")
                .HasColumnName("payment_method");
            entity.Property(e => e.RefundAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("refund_amount");
            entity.Property(e => e.RefundReason)
                .HasColumnType("text")
                .HasColumnName("refund_reason");
            entity.Property(e => e.RefundedAt)
                .HasColumnType("timestamp")
                .HasColumnName("refunded_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'pending'")
                .HasColumnType("enum('pending','processing','completed','failed','refunded','partially_refunded')")
                .HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payments_ibfk_1");
        });

        modelBuilder.Entity<PlantDiseaseDetection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("plant_disease_detections", tb => tb.HasComment("Plant disease detection history"));

            entity.HasIndex(e => e.CreatedAt, "idx_created");

            entity.HasIndex(e => e.FarmProfileId, "idx_farm");

            entity.HasIndex(e => e.PlantType, "idx_plant_type");

            entity.HasIndex(e => e.UserId, "idx_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AiProvider)
                .HasMaxLength(50)
                .HasComment("Computer vision API used")
                .HasColumnName("ai_provider");
            entity.Property(e => e.AiResponse)
                .HasComment("Raw AI API response")
                .HasColumnType("json")
                .HasColumnName("ai_response");
            entity.Property(e => e.ChemicalTreatmentSuggestions)
                .HasComment("Array of chemical treatment options")
                .HasColumnType("json")
                .HasColumnName("chemical_treatment_suggestions");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DetectedDiseases)
                .HasComment("Array of {disease_name, confidence, severity}")
                .HasColumnType("json")
                .HasColumnName("detected_diseases");
            entity.Property(e => e.FarmProfileId).HasColumnName("farm_profile_id");
            entity.Property(e => e.FeedbackComments)
                .HasColumnType("text")
                .HasColumnName("feedback_comments");
            entity.Property(e => e.ImageMetadata)
                .HasComment("Image size, format, etc.")
                .HasColumnType("json")
                .HasColumnName("image_metadata");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.OrganicTreatmentSuggestions)
                .HasComment("Array of organic treatment options")
                .HasColumnType("json")
                .HasColumnName("organic_treatment_suggestions");
            entity.Property(e => e.PlantType)
                .HasMaxLength(100)
                .HasColumnName("plant_type");
            entity.Property(e => e.PreventionTips)
                .HasComment("Array of prevention recommendations")
                .HasColumnType("json")
                .HasColumnName("prevention_tips");
            entity.Property(e => e.UserFeedback)
                .HasColumnType("enum('helpful','not_helpful','partially_helpful')")
                .HasColumnName("user_feedback");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.FarmProfile).WithMany(p => p.PlantDiseaseDetections)
                .HasForeignKey(d => d.FarmProfileId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("plant_disease_detections_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.PlantDiseaseDetections)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("plant_disease_detections_ibfk_1");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("products", tb => tb.HasComment("Green agricultural equipment products"));

            entity.HasIndex(e => new { e.IsActive, e.IsFeatured }, "idx_active_featured");

            entity.HasIndex(e => e.CategoryId, "idx_category");

            entity.HasIndex(e => e.Name, "idx_name");

            entity.HasIndex(e => e.Price, "idx_price");

            entity.HasIndex(e => new { e.IsActive, e.CategoryId, e.Price }, "idx_products_search");

            entity.HasIndex(e => e.RatingAverage, "idx_rating");

            entity.HasIndex(e => new { e.Name, e.NameEn, e.Description, e.DescriptionEn }, "idx_search").HasAnnotation("MySql:FullTextIndex", true);

            entity.HasIndex(e => e.Sku, "idx_sku").IsUnique();

            entity.HasIndex(e => e.VendorId, "idx_vendor");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.DescriptionEn)
                .HasColumnType("text")
                .HasColumnName("description_en");
            entity.Property(e => e.DimensionsCm)
                .HasComment("{length, width, height}")
                .HasColumnType("json")
                .HasColumnName("dimensions_cm");
            entity.Property(e => e.DiscountPercentage)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("discount_percentage");
            entity.Property(e => e.EnergyEfficiencyRating)
                .HasMaxLength(10)
                .HasColumnName("energy_efficiency_rating");
            entity.Property(e => e.GreenCertifications)
                .HasComment("Array of eco certifications")
                .HasColumnType("json")
                .HasColumnName("green_certifications");
            entity.Property(e => e.Images)
                .HasComment("Array of image URLs")
                .HasColumnType("json")
                .HasColumnName("images");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.IsFeatured)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_featured");
            entity.Property(e => e.ManualUrls)
                .HasComment("Array of manual/guide URLs")
                .HasColumnType("json")
                .HasColumnName("manual_urls");
            entity.Property(e => e.MinOrderQuantity)
                .HasDefaultValueSql("'1'")
                .HasColumnName("min_order_quantity");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.NameEn).HasColumnName("name_en");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasColumnName("price");
            entity.Property(e => e.RatingAverage)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("rating_average");
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("sku");
            entity.Property(e => e.SoldCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("sold_count");
            entity.Property(e => e.Specifications)
                .HasComment("Technical specifications as key-value pairs")
                .HasColumnType("json")
                .HasColumnName("specifications");
            entity.Property(e => e.StockQuantity)
                .HasDefaultValueSql("'0'")
                .HasColumnName("stock_quantity");
            entity.Property(e => e.TotalReviews)
                .HasDefaultValueSql("'0'")
                .HasColumnName("total_reviews");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.VendorId).HasColumnName("vendor_id");
            entity.Property(e => e.ViewCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("view_count");
            entity.Property(e => e.WarrantyMonths)
                .HasDefaultValueSql("'12'")
                .HasColumnName("warranty_months");
            entity.Property(e => e.WeightKg)
                .HasPrecision(10, 3)
                .HasColumnName("weight_kg");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_ibfk_2");

            entity.HasOne(d => d.Vendor).WithMany(p => p.Products)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("products_ibfk_1");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("product_categories", tb => tb.HasComment("Hierarchical product categories"));

            entity.HasIndex(e => new { e.IsActive, e.SortOrder }, "idx_active_sort");

            entity.HasIndex(e => e.ParentId, "idx_parent");

            entity.HasIndex(e => e.Slug, "idx_slug").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IconUrl)
                .HasMaxLength(500)
                .HasColumnName("icon_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NameEn)
                .HasMaxLength(255)
                .HasColumnName("name_en");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Slug).HasColumnName("slug");
            entity.Property(e => e.SortOrder)
                .HasDefaultValueSql("'0'")
                .HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("product_categories_ibfk_1");
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("product_reviews", tb => tb.HasComment("Product reviews and ratings"));

            entity.HasIndex(e => e.CreatedAt, "idx_created_at");

            entity.HasIndex(e => e.CustomerId, "idx_customer");

            entity.HasIndex(e => new { e.ProductId, e.Rating }, "idx_product_rating");

            entity.HasIndex(e => e.Status, "idx_status");

            entity.HasIndex(e => e.ModeratedBy, "moderated_by");

            entity.HasIndex(e => e.OrderId, "order_id");

            entity.HasIndex(e => new { e.ProductId, e.OrderId, e.CustomerId }, "unique_product_order_customer").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasColumnType("text")
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.HelpfulCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("helpful_count");
            entity.Property(e => e.Images)
                .HasComment("Array of review image URLs")
                .HasColumnType("json")
                .HasColumnName("images");
            entity.Property(e => e.IsFeatured)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_featured");
            entity.Property(e => e.IsVerifiedPurchase)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_verified_purchase");
            entity.Property(e => e.ModeratedAt)
                .HasColumnType("timestamp")
                .HasColumnName("moderated_at");
            entity.Property(e => e.ModeratedBy).HasColumnName("moderated_by");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'pending'")
                .HasColumnType("enum('pending','approved','rejected')")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UnhelpfulCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("unhelpful_count");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.VendorRepliedAt)
                .HasColumnType("timestamp")
                .HasColumnName("vendor_replied_at");
            entity.Property(e => e.VendorReply)
                .HasColumnType("text")
                .HasColumnName("vendor_reply");

            entity.HasOne(d => d.Customer).WithMany(p => p.ProductReviewCustomers)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("product_reviews_ibfk_3");

            entity.HasOne(d => d.ModeratedByNavigation).WithMany(p => p.ProductReviewModeratedByNavigations)
                .HasForeignKey(d => d.ModeratedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("product_reviews_ibfk_4");

            entity.HasOne(d => d.Order).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("product_reviews_ibfk_2");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("product_reviews_ibfk_1");
        });

        modelBuilder.Entity<SalesAnalyticsDaily>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("sales_analytics_daily", tb => tb.HasComment("Daily sales analytics aggregation"));

            entity.HasIndex(e => e.Date, "idx_date");

            entity.HasIndex(e => e.VendorId, "idx_vendor");

            entity.HasIndex(e => new { e.Date, e.VendorId }, "unique_date_vendor").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AverageOrderValue)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("average_order_value");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.NewCustomers)
                .HasDefaultValueSql("'0'")
                .HasColumnName("new_customers");
            entity.Property(e => e.ReturningCustomers)
                .HasDefaultValueSql("'0'")
                .HasColumnName("returning_customers");
            entity.Property(e => e.TopProducts)
                .HasComment("Array of top selling products")
                .HasColumnType("json")
                .HasColumnName("top_products");
            entity.Property(e => e.TotalOrders)
                .HasDefaultValueSql("'0'")
                .HasColumnName("total_orders");
            entity.Property(e => e.TotalProductsSold)
                .HasDefaultValueSql("'0'")
                .HasColumnName("total_products_sold");
            entity.Property(e => e.TotalRevenue)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("total_revenue");
            entity.Property(e => e.VendorId).HasColumnName("vendor_id");

            entity.HasOne(d => d.Vendor).WithMany(p => p.SalesAnalyticsDailies)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("sales_analytics_daily_ibfk_1");
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("system_settings", tb => tb.HasComment("System configuration settings"));

            entity.HasIndex(e => e.SettingKey, "idx_key").IsUnique();

            entity.HasIndex(e => e.IsPublic, "idx_public");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IsPublic)
                .HasDefaultValueSql("'0'")
                .HasComment("Can be exposed to frontend")
                .HasColumnName("is_public");
            entity.Property(e => e.SettingKey)
                .HasMaxLength(100)
                .HasColumnName("setting_key");
            entity.Property(e => e.SettingType)
                .HasDefaultValueSql("'string'")
                .HasColumnType("enum('string','number','boolean','json')")
                .HasColumnName("setting_type");
            entity.Property(e => e.SettingValue)
                .HasColumnType("text")
                .HasColumnName("setting_value");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users", tb => tb.HasComment("Base user authentication and profile table"));

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.CreatedAt, "idx_created_at");

            entity.HasIndex(e => e.Role, "idx_role");

            entity.HasIndex(e => e.Status, "idx_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.IsVerified)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_verified");
            entity.Property(e => e.LanguagePreference)
                .HasDefaultValueSql("'vi'")
                .HasColumnType("enum('vi','en')")
                .HasColumnName("language_preference");
            entity.Property(e => e.LastLoginAt)
                .HasColumnType("timestamp")
                .HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'customer'")
                .HasColumnType("enum('customer','farmer','seller','vendor','admin','expert','content_manager')")
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'active'")
                .HasColumnType("enum('active','inactive','suspended','deleted')")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.VerificationSentAt)
                .HasColumnType("timestamp")
                .HasColumnName("verification_sent_at");
            entity.Property(e => e.VerificationToken)
                .HasMaxLength(255)
                .HasColumnName("verification_token");
        });

        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user_activity_logs", tb => tb.HasComment("User activity tracking for analytics"));

            entity.HasIndex(e => e.CreatedAt, "idx_created");

            entity.HasIndex(e => e.SessionId, "idx_session");

            entity.HasIndex(e => new { e.UserId, e.ActivityType }, "idx_user_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityDetails)
                .HasColumnType("json")
                .HasColumnName("activity_details");
            entity.Property(e => e.ActivityType)
                .HasMaxLength(50)
                .HasComment("login, view_product, search, etc.")
                .HasColumnName("activity_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.UserAgent)
                .HasColumnType("text")
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserActivityLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_activity_logs_ibfk_1");
        });

        modelBuilder.Entity<UserInteraction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user_interactions", tb => tb.HasComment("User likes/dislikes for various content types"));

            entity.HasIndex(e => new { e.TargetType, e.TargetId }, "idx_target");

            entity.HasIndex(e => e.UserId, "idx_user");

            entity.HasIndex(e => new { e.UserId, e.TargetType, e.TargetId, e.InteractionType }, "unique_user_target_interaction").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.InteractionType)
                .HasColumnType("enum('like','dislike','helpful','unhelpful')")
                .HasColumnName("interaction_type");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasColumnType("enum('forum_post','forum_comment','blog_post','blog_comment','product_review')")
                .HasColumnName("target_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserInteractions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_interactions_ibfk_1");
        });

        modelBuilder.Entity<VendorProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("vendor_profiles", tb => tb.HasComment("Vendor/seller profile and verification details"));

            entity.HasIndex(e => e.BusinessRegistrationNumber, "business_registration_number").IsUnique();

            entity.HasIndex(e => e.CompanyName, "idx_company_name");

            entity.HasIndex(e => e.RatingAverage, "idx_rating");

            entity.HasIndex(e => e.VerifiedAt, "idx_verified");

            entity.HasIndex(e => e.UserId, "user_id").IsUnique();

            entity.HasIndex(e => e.VerifiedBy, "verified_by");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BankAccountInfo)
                .HasComment("Encrypted bank details for payments")
                .HasColumnType("json")
                .HasColumnName("bank_account_info");
            entity.Property(e => e.BusinessRegistrationNumber)
                .HasMaxLength(100)
                .HasColumnName("business_registration_number");
            entity.Property(e => e.CommissionRate)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'10.00'")
                .HasComment("Platform commission percentage")
                .HasColumnName("commission_rate");
            entity.Property(e => e.CompanyAddress)
                .HasColumnType("text")
                .HasColumnName("company_address");
            entity.Property(e => e.CompanyName).HasColumnName("company_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.RatingAverage)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("rating_average");
            entity.Property(e => e.SustainabilityCredentials)
                .HasComment("JSON array of sustainability certifications")
                .HasColumnType("json")
                .HasColumnName("sustainability_credentials");
            entity.Property(e => e.TaxCode)
                .HasMaxLength(50)
                .HasColumnName("tax_code");
            entity.Property(e => e.TotalReviews)
                .HasDefaultValueSql("'0'")
                .HasColumnName("total_reviews");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VerifiedAt)
                .HasColumnType("timestamp")
                .HasColumnName("verified_at");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");

            entity.HasOne(d => d.User).WithOne(p => p.VendorProfileUser)
                .HasForeignKey<VendorProfile>(d => d.UserId)
                .HasConstraintName("vendor_profiles_ibfk_1");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.VendorProfileVerifiedByNavigations)
                .HasForeignKey(d => d.VerifiedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("vendor_profiles_ibfk_2");
        });

        modelBuilder.Entity<WeatherDataCache>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("weather_data_cache", tb => tb.HasComment("Cached weather data from external APIs"));

            entity.HasIndex(e => new { e.FarmProfileId, e.WeatherDate }, "idx_farm_date");

            entity.HasIndex(e => e.FetchedAt, "idx_fetched");

            entity.HasIndex(e => new { e.FarmProfileId, e.WeatherDate, e.ApiSource }, "idx_weather_lookup").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApiSource)
                .HasColumnType("enum('openweathermap','accuweather')")
                .HasColumnName("api_source");
            entity.Property(e => e.FarmProfileId).HasColumnName("farm_profile_id");
            entity.Property(e => e.FetchedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fetched_at");
            entity.Property(e => e.HumidityPercentage)
                .HasPrecision(5, 2)
                .HasColumnName("humidity_percentage");
            entity.Property(e => e.PrecipitationMm)
                .HasPrecision(8, 2)
                .HasColumnName("precipitation_mm");
            entity.Property(e => e.RawApiResponse)
                .HasColumnType("json")
                .HasColumnName("raw_api_response");
            entity.Property(e => e.SunriseTime)
                .HasColumnType("time")
                .HasColumnName("sunrise_time");
            entity.Property(e => e.SunsetTime)
                .HasColumnType("time")
                .HasColumnName("sunset_time");
            entity.Property(e => e.TemperatureAvg)
                .HasPrecision(5, 2)
                .HasColumnName("temperature_avg");
            entity.Property(e => e.TemperatureMax)
                .HasPrecision(5, 2)
                .HasColumnName("temperature_max");
            entity.Property(e => e.TemperatureMin)
                .HasPrecision(5, 2)
                .HasColumnName("temperature_min");
            entity.Property(e => e.UvIndex)
                .HasPrecision(3, 1)
                .HasColumnName("uv_index");
            entity.Property(e => e.WeatherCondition)
                .HasMaxLength(100)
                .HasColumnName("weather_condition");
            entity.Property(e => e.WeatherDate).HasColumnName("weather_date");
            entity.Property(e => e.WeatherIcon)
                .HasMaxLength(50)
                .HasColumnName("weather_icon");
            entity.Property(e => e.WindDirection)
                .HasMaxLength(10)
                .HasColumnName("wind_direction");
            entity.Property(e => e.WindSpeedKmh)
                .HasPrecision(6, 2)
                .HasColumnName("wind_speed_kmh");

            entity.HasOne(d => d.FarmProfile).WithMany(p => p.WeatherDataCaches)
                .HasForeignKey(d => d.FarmProfileId)
                .HasConstraintName("weather_data_cache_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
