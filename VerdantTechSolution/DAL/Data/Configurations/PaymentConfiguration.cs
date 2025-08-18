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
        
        // Foreign Key
        builder.Property(e => e.OrderId)
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .HasColumnName("order_id");
        
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
        
        // Optional unique transaction ID
        builder.Property(e => e.TransactionId)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("transaction_id");
        
        // Amount fields
        builder.Property(e => e.Amount)
            .HasPrecision(12, 2)
            .IsRequired();
            
        builder.Property(e => e.RefundAmount)
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("refund_amount");
        
        // Currency field
        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(3)
            .HasDefaultValue("VND")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("currency_code");
        
        // JSON field for gateway response
        builder.Property(e => e.GatewayResponse)
            .HasConversion(
                v => v == null || v.Count == 0 ? "{}" : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) || v == "{}" ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("json")
            .HasDefaultValueSql("'{}'")
            .HasColumnName("gateway_response");
        
        // Refund reason
        builder.Property(e => e.RefundReason)
            .HasColumnType("text")
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("refund_reason");
        
        // DateTime fields
        builder.Property(e => e.RefundedAt)
            .HasColumnType("timestamp")
            .HasColumnName("refunded_at");
            
        builder.Property(e => e.PaidAt)
            .HasColumnType("timestamp")
            .HasColumnName("paid_at");
            
        builder.Property(e => e.FailedAt)
            .HasColumnType("timestamp")
            .HasColumnName("failed_at");
            
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationship
        builder.HasOne(d => d.Order)
            .WithMany(p => p.Payments)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Unique constraint on transaction_id
        builder.HasIndex(e => e.TransactionId)
            .IsUnique()
            .HasDatabaseName("idx_transaction");
        
        // Indexes
        builder.HasIndex(e => e.OrderId)
            .HasDatabaseName("idx_order");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
            
        builder.HasIndex(e => e.PaymentMethod)
            .HasDatabaseName("idx_payment_method");
    }
}
