using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Key to Transaction
        builder.Property(e => e.TransactionId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("transaction_id");
        
        // Enum conversions
        builder.Property(e => e.PaymentMethod)
            .HasConversion<string>()
            .HasColumnType("enum('credit_card','debit_card','paypal','stripe','bank_transfer','cod')")
            .IsRequired()
            .HasColumnName("payment_method");
            
        builder.Property(e => e.PaymentGateway)
            .HasConversion<string>()
            .HasColumnType("enum('stripe','paypal','vnpay','momo','manual')")
            .IsRequired()
            .HasColumnName("payment_gateway");
            
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("enum('pending','processing','completed','failed','refunded','partially_refunded')")
            .HasDefaultValue(PaymentStatus.Pending);
        
        // Gateway transaction ID
        builder.Property(e => e.GatewayTransactionId)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("gateway_transaction_id");
        
        // Amount field
        builder.Property(e => e.Amount)
            .HasPrecision(12, 2)
            .IsRequired();
        
        // JSON field for gateway response
        builder.Property(e => e.GatewayResponse)
            .HasConversion(
                v => v == null || v.Count == 0 ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "{}" ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasDefaultValueSql("'{}'")
            .HasColumnName("gateway_response");
        
        // DateTime fields
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationship
        builder.HasOne(d => d.Transaction)
            .WithMany(p => p.Payments)
            .HasForeignKey(d => d.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Indexes
        builder.HasIndex(e => e.TransactionId)
            .HasDatabaseName("idx_transaction");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
            
        builder.HasIndex(e => e.PaymentMethod)
            .HasDatabaseName("idx_payment_method");
    }
}
