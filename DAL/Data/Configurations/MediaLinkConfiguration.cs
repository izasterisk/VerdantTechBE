using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

internal static class MediaLinkEfMaps
{
    public static string OwnerTypeToDb(MediaOwnerType v) => v switch
    {
        MediaOwnerType.VendorCertificates => "vendor_certificates",
        MediaOwnerType.ChatbotMessages => "chatbot_messages",
        MediaOwnerType.Products => "products",
        MediaOwnerType.ProductRegistrations => "product_registrations",
        MediaOwnerType.ProductCertificates => "product_certificates",
        MediaOwnerType.ProductReviews => "product_reviews",
        MediaOwnerType.ForumPosts => "forum_posts",
        MediaOwnerType.RequestMessage => "request_message",
        MediaOwnerType.ProductSnapshot => "product_snapshot",
        _ => throw new ArgumentOutOfRangeException(nameof(v), v, null)
    };

    public static MediaOwnerType OwnerTypeFromDb(string s) => s switch
    {
        "vendor_certificates" => MediaOwnerType.VendorCertificates,
        "chatbot_messages" => MediaOwnerType.ChatbotMessages,
        "products" => MediaOwnerType.Products,
        "product_registrations" => MediaOwnerType.ProductRegistrations,
        "product_certificates" => MediaOwnerType.ProductCertificates,
        "product_reviews" => MediaOwnerType.ProductReviews,
        "forum_posts" => MediaOwnerType.ForumPosts,
        "request_message" => MediaOwnerType.RequestMessage,
        "product_snapshot" => MediaOwnerType.ProductSnapshot,
        _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
    };

    public static string PurposeToDb(MediaPurpose v) => v switch
    {
        MediaPurpose.Front => "front",
        MediaPurpose.Back => "back",
        MediaPurpose.None => "none",
        MediaPurpose.ProductCertificatePdf => "productcertificatepdf",
        MediaPurpose.VendorCertificatesPdf => "vendorcertificatespdf",
        MediaPurpose.ProductImage => "productimage",
        _ => "none"
    };

    public static MediaPurpose PurposeFromDb(string s) => s.ToLowerInvariant() switch
    {
        "front" => MediaPurpose.Front,
        "back" => MediaPurpose.Back,
        "none" => MediaPurpose.None,
        "productcertificatepdf" => MediaPurpose.ProductCertificatePdf,
        "vendorcertificatespdf" => MediaPurpose.VendorCertificatesPdf,
        "productimage" => MediaPurpose.ProductImage,
        _ => MediaPurpose.None
    };
}

public class MediaLinkConfiguration : IEntityTypeConfiguration<MediaLink>
{
    public void Configure(EntityTypeBuilder<MediaLink> builder)
    {
        builder.ToTable("media_links");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        var ownerTypeConverter = new ValueConverter<MediaOwnerType, string>(
            v => MediaLinkEfMaps.OwnerTypeToDb(v),
            s => MediaLinkEfMaps.OwnerTypeFromDb(s));

        var purposeConverter = new ValueConverter<MediaPurpose, string>(
            v => MediaLinkEfMaps.PurposeToDb(v),
            s => MediaLinkEfMaps.PurposeFromDb(s));

        builder.Property(e => e.OwnerType)
            .HasConversion(ownerTypeConverter)
            .HasMaxLength(500)                 
            .HasColumnType("enum('vendor_certificates','chatbot_messages','products','product_registrations','product_certificates','product_reviews','forum_posts','request_message','product_snapshot')")
            .IsRequired()
            .HasColumnName("owner_type");

        builder.Property(e => e.OwnerId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("owner_id");

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(1024)
            .IsRequired()
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("image_url");

        builder.Property(e => e.ImagePublicId)
            .HasMaxLength(512)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("image_public_id");

        builder.Property(e => e.Purpose)
            .HasConversion(purposeConverter)
            .HasMaxLength(500)
            .HasColumnType("enum('front','back','none','productcertificatepdf','vendorcertificatesPdf','productimage')")
            .HasDefaultValue(MediaPurpose.None)
            .HasColumnName("purpose");


        builder.Property(e => e.SortOrder)
            .HasColumnType("int")
            .HasDefaultValue(0)
            .HasColumnName("sort_order");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");

        builder.HasIndex(e => new { e.OwnerType, e.OwnerId }).HasDatabaseName("idx_owner");
        builder.HasIndex(e => e.Purpose).HasDatabaseName("idx_purpose");
    }
}
