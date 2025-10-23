using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using DAL.Data.Models;
using System.Text.Json;

namespace DAL.Data.Configurations;

public class ProductRegistrationConfiguration : IEntityTypeConfiguration<ProductRegistration>
{
    public void Configure(EntityTypeBuilder<ProductRegistration> builder)
    {
        builder.ToTable("product_registrations");

        // PK
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Required FKs
        builder.Property(e => e.VendorId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("vendor_id")
            .IsRequired();

        builder.Property(e => e.CategoryId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("category_id")
            .IsRequired();

        // Strings
        builder.Property(e => e.ProposedProductCode)
            .HasMaxLength(100)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("proposed_product_code")
            .IsRequired();

        builder.Property(e => e.ProposedProductName)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("proposed_product_name")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("Description");

        builder.Property(e => e.UnitPrice)
            .HasPrecision(12, 2)
            .HasColumnType("decimal(12,2)")
            .HasColumnName("unit_price")
            .IsRequired();

        builder.Property(e => e.EnergyEfficiencyRating)
            .HasColumnType("int")
            .HasColumnName("energy_efficiency_rating");

        builder.Property(e => e.ManualUrls)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("manual_urls");

        builder.Property(e => e.PublicUrl)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("public_url")
            .IsRequired(false);

        builder.Property(e => e.WarrantyMonths)
            .HasColumnType("int")
            .HasColumnName("warranty_months")
            .HasDefaultValue(12);

        builder.Property(e => e.WeightKg)
            .IsRequired()
            .HasPrecision(10, 3)
            .HasColumnType("decimal(10,3)")
            .HasColumnName("weight_kg");

        // ===== JSON options & comparers =====
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Dictionary<string, object> comparer
        var dictObjectComparer = new ValueComparer<Dictionary<string, object>>(
            (d1, d2) => JsonSerializer.Serialize(d1 ?? new(), jsonOptions) == JsonSerializer.Serialize(d2 ?? new(), jsonOptions),
            d => (d == null ? 0 : JsonSerializer.Serialize(d, jsonOptions).GetHashCode()),
            d => d == null ? new() : new Dictionary<string, object>(d)
        );

        // Dictionary<string, decimal> comparer
        var dictDecimalComparer = new ValueComparer<Dictionary<string, decimal>>(
            (d1, d2) => JsonSerializer.Serialize(d1 ?? new(), jsonOptions) == JsonSerializer.Serialize(d2 ?? new(), jsonOptions),
            d => (d == null ? 0 : JsonSerializer.Serialize(d, jsonOptions).GetHashCode()),
            d => d == null ? new() : new Dictionary<string, decimal>(d)
        );

        // Specifications (json)
        builder.Property(e => e.Specifications)
            .HasColumnType("json")
            .HasColumnName("specifications")
            .HasConversion(
                v => JsonSerializer.Serialize(v ?? new Dictionary<string, object>(), jsonOptions),
                v => string.IsNullOrWhiteSpace(v)
                        ? new Dictionary<string, object>()
                        : ToObjectDictionary(JsonSerializer.Deserialize<Dictionary<string, object>>(v, jsonOptions))
            )
            .Metadata.SetValueComparer(dictObjectComparer);

        // DimensionsCm (json)
        builder.Property(e => e.DimensionsCm)
            .HasColumnType("json")
            .HasColumnName("dimensions_cm")
            .HasConversion(
                v => JsonSerializer.Serialize(v ?? new Dictionary<string, decimal>(), jsonOptions),
                v => string.IsNullOrWhiteSpace(v)
                        ? new Dictionary<string, decimal>()
                        : JsonSerializer.Deserialize<Dictionary<string, decimal>>(v, jsonOptions) ?? new()
            )
            .Metadata.SetValueComparer(dictDecimalComparer);

        // ===== Enum Status: lowercase in DB <-> enum in code =====
        var statusConverter = new ValueConverter<ProductRegistrationStatus, string>(
            v => v.ToString().ToLowerInvariant(),                      // enum -> "pending|approved|rejected"
            v => Enum.Parse<ProductRegistrationStatus>(v, true)        // "pending" -> ProductRegistrationStatus.Pending
        );

        builder.Property(e => e.Status)
            .HasConversion(statusConverter)
            .HasColumnName("status")
            .HasColumnType("enum('pending','approved','rejected')")
            // Nếu DB đã có default 'pending' thì dòng dưới không bắt buộc:
            //.HasDefaultValueSql("'pending'")
            .IsRequired();

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("rejection_reason");

        builder.Property(e => e.ApprovedBy)
            .HasColumnType("bigint unsigned")
            .HasColumnName("approved_by");

        // DateTimes
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasColumnName("UpdatedAt");

        builder.Property(e => e.ApprovedAt)
            .HasColumnType("timestamp")
            .HasColumnName("approved_at");

        // Indexes
        builder.HasIndex(e => e.VendorId).HasDatabaseName("idx_vendor");
        builder.HasIndex(e => new { e.VendorId, e.Status }).HasDatabaseName("idx_vendor_status");
        builder.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_created");

        // FKs
        builder.HasOne(e => e.Vendor)
            .WithMany()
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_product_registrations__vendor_id__vendor_profiles__id");

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_product_registrations__category_id__product_categories__id");

        builder.HasOne(e => e.ApprovedByUser)
            .WithMany()
            .HasForeignKey(e => e.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }

    // Convert JsonElement thành object .NET an toàn cho Specifications
    private static Dictionary<string, object> ToObjectDictionary(Dictionary<string, object>? raw)
    {
        if (raw == null) return new();

        object Convert(object? val)
        {
            if (val is JsonElement je)
            {
                switch (je.ValueKind)
                {
                    case JsonValueKind.Object:
                        var dict = new Dictionary<string, object>();
                        foreach (var p in je.EnumerateObject())
                            dict[p.Name] = Convert(p.Value);
                        return dict;

                    case JsonValueKind.Array:
                        var list = new List<object?>();
                        foreach (var it in je.EnumerateArray())
                            list.Add(Convert(it));
                        return list;

                    case JsonValueKind.String: return je.GetString() ?? string.Empty;
                    case JsonValueKind.Number:
                        if (je.TryGetDecimal(out var dec)) return dec;
                        if (je.TryGetDouble(out var dbl)) return dbl;
                        return je.ToString();

                    case JsonValueKind.True:
                    case JsonValueKind.False: return je.GetBoolean();
                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                    default: return null!;
                }
            }
            return val!;
        }

        var result = new Dictionary<string, object>();
        foreach (var kv in raw)
            result[kv.Key] = Convert(kv.Value);
        return result;
    }
}
